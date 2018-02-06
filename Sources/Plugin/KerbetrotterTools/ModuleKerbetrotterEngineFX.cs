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
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterEngineFX : ModuleEnginesFX
    {

        /// <summary>
        /// Whether an atmosphere is needed
        /// </summary>
        [KSPField]
        public bool needsAtmosphere = false;

        /// <summary>
        ///  Whether oxygen is needed in the atmosphere
        /// </summary>
        [KSPField]
        public bool needsOxygen = false;

        /// <summary>
        /// Whether the engines needs to be in water
        /// </summary>
        [KSPField]
        public bool needsWater = false;

        string engine_state_noAtmosphere = Localizer.Format("#LOC_KERBETROTTER.engine.state.noAtmosphere");
        string engine_state_noOxygen = Localizer.Format("#LOC_KERBETROTTER.engine.state.noOxygen");
        string engine_state_noWater = Localizer.Format("#LOC_KERBETROTTER.engine.state.notInWater");

        //saves wherer this engine caused a flameout
        bool causedflameout = false;

        bool controlled = false;

        float savedThrustPercentage = 100.0f;

        /// <summary>
        /// The thrust limiter setting
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#autoLOC_6001363"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.5f, affectSymCounterparts = UI_Scene.All)]
        public float thrustLimiter = 100.0f;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Debug.Log("[KERBENGINE] Needs Water: " + needsWater);
            Debug.Log("[KERBENGINE] Needs Atmosphere: " + needsAtmosphere);
            Debug.Log("[KERBENGINE] Needs Oxygen: " + needsOxygen);

            Fields["thrustPercentage"].guiActive = false;
            Fields["thrustPercentage"].guiActiveEditor = false;
        }

        public void setControlled(bool controlled)
        {
            this.controlled = controlled;
        }

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight)
            {
                //check for atmosphere
                bool hasAtmosphere = vessel.mainBody.atmosphere;

                //check for oxygen
                bool hasOxygen = hasAtmosphere && vessel.mainBody.atmosphereContainsOxygen;

                //check for water
                bool inWater = false;
                for (int i = 0; i < thrustTransforms.Count; i++)
                {
                    if ((vessel.mainBody.ocean) && (FlightGlobals.getAltitudeAtPos(thrustTransforms[i].position) < 0.0f))
                    {
                        inWater = true;
                        break;
                    }
                }
                //if the engine is not in water disable it
                if ((EngineIgnited) && ((vessel.ctrlState.mainThrottle > 0.0f) || controlled))
                {
                    Debug.Log("[ModuleKerbetrotterEngineFX] Run Check");
                    Debug.Log("[ModuleKerbetrotterEngineFX] Needs Oxygen: " + needsOxygen + "Has Oxygen: " + hasOxygen); 

                    if (needsAtmosphere && !hasAtmosphere)
                    {
                        thrustPercentage = 0;
                        if (!flameout)
                        {
                            Debug.Log("[ModuleKerbetrotterEngineFX] Flameout Atmosphere");
                            Flameout(engine_state_noAtmosphere, false, false);
                            savedThrustPercentage = thrustLimiter;
                            causedflameout = true;
                        }
                    }
                    else if (needsOxygen && !hasOxygen)
                    {
                        thrustPercentage = 0;
                        if (!flameout)
                        {
                            Debug.Log("[ModuleKerbetrotterEngineFX] Flameout Oxygen");
                            Flameout(engine_state_noOxygen, false, false);
                            savedThrustPercentage = thrustLimiter;
                            causedflameout = true;
                        }
                    }
                    else if (needsWater && !inWater)
                    {
                        thrustPercentage = 0;
                        if (!flameout)
                        {
                            Debug.Log("[ModuleKerbetrotterEngineFX] Flameout Water");
                            Flameout(engine_state_noWater, false, false);
                            savedThrustPercentage = thrustLimiter;
                            causedflameout = true;
                        }
                    }
                    else
                    {
                        if ((flameout) && (causedflameout))
                        {
                            Debug.Log("[ModuleKerbetrotterEngineFX] UnFlameout");
                            UnFlameout();
                            causedflameout = false;
                            if (controlled)
                            {
                                thrustPercentage = savedThrustPercentage;
                            } 
                        }
                        if (!controlled)
                        {
                            thrustPercentage = thrustLimiter;
                        }
                        
                    }   
                }
                else
                {
                    if (flameout && causedflameout)
                    {
                        Debug.Log("[ModuleKerbetrotterEngineFX] UnFlameout OFF");
                        UnFlameout();
                        causedflameout = false;
                        if (controlled)
                        {
                            thrustPercentage = savedThrustPercentage;
                        }
                    }
                    if (!controlled)
                    {
                        thrustPercentage = thrustLimiter;
                    }
                }
            }
            else if (HighLogic.LoadedSceneIsEditor)
            {
                if (!controlled)
                {
                    thrustPercentage = thrustLimiter;
                }
            }
        }
    }
}
