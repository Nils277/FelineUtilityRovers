/*
 * Copyright (C) 2018 Nils277 (https://github.com/Nils277)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KerbetrotterTools
{
    class KerbetrotterEngineMode : IConfigNode
    {
        //-------------------------Parameters----------------------

        //The name of the engine mode
        private string name = "Mode";

        //Fuel consumption depending on throttle
        private FloatCurve consumptionCurve = null;

        //Maximal thrust depending on velocity
        private FloatCurve velocityCurve = null;

        //Maximal Thrust depending on pressure
        private FloatCurve atmosphereThrustCurve = null;

        //ISP depending on pressure
        private FloatCurve atmosphereISPCurve = null;

        //The density of the needed fuel
        private float fuelFlow;

        //Whether the engine needs an atmosphere to operate
        private bool needsAtmosphere = false;

        //Whether the engine needs oxygen to operate
        private bool needsOxygen = false;

        //Whether the enine needs to be in water to operate
        private bool needsWater = false;

        //the threshold for the flameout
        private float flameoutThreshold = 0.1f;
        
        //The propellants needed for this module
        public List<Propellant> propellants;

        //---------------------------Internal State-----------------------

        //the maximal thrust of the engine
        private float maxThrust = 0;

        //The maximal isp of the engine
        private float maxISP = 0;

        //The minimal amout of resources to take
        private static double minAmount = 0.0002d;

        //flameout of the engine
        private bool flameout = false;

        // the propellant causing a flameout
        private Propellant flameOutResource = null;

        //the modifier for the output
        private float modifier = 1.0f;

        //---------------------------Constructors------------------------

        /// <summary>
        /// Constructor of the engine mode
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public KerbetrotterEngineMode(ConfigNode node, float maxThrust)
        {
            this.maxThrust = maxThrust;
            Load(node);
        }

        /// <summary>
        /// Save the engine config
        /// </summary>
        /// <param name="node">The config to save to</param>
        public void Save(ConfigNode node)
        {
            ConfigNode.CreateConfigFromObject(this, node);
        }

        /// <summary>
        /// Load the engine config
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public void Load(ConfigNode node)
        {
            if (node.name.Equals("MODE"))
            {
                ConfigNode.LoadObjectFromConfig(this, node);

                //load the propellants
                ConfigNode[] propellantNodes = node.GetNodes("PROPELLANT");
                propellants = new List<Propellant>();
                for (int i = 0; i < propellantNodes.Length; i++)
                {
                    Propellant p = new Propellant();
                    p.Load(propellantNodes[i]);
                    propellants.Add(p);
                }

                //load the name of the mode
                if (node.HasValue("name"))
                {
                    name = node.GetValue("name");
                }

                //load the curve for atmosphere ISP or Thrust
                if (node.HasNode("atmosphereISPCurve"))
                {
                    atmosphereISPCurve = new FloatCurve();
                    atmosphereISPCurve.Load(node.GetNode("atmosphereISPCurve"));

                    float tmpMin;
                    atmosphereISPCurve.FindMinMaxValue(out tmpMin, out maxISP);
                }
                else if (node.HasNode("atmosphereThrustCurve"))
                {
                    atmosphereThrustCurve = new FloatCurve();
                    atmosphereThrustCurve.Load(node.GetNode("atmosphereThrustCurve"));
                }
                else
                {
                    atmosphereISPCurve = new FloatCurve();
                    atmosphereISPCurve.Add(0, 0);
                    atmosphereISPCurve.Add(1, 100);
                    maxISP = 100;
                }

                //load the consumption curve
                if (node.HasNode("consumptionCurve"))
                {
                    consumptionCurve = new FloatCurve();
                    consumptionCurve.Load(node.GetNode("consumptionCurve"));
                }

                //load the velocity curve
                if (node.HasNode("velocityCurve"))
                {
                    velocityCurve = new FloatCurve();
                    velocityCurve.Load(node.GetNode("velocityCurve"));
                }

                //wheter the engine needs an atmosphere to work
                if (node.HasValue("needsAtmosphere"))
                {
                    needsAtmosphere = bool.Parse(node.GetValue("needsAtmosphere"));
                }

                //wheter the engine needs oxygen to operate
                if (node.HasValue("needsOxygen"))
                {
                    needsOxygen = bool.Parse(node.GetValue("needsOxygen"));
                }

                //wheter the engine needs to be in water to operate
                if (node.HasValue("needsWater"))
                {
                    needsWater = bool.Parse(node.GetValue("needsWater"));
                }

                //wheter the engine needs to be in water to operate
                if (node.HasValue("maxThrust"))
                {
                    try
                    {
                        maxThrust = float.Parse(node.GetValue("maxThrust"));
                    }
                    catch (Exception e)
                    {
                        maxThrust = 0;
                        Debug.LogError("[LYNX] Cannot load maxThrust for engine: " + e.Message);
                    }
                }

                //wheter the engine needs to be in water to operate
                if (node.HasValue("flameoutThreshold"))
                {
                    try
                    {
                        flameoutThreshold = float.Parse(node.GetValue("flameoutThreshold"));
                    }
                    catch (Exception e)
                    {
                        flameoutThreshold = 0.1f;
                        Debug.LogError("[LYNX] Cannot load flameoutThreshold for engine: " + e.Message);
                    }
                }
                
                //calculate the fuel flow modifier
                if (atmosphereISPCurve != null) {
                    float fuelDensity = 0.0f;
                    for (int i = 0; i < propellants.Count; i++)
                    {
                        fuelDensity += PartResourceLibrary.Instance.resourceDefinitions[propellants[i].name].density;
                    }

                    fuelFlow = (fuelDensity > 0.0f) ? maxThrust / (9.80665f * maxISP * fuelDensity) : 1;
                }
                else {
                    fuelFlow = 1.0f;
                }
            }
        }

        //--------------------------------Interface----------------------------

        /// <summary>
        /// Get the thrust output if the engine
        /// </summary>
        /// <param name="speed">The speed of the vessel</param>
        /// <param name="density">The density of the atmosphere</param>
        /// <returns>The thrust of the vessel</returns>
        public float getThrust(double speed, double density, float throttle, Part part)
        {
            modifier = throttle;

            if (atmosphereISPCurve != null)
            {
                modifier *= atmosphereISPCurve.Evaluate((float)density)/maxISP;
            }
            else if (atmosphereThrustCurve != null)
            {
                modifier *= atmosphereThrustCurve.Evaluate((float)density);
            }

            if (velocityCurve != null)
            {
                modifier *= velocityCurve.Evaluate((float)speed);
            }

            modifier *= consumeResources(throttle, part);

            return modifier * maxThrust;
        }

        /// <summary>
        /// Get the thrust modifier for this mode
        /// </summary>
        /// <returns></returns>
        public float getThrustModifier()
        {
            return modifier;
        }

        //consume resources depending on the throttle setting
        private  float consumeResources(float throttle, Part part)
        {
            //get the consumption depending on the power curve
            float consumption = consumptionCurve != null ? consumptionCurve.Evaluate(throttle) : throttle;

            //the minimal curve of consumption
            double minimalRecievedFuel = 1.0f;

            //first check the flameout resoure
            if (flameOutResource != null)
            {
                minimalRecievedFuel = consumeResource(flameOutResource, consumption, part);

                if (minimalRecievedFuel < flameoutThreshold)
                {
                    flameout = true;
                    return 0;
                }
            }

            Propellant tmpFlameoutResouce = null;

            //consume resources for all but the flameout resource
            for (int i = 0; i < propellants.Count; i++)
            {
                if (propellants[i] != flameOutResource)
                {
                    double recievedFuel = consumeResource(propellants[i], consumption, part);
                    if (recievedFuel < flameoutThreshold)
                    {
                        tmpFlameoutResouce = propellants[i];
                    }
                    minimalRecievedFuel = Math.Min(recievedFuel, minimalRecievedFuel);
                }
            }

            //check for lameout
            if (minimalRecievedFuel < flameoutThreshold)
            {
                flameOutResource = tmpFlameoutResouce;
                flameout = true;
                return 0.0f;
            }

            flameout = false;
            flameOutResource = null;
            return (float)minimalRecievedFuel;
        }
        
        private double consumeResource(Propellant propellant, float consumption, Part part)
        {
            double requestAmount = consumption * propellant.ratio * fuelFlow * TimeWarp.deltaTime;

            if (requestAmount < minAmount)
            {
                if (requestAmount <= 0f)
                {
                    requestAmount = 0f;
                }
                else
                {
                    requestAmount = minAmount;
                }
            }
            if (requestAmount > 0f)
            {
                double recievedAmount = part.RequestResource(propellant.id, requestAmount, propellant.GetFlowMode());
                return recievedAmount / requestAmount;
            }
            return 1;
        }

        //----------------------------Getter--------------------------

        /// <summary>
        /// The name of the mode
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Whether the engine needs an atmosphere to operate
        /// </summary>
        public bool NeedsAtmosphere
        {
            get
            {
                return needsAtmosphere;
            }
        }

        /// <summary>
        /// Whether the engine needs oxygen to operate
        /// </summary>
        public bool NeedsOxygen
        {
            get
            {
                return needsOxygen;
            }
        }

        /// <summary>
        /// The name of the mode
        /// </summary>
        public bool NeedsWater
        {
            get
            {
                return needsWater;
            }
        }

        /// <summary>
        /// Get wheter there is a flameout
        /// </summary>
        public bool FlameOut
        {
            get
            {
                return flameout;
            }
        }

        public string getDescription(bool showmode)
        {
            StringBuilder info = new StringBuilder();
            
            if (showmode)
            {
                info.AppendLine(Localizer.Format("#LOC_KERBETROTTER.engine.mode", name));
            }

            info.Append(Localizer.Format("#autoLOC_6001002", "", maxThrust));
            if (needsAtmosphere)
            {
                info.AppendLine(Localizer.Format("#LOC_KERBETROTTER.info.atmosphere"));
            }
            if (needsOxygen)
            {
                info.AppendLine(Localizer.Format("#LOC_KERBETROTTER.info.oxygen"));
            }
            if (needsWater)
            {
                info.AppendLine(Localizer.Format("#LOC_KERBETROTTER.info.water"));
            }

            if (propellants.Count > 0)
            {
                //info.AppendLine();
                info.Append(Localizer.Format("#autoLOC_220748"));
                for (int i = 0; i < propellants.Count; i++)
                {
                    info.Append(Localizer.Format("#autoLOC_220756", propellants[i].displayName, propellants[i].ratio));
                }
            }
            return info.ToString();
        }

        /// <summary>
        /// Struct holding the information about the consumed resources
        /// </summary>
        public struct ConsumedResources
        {
            /// <summary>
            /// The constructor of the engine mode
            /// </summary>
            /// <param name="name"></param>
            /// <param name="amount"></param>
            /// <param name="mode"></param>
            public ConsumedResources(string name, double amount, ResourceFlowMode mode)
            {
                this.name = name;
                this.amount = amount;
                this.mode = mode;
            }

            public double amount;
            public ResourceFlowMode mode;
            public string name;

        }
    }
}
