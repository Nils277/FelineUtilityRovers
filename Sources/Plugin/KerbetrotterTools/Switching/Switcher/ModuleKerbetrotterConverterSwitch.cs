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
using KSP.Localization;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Module to switch converters
    /// </summary>
    [KSPModule("Kerbetrotter Converter Switch")]
    class ModuleKerbetrotterConverterSwitch : ModuleKerbetrotterSwitch<KerbetrotterConverterSetup>
    {
        #region-------------------------Private Members----------------------

        //Holds whether this is the first time the converter is updated
        private bool firstUpdate = true;

        //deactivated modules
        private List<ModuleResourceConverter> disabledModules = new List<ModuleResourceConverter>();

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

            //when a harvester of that type cannot be found (e.g. string empty) set to first one or to old saved one
            if (selectedSetup == -1)
            {
                selectedSetup = 0;
                activeSetup = setups[selectedSetup].ID;
            }

            updateMenuVisibility(changable);
            updateConverter();
            updateListener(activeSetup);
        }

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight && firstUpdate)
            {
                updateConverter();
                firstUpdate = false;
            }
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
            updateConverter();
            base.updateSetup(selected);
        }

        /// <summary>
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="selected">The ID of the new resource</param>
        public override void updateSetup(int selected)
        {
            base.updateSetup(selected);
            updateConverter();
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
            List<ModuleResourceConverter> converter = part.FindModulesImplementing<ModuleResourceConverter>();

            for (int i = 0; i < configSetups.Length; i++)
            {
                //only add setups that do have corresponding harvester
                KerbetrotterConverterSetup setup = new KerbetrotterConverterSetup(configSetups[i]);
                for (int j = 0; j < converter.Count; j++)
                {
                    if (setup.contains(converter[j].ConverterName))
                    {
                        setups.Add(setup);
                        break;
                    }
                }
            }
        }

        //Update the harvester
        private void updateConverter()
        {
            //disabledModules.Clear();
            if (setups.Count > 0)
            {
                List<ModuleResourceConverter> modules = part.FindModulesImplementing<ModuleResourceConverter>();
                for (int i = 0; i < modules.Count; ++i)
                {
                    if (setups[selectedSetup].contains(modules[i].ConverterName)) {
                        modules[i].EnableModule();
                    }
                    else 
                    {
                        modules[i].DisableModule();
                        //disabledModules.Add(modules[i]);
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
            if (HighLogic.LoadedSceneIsFlight)
            {
                List<ModuleResourceConverter> converters = part.FindModulesImplementing<ModuleResourceConverter>();
                foreach (ModuleResourceConverter converter in converters)
                {
                    if (converter.ModuleIsActive())
                    {
                        return Localizer.Format("#LOC_KERBETROTTER.converterswitch.error_running");
                    }
                }
            }
            return base.getWarning();
        }

        /// <summary>
        /// Get whether preparion is needed to deploy change the drills state
        /// </summary>
        /// <returns></returns>
        public override bool needsPreparation()
        {
            return false;
        }

        /// <summary>
        /// Get the progress of the preparation 
        /// </summary>
        /// <returns>The progress of the preparation. Any value >= 1.0 means preparation is done</returns>
        public override float preparationProgress()
        {
            return 1.0f;
        }

        /// <summary>
        /// Start the preparation of the switching
        /// </summary>
        public override void startPreparation()
        {
            List<ModuleResourceConverter> converters = part.FindModulesImplementing<ModuleResourceConverter>();
            foreach (ModuleResourceConverter converter in converters)
            {
                if (converter.ModuleIsActive())
                {
                    converter.StopResourceConverter();
                }
            }
        }

        #endregion
    }
}
