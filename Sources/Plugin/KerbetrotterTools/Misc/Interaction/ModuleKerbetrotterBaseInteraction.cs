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
using KSP.Localization;
using System;

namespace KerbetrotterTools.Misc.Gameplay
{
    /// <summary>
    /// Base class for any interaction of the module in editor and flight
    /// </summary>
    public class ModuleKerbetrotterBaseInteraction : PartModule
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// //The switch can only be used from eva
        /// </summary>
        [KSPField]
        public bool evaOnly = false;

        /// <summary>
        /// The trait required for usage
        /// </summary>
        [KSPField]
        public string requiredClass = string.Empty;

        /// <summary>
        /// The class required for usage
        /// </summary>
        [KSPField]
        public int requiredLevel = 0;

        /// <summary>
        /// Whether the switch is available in flight
        /// </summary>
        [KSPField]
        public bool availableInFlight = true;

        /// <summary>
        /// Whether the switch is available in editor
        /// </summary>
        [KSPField]
        public bool availableInEditor = true;

        #endregion

        #region-------------------------Private Members----------------------

        //String for an error message
        protected String mActionError = String.Empty;

        #endregion

        #region----------------------------Life Cycle------------------------

        /// <summary>
        /// Start method of the module
        /// </summary>
        /// <param name="state">The start state</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            evaOnly |= !String.IsNullOrEmpty(requiredClass);
            initInFlighSwitchMessage();
        }

        #endregion

        #region--------------------------Public Methods----------------------

        /// <summary>
        /// Get whether an certain action is possible in the current situation
        /// </summary>
        /// <returns>When true, an interation is possible, else false</returns>
        protected bool actionPossible()
        {

            if (HighLogic.LoadedSceneIsEditor)
            {
                return availableInEditor;
            }
            else if (HighLogic.LoadedSceneIsFlight && availableInFlight)
            {
                //when IVA only is not set, the action is possible
                if (!evaOnly)
                {
                    return true;
                }

                //check if the current vessel is a kerbal
                Vessel vessel = FlightGlobals.ActiveVessel;
                if (vessel != null && vessel.isEVA)
                {
                    //when the trait is needed, the action is possible
                    if (String.IsNullOrEmpty(requiredClass))
                    {
                        ProtoCrewMember kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
                        return kerbal != null && kerbal.experienceLevel >= requiredLevel;
                    }
                    //else check if the trais is correct
                    else
                    {
                        ProtoCrewMember kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
                        return kerbal != null && requiredClass.Equals(kerbal.trait) && kerbal.experienceLevel >= requiredLevel;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Initialize the message to the user for errors during flight when a requirement for switching was not met
        /// </summary>
        private void initInFlighSwitchMessage()
        {
            //Get the error message for interaction
            if (evaOnly)
            {
                if (String.IsNullOrEmpty(requiredClass))
                {
                    mActionError = Localizer.GetStringByTag("#LOC_KERBETROTTER.error.switch.eva");
                }
                else if (requiredClass.Equals("Engineer"))
                {
                    if (requiredLevel == 0)
                    {
                        mActionError = Localizer.Format("#LOC_KERBETROTTER.error.switch.engineer");
                    }
                    else
                    {
                        mActionError = Localizer.Format("#LOC_KERBETROTTER.error.switch.engineer_level", requiredLevel);
                    }
                }
                else if (requiredClass.Equals("Pilot"))
                {
                    if (requiredLevel == 0)
                    {
                        mActionError = Localizer.Format("#LOC_KERBETROTTER.error.switch.pilot");
                    }
                    else
                    {
                        mActionError = Localizer.Format("#LOC_KERBETROTTER.error.switch.pilot_level", requiredLevel);
                    }
                }
                else if (requiredClass.Equals("Scientist"))
                {
                    if (requiredLevel == 0)
                    {
                        mActionError = Localizer.Format("#LOC_KERBETROTTER.error.switch.scientist");
                    }
                    else
                    {
                        mActionError = Localizer.Format("#LOC_KERBETROTTER.error.switch.scientist_level", requiredLevel);
                    }
                }
            }
        }

        #endregion
    }
}
