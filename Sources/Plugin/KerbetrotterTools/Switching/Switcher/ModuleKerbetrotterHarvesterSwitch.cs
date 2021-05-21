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
using KerbetrotterTools.Switching;
using KerbetrotterTools.Switching.Setups;
using KSP.Localization;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Switch for harvesters
    /// </summary>
    class ModuleKerbetrotterHarvesterSwitch : ModuleKerbetrotterSwitch<KerbetrotterHarvesterSetup>
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The name of the resource configuration used for this part
        /// </summary>
        [KSPField]
        public string deployAnimationName = string.Empty;

        #endregion

        #region-------------------------Private Members----------------------

        /// <summary>
        /// The current configuration, LEGACY! DO NOT USE
        /// </summary>
        [KSPField(isPersistant = true)]
        private string currentSetup = string.Empty;

        //The animation group of the harvester
        private ModuleResourceHarvester mHarvester;
        private ModuleAnimationGroup mHarvesterAnimation;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// When the module is started
        /// Initialize all settings and load the setups
        /// </summary>
        /// <param name="state">The state at start</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            mHarvesterAnimation = part.FindModuleImplementing<ModuleAnimationGroup>();

            //update from previous versions
            if (!string.IsNullOrEmpty(currentSetup))
            {
                for (int i = 0; i < setups.Count; i++)
                {
                    if (setups[i].ID == currentSetup)
                    {
                        selectedSetupID = currentSetup;
                        selectedSetup = i;
                        break;
                    }
                }
                currentSetup = "";
            }

            //when a harvester of that type cannot be found (e.g. string empty) set to first one or to old saved one
            if (selectedSetup == -1)
            {
                selectedSetup = 0;
                selectedSetupID = setups[selectedSetup].ID;
            }

            updateHarvester();
            updateListener(currentSetup);
        }

        #endregion

        #region---------------------------Switching--------------------------

        /// <summary>
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="selected">The ID of the new resource</param>
        protected override void updateSetup(string selected)
        {
            //switch the resource in this part
            updateHarvester();
            base.updateSetup(selected);
        }

        /// <summary>
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="selected">The ID of the new resource</param>
        public override void updateSetup(int selected)
        {
            base.updateSetup(selected);
            updateHarvester();
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        /// <summary>
        /// Enhanced class to load the setups
        /// </summary>
        /// <param name="setups"></param>
        protected override void loadSetups(ConfigNode[] configSetups)
        {
            //find all the harvesters from that module
            List<ModuleResourceHarvester> harvesters = part.FindModulesImplementing<ModuleResourceHarvester>();

            for (int i = 0; i < configSetups.Length; i++)
            {
                //only add setups that do have corresponding harvester
                KerbetrotterHarvesterSetup setup = new KerbetrotterHarvesterSetup(configSetups[i]);
                for (int j = 0; j < harvesters.Count; j++)
                {
                    if (harvesters[j].ResourceName.Equals(setup.resourceName))
                    {
                        setups.Add(setup);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Update the harvester selection
        /// </summary>
        private void updateHarvester()
        {
            if (setups.Count > 0)
            {
                List<ModuleResourceHarvester> modules = part.FindModulesImplementing<ModuleResourceHarvester>();
                for (int i = 0; i < modules.Count; ++i)
                {
                    if (modules[i].ResourceName == setups[selectedSetup].resourceName)
                    {
                        modules[i].EnableModule();
                        mHarvester = modules[i];
                    }
                    else
                    {
                        modules[i].DisableModule();
                    }
                }
            }
        }

        #endregion

        #region------------------------UI Interaction------------------------

        /// <summary>
        /// Get a warning message from the module when switching affects something
        /// </summary>
        /// <returns></returns>
        public override string getWarning()
        {
            if (HighLogic.LoadedSceneIsFlight && mHarvesterAnimation != null && mHarvesterAnimation.isDeployed)
            {
                return Localizer.Format("#LOC_KERBETROTTER.harvesterswitch.error_deployed");
            }
            return base.getWarning();
        }

        /// <summary>
        /// Get whether preparion is needed to deploy change the drills state
        /// </summary>
        /// <returns></returns>
        public override bool needsPreparation()
        {
            return !(HighLogic.LoadedSceneIsEditor) && mHarvesterAnimation != null && mHarvesterAnimation.isDeployed;
        }

        /// <summary>
        /// Get the progress of the preparation 
        /// </summary>
        /// <returns>The progress of the preparation. Any value >= 1.0 means preparation is done</returns>
        public override float preparationProgress()
        {
            if (HighLogic.LoadedSceneIsFlight && !string.IsNullOrEmpty(deployAnimationName))
            {
                if (mHarvesterAnimation.DeactivateAnimation != null)
                {
                    AnimationState state = mHarvesterAnimation.DeactivateAnimation[deployAnimationName];
                    return state ? state.normalizedTime : 1.0f;
                }
                else if (mHarvesterAnimation.DeployAnimation != null)
                {
                    AnimationState state = mHarvesterAnimation.DeployAnimation[deployAnimationName];
                    return state ? (1.0f - state.normalizedTime) : 1.0f;
                }
            }
            return 1.0f;
        }

        /// <summary>
        /// Start the preparation of the switching
        /// </summary>
        public override void startPreparation()
        {
            if (mHarvester != null && mHarvester.ModuleIsActive())
            {
                mHarvester.StopResourceConverter();
            }

            if (mHarvesterAnimation.isDeployed || mHarvesterAnimation.DeployAnimation.isPlaying)
            {
                mHarvesterAnimation.RetractModule();
            }
        }

        #endregion
    }
}
