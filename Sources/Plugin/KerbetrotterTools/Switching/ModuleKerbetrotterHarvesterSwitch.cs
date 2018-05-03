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
    class ModuleKerbetrotterHarvesterSwitch : ModuleKerbetrotterSwitchMaster
    {
        //The displayed type of harvester
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.resourceswitch.current")]
        public string activeHarvester = string.Empty;

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

        //the list unique ids of harvesters
        //private List<int> harvesters = new List<int>();

        //The list of setups
        private List<Setup> setups = new List<Setup>();

        //cached strings for texts
        private string nextString = string.Empty;
        private string prevString = string.Empty;

        //the currently selected harvester
        private int selectedHarvester = -1;

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

            selectedHarvester++;
            if (selectedHarvester >= setups.Count)
            {
                selectedHarvester = 0;
            }
            currentSetup = setups[selectedHarvester].ID;

            updateHarvester();
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

            selectedHarvester--;
            if (selectedHarvester < 0)
            {
                selectedHarvester = setups.Count - 1;
            }
            currentSetup = setups[selectedHarvester].ID;

            updateHarvester();
            updateMenuTexts();
            updateListener(currentSetup);
        }

        //-------------------------------Life Cycle----------------------------

        /// <summary>
        /// Register the events for animation changes
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();
            GameEvents.OnAnimationGroupStateChanged.Add(OnAnimationGroupStateChanged);
        }

        /// <summary>
        /// When the module is started
        /// Initialize all settings and load the setups
        /// </summary>
        /// <param name="state">The state at start</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //cache the strings for the GUI
            nextString = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.nextRes") + " ";
            prevString = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.prevRes") + " ";

            //load the setups
            loadSetups(part.partInfo.partConfig);

            Debug.Log("[KerbetrotterTools:HarvesterSwitch] Loaded " + setups.Count + " setups");

            //find the index of the selected setup
            for (int i = 0; i < setups.Count; i++)
            {
                if (setups[i].ID == currentSetup)
                {
                    Debug.Log("[KerbetrotterTools:HarvesterSwitch] Found selected type: " + i);
                    selectedHarvester = i;
                    break;
                }
            }

            //when a harvester of that type cannot be found (e.g. string empty) set to first one or to old saved one
            if (selectedHarvester == -1)
            {
                if ((currentConfig >= 0) && (currentConfig < setups.Count))
                {
                    selectedHarvester = currentConfig;
                }
                else
                {
                    selectedHarvester = 0;
                }
                Debug.Log("[KerbetrotterTools:HarvesterSwitch] Found default type: " + selectedHarvester);
                currentSetup = setups[selectedHarvester].ID;
                
            }

            //when there is only one harvester, do not show the switches
            if (setups.Count < 2)
            {
                Fields["activeHarvester"].guiActive = false;
                Fields["activeHarvester"].guiActiveEditor = false;
                Events["NextConverter"].guiActive = false;
                Events["NextConverter"].guiActiveEditor = false;
                Events["PrevConverter"].guiActive = false;
                Events["PrevConverter"].guiActiveEditor = false;
            }
            //when there are exactly two harvesters, show only the next button
            else if (setups.Count == 2)
            {
                Events["PrevConverter"].guiActive = false;
                Events["PrevConverter"].guiActiveEditor = false;
            }

            updateMenuVisibility(changable);
            updateHarvester();
            updateMenuTexts();
            updateListener(currentSetup);
        }

        /// <summary>
        /// Remove this module from the events
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            GameEvents.OnAnimationGroupStateChanged.Remove(OnAnimationGroupStateChanged);
        }

        //Event when the state of the animation changed
        private void OnAnimationGroupStateChanged(ModuleAnimationGroup module, bool enabled)
        {
            if ((module == null) || (module.part != part))
            {
                return;
            }
            changable = !enabled;
            updateMenuVisibility(!enabled);
            updateHarvester();
        }

        //Update the harvester
        private void updateHarvester()
        {
            if (setups.Count > 0)
            {
                List<ModuleResourceHarvester> modules = part.FindModulesImplementing<ModuleResourceHarvester>();
                for (int i = 0; i < modules.Count; ++i)
                {
                    if (modules[i].ResourceName == setups[selectedHarvester].resourceName)
                    {
                        modules[i].EnableModule();
                    }

                    else
                    {
                        modules[i].DisableModule();
                    }

                }
            }
        }

        //Update the visibility of the gui
        private void updateMenuVisibility(bool visible)
        {
            bool show = visible && (setups != null) && (setups.Count > 1);

            Events["NextConverter"].guiActive = show;
            Events["PrevConverter"].guiActive = show && (setups.Count > 2);
        }

        //Update the menus
        private void updateMenuTexts()
        {
            int next = selectedHarvester + 1;
            next = next >= setups.Count ? 0 : next;
            Events["NextConverter"].guiName = nextString + PartResourceLibrary.Instance.resourceDefinitions[setups[next].resourceName.Trim()].displayName;

            int prev = selectedHarvester - 1;
            prev = prev < 0 ? setups.Count - 1 : prev;
            Events["PrevConverter"].guiName = prevString + PartResourceLibrary.Instance.resourceDefinitions[setups[prev].resourceName.Trim()].displayName;

            activeHarvester = PartResourceLibrary.Instance.resourceDefinitions[setups[selectedHarvester].resourceName.Trim()].displayName;
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
                List<ModuleResourceHarvester> harvesters = part.FindModulesImplementing<ModuleResourceHarvester>();

                ConfigNode[] propConfig = modules[index].GetNodes("SETUP");
                for (int i = 0; i < propConfig.Length; i++)
                {
                    Setup setup = new Setup(propConfig[i]);

                    //only add setups that do have corresponding harvester
                    for (int j = 0; j < harvesters.Count; j++)
                    {
                        if (harvesters[j].ResourceName == setup.resourceName)
                        {
                            setups.Add(setup);
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("[KerbetrotterTools:HarvesterSwitch] Cannot load setups");
            }
        }

        /// <summary>
        /// Class holding the resource name as well as the ID of the setup
        /// </summary>
        private class Setup
        {
            public string ID;
            public string resourceName;

            public Setup(ConfigNode node)
            {
                //load the ID of the mode
                if (node.HasValue("ID"))
                {
                    ID = node.GetValue("ID");
                }

                //load the name of the resource to load
                if (node.HasValue("ResourceName"))
                {
                    resourceName = node.GetValue("ResourceName");
                }
            }
        }
    }
}
