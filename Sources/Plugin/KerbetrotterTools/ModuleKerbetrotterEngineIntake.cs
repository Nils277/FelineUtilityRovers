using System;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterEngineIntake : PartModule
    {
        /// <summary>
        /// The transform of the thrust vector to control
        /// </summary>
        [KSPField]
        public string thrustTransformName = "thrustTransform";

        //parameters for the rotation control
        [KSPField]
        public bool needsOxygen = false;

        [KSPField]
        public bool needsAtmosphere = false;

        [KSPField]
        public bool needsSubmerged = false;

        [KSPField]
        public float amount = 1.0f;

        [KSPField]
        public string resourceName = "";

        Transform thrustTransform;

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
    }
}
