/*
 * Copyright (C) 2017 Nils277 (https://github.com/Nils277)
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

namespace KerbetrotterTools
{
    class ModuleKerbetrotterHarvesterSwitch : PartModule, IConfigurableResourceModule
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.resourceswitch.output")]
        public string currentConverter = string.Empty;

        //-----------------------------Private data----------------------

        //the current configuration
        [KSPField(isPersistant = true)]
        private int currentConfig = 0;

        //the current configuration
        [KSPField(isPersistant = true)]
        private bool changable = false;

        //the list of harvesters
        private List<string> harvesters = null;

        //Listener for resource changes
        private List<IResourceChangeListener> listeners = new List<IResourceChangeListener>();

        /// <summary>
        /// Set the next setup for the harvester
        /// </summary>
        [KSPEvent(guiName = "Next Setup", guiActive = true, guiActiveEditor = true)]
        public void NextConverter()
        {
            if (harvesters == null || harvesters.Count < 2) {
                return;
            }

            currentConfig++;
            if (currentConfig >= harvesters.Count)
            {
                currentConfig = 0;
            }
            updateHarvester();
            updateMenuTexts();
            updateListeners();
        }

        /// <summary>
        /// Set the previous setup for the harvester
        /// </summary>
        [KSPEvent(guiName = "Prev. Setup", guiActive = true, guiActiveEditor = true)]
        public void PrevConverter()
        {
            if (harvesters == null || harvesters.Count < 2)
            {
                return;
            }

            currentConfig--;
            if (currentConfig < 0)
            {
                currentConfig = harvesters.Count-1;
            }
            updateHarvester();
            updateMenuTexts();
            updateListeners();
        }

        //-------------------------------Life Cycle----------------------------

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            harvesters = new List<string>();

            List<ModuleResourceHarvester> modules = part.FindModulesImplementing<ModuleResourceHarvester>();
            for (int i = 0; i < modules.Count; i++)
            {
                harvesters.Add(modules[i].ResourceName);
            }

            if ((currentConfig >= harvesters.Count) || (currentConfig < 0))
            {
                currentConfig = 0;
            }

            if (harvesters.Count < 2)
            {
                Fields["currentConverter"].guiActive = false;
                Fields["currentConverter"].guiActiveEditor = false;
                Events["NextConverter"].guiActive = false;
                Events["NextConverter"].guiActiveEditor = false;
                Events["PrevConverter"].guiActive = false;
                Events["PrevConverter"].guiActiveEditor = false;
            }
            else if (harvesters.Count == 2)
            {
                Events["PrevConverter"].guiActive = false;
                Events["PrevConverter"].guiActiveEditor = false;
            }

            updateMenuVisibility(changable);
            updateHarvester();
            updateMenuTexts();
            updateListeners();

            GameEvents.OnAnimationGroupStateChanged.Add(OnAnimationGroupStateChanged);
        }

        public void OnDestroy()
        {
            GameEvents.OnAnimationGroupStateChanged.Remove(OnAnimationGroupStateChanged);
            listeners = null;
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
            List<ModuleResourceHarvester> harvester = part.FindModulesImplementing<ModuleResourceHarvester>();
            for (int i = 0; i < harvester.Count; ++i)
            {
                if (harvester[i].ResourceName == harvesters[currentConfig])
                    harvester[i].EnableModule();
                else
                    harvester[i].DisableModule();
            }
        }

        //Update the visibility of the gui
        private void updateMenuVisibility(bool visible)
        {
            bool show = visible && (harvesters != null) && (harvesters.Count > 1);

            Events["NextConverter"].guiActive = show;
            Events["PrevConverter"].guiActive = show;
        }

        //Update all the listeners
        private void updateListeners()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].onResourceChanged(harvesters[currentConfig]);
            }
        }

        //Update the menus
        private void updateMenuTexts()
        {
            int next = currentConfig + 1;
            next = next >= harvesters.Count ? 0 : next;
            Events["NextConverter"].guiName = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.nextRes") + " " + PartResourceLibrary.Instance.resourceDefinitions[harvesters[next].Trim()].displayName;

            int prev = currentConfig - 1;
            prev = prev < 0 ? harvesters.Count-1 : prev;
            Events["PrevConverter"].guiName = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.prevRes") + " " + PartResourceLibrary.Instance.resourceDefinitions[harvesters[prev].Trim()].displayName;

            currentConverter = PartResourceLibrary.Instance.resourceDefinitions[harvesters[currentConfig].Trim()].displayName;
        }

        //------------------------------------Interface-------------------------------
        /// <summary>
        /// Adds a listener for resource changes to the list
        /// </summary>
        /// <param name="listener">The new listener</param>
        public void addResourceChangeListener(IResourceChangeListener listener)
        {
            if ((listener != null) && (!listeners.Contains(listener)))
            {
                listener.onResourceChanged(harvesters[currentConfig]);
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Removes a listener for resource changes from the list
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void removeResourceChangeListener(IResourceChangeListener listener)
        {
            if ((listener != null) && (listeners.Contains(listener)))
            {
                listeners.Remove(listener);
            }
        }

    }
}
