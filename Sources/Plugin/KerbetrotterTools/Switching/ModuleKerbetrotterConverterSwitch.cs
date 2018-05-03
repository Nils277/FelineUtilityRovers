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
        //saves which converter is used
        [KSPField(isPersistant = true)]
        public string converter = string.Empty;

        //the list of converters in the part;
        private List<ModuleResourceConverter> converters = new List<ModuleResourceConverter>();

        //The list of setups for the converters
        private List<KerbetrotterConverterSetup> setups = new List<KerbetrotterConverterSetup>();

        //holds the number of the current converter
        private int currentConfig = -1;

        /// <summary>
        /// Set the next setup for the harvester
        /// </summary>
        [KSPEvent(guiName = "Next Setup", guiActive = true, guiActiveEditor = true)]
        public void NextConverter()
        {
            if (converters == null || converters.Count < 2)
            {
                return;
            }

            currentConfig++;
            if (currentConfig >= converters.Count)
            {
                currentConfig = 0;
            }
            //updateConverters();
            updateMenuTexts();
            //updateListener();
        }

        /// <summary>
        /// Set the previous setup for the harvester
        /// </summary>
        [KSPEvent(guiName = "Prev. Setup", guiActive = true, guiActiveEditor = true)]
        public void PrevConverter()
        {
            if (converters == null || converters.Count < 2)
            {
                return;
            }

            currentConfig--;
            if (currentConfig < 0)
            {
                //currentConfig = harvesters.Count - 1;
            }
            //updateConverters();
            updateMenuTexts();
            //updateListener();
        }


        /// <summary>
        /// Start up the module and initialize it
        /// </summary>
        /// <param name="state">The start state of the module</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //Load the setups for the converter
            loadConverterSetups(part.partInfo.partConfig);

            //Load the converter of the part
            loadConverters();

            //Initialize the converters
            initConverters();
        }

        private void initConverters()
        {
            //if ()
        }

        //Update the menus
        private void updateMenuTexts()
        {
            int next = currentConfig + 1;
            next = next >= converters.Count ? 0 : next;
            //Events["NextConverter"].guiName = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.nextRes") + " " + PartResourceLibrary.Instance.resourceDefinitions[harvesters[next].Trim()].displayName;

            int prev = currentConfig - 1;
            prev = prev < 0 ? converters.Count - 1 : prev;
            //Events["PrevConverter"].guiName = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.prevRes") + " " + PartResourceLibrary.Instance.resourceDefinitions[harvesters[prev].Trim()].displayName;

            //currentConverter = PartResourceLibrary.Instance.resourceDefinitions[converters[currentConfig].Trim()].displayName;
        }

        // Load the needed propellants
        private void loadConverters()
        {
            converters.Clear();
            for (int i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i] is ModuleResourceConverter)
                {
                    ModuleResourceConverter converter = (ModuleResourceConverter)part.Modules[i];
                    if (converter.ConverterName != string.Empty)
                    {
                        converters.Add(converter);
                    }
                }
            }
        }

        //Load the profiles for the PID controller
        private void loadConverterSetups(ConfigNode node)
        {
            setups.Clear();

            ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");

            int index = part.Modules.IndexOf(this);
            if ((index != -1 && index < modules.Length) && (modules[index].GetValue("name") == moduleName))
            {
                loadConverterSetupsInternal(modules[index]);
            }
            else
            {
                Debug.Log("[KERBETROTTER] ConverterSwitch Config NOT found");
            }
        }

        // Load the needed propellants
        private void loadConverterSetupsInternal(ConfigNode node)
        {
            ConfigNode[] config = node.GetNodes("SETUP");
            for (int i = 0; i < config.Length; i++)
            {
                KerbetrotterConverterSetup profile = new KerbetrotterConverterSetup(config[i]);
                setups.Add(profile);
            }
        }
    }
}
