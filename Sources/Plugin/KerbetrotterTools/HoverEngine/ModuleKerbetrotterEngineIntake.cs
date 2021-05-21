/*
 * Copyright (C) 2021 Nils277 (https://github.com/Nils277)
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
using System;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Intake for a resource
    /// </summary>
    class ModuleKerbetrotterEngineIntake : PartModule
    {
        #region-----------------------Module Settings------------------------

        /// <summary>
        /// The transform of the thrust vector to control
        /// </summary>
        [KSPField]
        public string thrustTransformName = "thrustTransform";

        /// <summary>
        /// When true the module requires oxygen
        /// </summary>
        [KSPField]
        public bool needsOxygen = false;

        /// <summary>
        /// When true the module requires athmosphere
        /// </summary>
        [KSPField]
        public bool needsAtmosphere = false;

        /// <summary>
        /// When true the module requires to be submerged
        /// </summary>
        [KSPField]
        public bool needsSubmerged = false;

        /// <summary>
        /// The intake amount
        /// </summary>
        [KSPField]
        public float amount = 1.0f;

        /// <summary>
        /// The name of the intake resource
        /// </summary>
        [KSPField]
        public string resourceName = "";

        #endregion

        #region-----------------------Private Members------------------------

        //Transform for the intake
        Transform thrustTransform;

        #endregion

        #region-------------------------Life Cylce---------------------------

        /// <summary>
        /// When the module starts
        /// </summary>
        /// <param name="state">The state at the start</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            thrustTransform = part.FindModelTransform(thrustTransformName.Trim());
        }

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight)
            {
                bool valid = true;

                //check for atmosphere
                if (needsAtmosphere)
                {
                    valid &= vessel.mainBody.atmosphere;
                }

                if (needsOxygen)
                {
                    valid &= vessel.mainBody.atmosphere && vessel.mainBody.atmosphereContainsOxygen;
                }

                //check for water
                if (needsSubmerged)
                {
                    valid &= (vessel.mainBody.ocean) && (thrustTransform != null) && (FlightGlobals.getAltitudeAtPos(thrustTransform.position) < 0.0f);
                }

                //when all conditions are fulfilled, produce the resource
                if (valid)
                {
                    produce(resourceName, amount);
                }
            }
        }

        #endregion

        #region------------------------Functionality-------------------------

        /// <summary>
        /// Receive nuclear fuel. 
        /// </summary>
        /// <param name="amount">The amount received</param>
        /// <returns>The amount that can be added</returns>
        public void produce(string resource, double amount)
        {
            if (!part.Resources.Contains(resource))
            {
                return;
            }

            double newAmount = Math.Min(part.Resources[resource].maxAmount - part.Resources[resource].amount, amount);
            part.Resources[resource].amount += newAmount;
            if (part.Resources[resource].amount > part.Resources[resource].maxAmount)
            {
                part.Resources[resource].amount = part.Resources[resource].maxAmount;
            }
        }

        #endregion
    }
}
