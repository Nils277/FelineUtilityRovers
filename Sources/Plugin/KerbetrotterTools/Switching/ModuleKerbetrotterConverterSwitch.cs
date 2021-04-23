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
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    [KSPModule("Kerbetrotter Converter Switch")]
    class ModuleKerbetrotterConverterSwitch : ModuleKerbetrotterSwitchMaster
    {
        //The displayed type of harvester
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.converter.current")]
        public string activeConverter = string.Empty;

        //The list of setups for the converters
        private List<KerbetrotterConverterSetup> setups = new List<KerbetrotterConverterSetup>();

        //-----------------------------Private data----------------------

        //the current configuration
        [KSPField(isPersistant = true)]
        private int currentConfig = -1;

        //the current configuration
        [KSPField(isPersistant = true)]
        private string currentSetup = string.Empty;

        //the current configuration
        [KSPField(isPersistant = true)]
        private bool changable = false;

        //cached strings for texts
        private string nextString = Localizer.Format("#LOC_KERBETROTTER.converter.nextRes") + " ";
        private string prevString = Localizer.Format("#LOC_KERBETROTTER.converter.prevRes") + " ";
        private string runningString = Localizer.Format("#LOC_KERBETROTTER.error.switch.running");

        //the currently selected harvester
        private int selectedConverter = -1;
        private bool firstUpdate = true;

        //deactivated modules
        private List<ModuleResourceConverter> disabledModules = new List<ModuleResourceConverter>();

        //-------------------------------Life Cycle----------------------------

        /// <summary>
        /// When the module is started
        /// Initialize all settings and load the setups
        /// </summary>
        /// <param name="state">The state at start</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //load the setups
            loadSetups(part.partInfo.partConfig);

            //Debug.Log("[KerbetrotterTools:ConverterSwitch] Loaded " + setups.Count + " setups");

            //find the index of the selected setup
            for (int i = 0; i < setups.Count; i++)
            {
                if (setups[i].ID == currentSetup)
                {
                    //Debug.Log("[KerbetrotterTools:ConverterSwitch] Found selected type: " + i);
                    selectedConverter = i;
                    break;
                }
            }

            //when a harvester of that type cannot be found (e.g. string empty) set to first one or to old saved one
            if (selectedConverter == -1)
            {
                if ((currentConfig >= 0) && (currentConfig < setups.Count))
                {
                    selectedConverter = currentConfig;
                }
                else
                {
                    selectedConverter = 0;
                }
                //Debug.Log("[KerbetrotterTools:ConverterSwitch] Found default type: " + selectedConverter);
                currentSetup = setups[selectedConverter].ID;

            }

            //when there is only one harvester, do not show the switches
            if (setups.Count < 2)
            {
                Fields["activeConverter"].guiActive = false;
                Fields["activeConverter"].guiActiveEditor = false;
                Events["NextConverter"].guiActive = false;
                Events["NextConverter"].guiActiveEditor = false;
                Events["PrevConverter"].guiActive = false;
                Events["PrevConverter"].guiActiveEditor = false;
                //Debug.Log("[KerbetrotterTools:ConverterSwitch] Not enough setups, all disabled");
            }
            //when there are exactly two harvesters, show only the next button
            else
            {
                if (setups.Count == 2)
                {
                    Events["PrevConverter"].guiActive = false;
                    Events["PrevConverter"].guiActiveEditor = false;
                    //Debug.Log("[KerbetrotterTools:ConverterSwitch] 2 setup, prev disabled");
                }
                else
                {
                    Events["PrevConverter"].guiActive = evaOnly;
                    Events["PrevConverter"].guiActiveEditor = evaOnly;
                }
                //Debug.Log("[KerbetrotterTools:ConverterSwitch] Enabling: " + evaOnly);
                Events["NextConverter"].externalToEVAOnly = evaOnly;
                Events["NextConverter"].guiActiveUnfocused = evaOnly;
            }

            updateMenuVisibility(changable);
            updateConverter();
            updateMenuTexts();
            updateListener(currentSetup);
        }

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight && firstUpdate)
            {
                /*for (int i = 0; i < disabledModules.Count; i++) {
                    if (disabledModules[i].isEnabled)
                    {
                        disabledModules[i].DisableModule();
                    }
                }*/

                updateConverter();
                firstUpdate = false;
            }
        }

        /// <summary>
        /// Set the next setup for the harvester
        /// </summary>
        [KSPEvent(guiName = "Next Setup", guiActive = true, guiActiveEditor = true)]
        public void NextConverter()
        {
            if (setups == null || setups.Count < 2)
            {
                return;
            }

            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            if (!checkChangePossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(runningString, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            selectedConverter++;
            if (selectedConverter >= setups.Count)
            {
                selectedConverter = 0;
            }
            currentSetup = setups[selectedConverter].ID;

            updateConverter();
            updateMenuTexts();
            updateListener(currentSetup);
        }

        /// <summary>
        /// Set the previous setup for the harvester
        /// </summary>
        [KSPEvent(guiName = "Prev. Setup", guiActive = true, guiActiveEditor = true)]
        public void PrevConverter()
        {
            if (setups == null || setups.Count < 2)
            {
                return;
            }

            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            if (!checkChangePossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(runningString, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }


            selectedConverter--;
            if (selectedConverter < 0)
            {
                selectedConverter = setups.Count - 1;
            }
            currentSetup = setups[selectedConverter].ID;

            updateConverter();
            updateMenuTexts();
            updateListener(currentSetup);
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
                    if (setups[selectedConverter].contains(modules[i].ConverterName)) {
                        //Debug.Log("[KerbetrotterTools:ConverterSwitch] ENABLING module: " + modules[i].ConverterName);
                        modules[i].EnableModule();
                    }
                    else 
                    {
                        //Debug.Log("[KerbetrotterTools:ConverterSwitch] DISABLING module: " + modules[i].ConverterName);
                        modules[i].DisableModule();
                        //disabledModules.Add(modules[i]);
                    }
                }
            }
        }

        private bool checkChangePossible()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                return true;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                List<ModuleResourceConverter> modules = part.FindModulesImplementing<ModuleResourceConverter>();
                for (int i = 0; i < modules.Count; ++i)
                {
                    if (modules[i].IsActivated)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        //Update the visibility of the gui
        private void updateMenuVisibility(bool visible)
        {
            bool show = visible && (setups != null) && (setups.Count > 1);
            //Debug.Log("[KerbetrotterTools:ConverterSwitch] updateMenuVisibility: " + visible + " " + show);
            Events["NextConverter"].guiActive = show;
            Events["PrevConverter"].guiActive = show && (setups.Count > 2);
        }

        //Update the menus
        private void updateMenuTexts()
        {
            int next = selectedConverter + 1;
            next = next >= setups.Count ? 0 : next;
            Events["NextConverter"].guiName = nextString + setups[next].GuiName;

            int prev = selectedConverter - 1;
            prev = prev < 0 ? setups.Count - 1 : prev;
            Events["PrevConverter"].guiName = setups[prev].GuiName;

            activeConverter = setups[selectedConverter].GuiName;
        }

        //Load the setups from the config nodes
        private void loadSetups(ConfigNode node)
        {
            setups.Clear();
            ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");
            int index = part.Modules.IndexOf(this);

            if (index != -1 && index < modules.Length && modules[index].GetValue("name") == moduleName)
            {
                //find all the harvesters from that module
                List<ModuleResourceConverter> converter = part.FindModulesImplementing<ModuleResourceConverter>();

                ConfigNode[] config = modules[index].GetNodes("SETUP");
                for (int i = 0; i < config.Length; i++)
                {
                    KerbetrotterConverterSetup setup = new KerbetrotterConverterSetup(config[i]);
                    //only add setups that do have corresponding harvester
                    for (int j = 0; j < converter.Count; j++)
                    {
                        if (setup.contains(converter[j].ConverterName))
                        {
                            //Debug.Log("[KerbetrotterTools:ConverterSwitch] added setup: " + setup.GuiName);
                            setups.Add(setup);
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("[KerbetrotterTools:ConverterSwitch] Cannot load setups");
            }
        }
    }
}
