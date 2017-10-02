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
using System.Text;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterMultiResourceHarvester : ModuleResourceHarvester, IConfigurableResourceModule
    {
        //-----------------------private settings-------------------

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.resourceswitch.output")]
        private string currentResource;

        //the output resources
        private string[] outputResources = null;

        //the current harvested resource
        [KSPField(isPersistant = true)]
        private int numResource = 0;

        private List<IResourceChangeListener> listeners = new List<IResourceChangeListener>();

        //------------------------interaction--------------------

        /// <summary>
        /// Event to switch the harvested resource
        /// </summary>
        [KSPEvent(name = "nextResourceSetup", guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.resourceswitch.next")]
        public void nextResourceSetup()
        {
            //go to the next harvestable resource
            numResource = (numResource + 1) % outputResources.Length;

            //update the harvested resource
            ResourceName = outputResources[numResource].Trim();
            
            //update the text for the resources
            int nextRes = (numResource + 1) % outputResources.Length;
            Events["nextResourceSetup"].guiName = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.nextRes") + " " + PartResourceLibrary.Instance.resourceDefinitions[outputResources[nextRes].Trim()].displayName;
            currentResource = PartResourceLibrary.Instance.resourceDefinitions[outputResources[numResource].Trim()].displayName;

            //update all listeners with the new configuration
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].onResourceChanged(outputResources[numResource]);
            }
        }

        //----------------------life cycle-------------------------

        /// <summary>
        /// Get the switchable resources on load to allow the partInfo to be populated
        /// </summary>
        /// <param name="partNode"> The config node for this partmodule</param>
        public override void OnLoad(ConfigNode partNode)
        {
            base.OnLoad(partNode);

            loadResoucesInternal(partNode);

            //init all listeners with the right resource
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].onResourceChanged(outputResources[numResource]);
            }
        }


        /// <summary>
        /// The start of the module
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            loadResouces(part.partInfo.partConfig);

            if (outputResources == null) {
                //disable switcher
                Events["nextResourceSetup"].guiActiveEditor = false;
                currentResource = PartResourceLibrary.Instance.resourceDefinitions[ResourceName].displayName;
            }
            else {
                //set the display of the switcher
                if ((outputResources.Length > 1))
                {
                    //update the text for the resources
                    int nextRes = (numResource + 1) % outputResources.Length;
                    Events["nextResourceSetup"].guiName = Localizer.Format("#LOC_KERBETROTTER.resourceswitch.nextRes") + " " + PartResourceLibrary.Instance.resourceDefinitions[outputResources[nextRes].Trim()].displayName;
                }
                else
                {
                    Events["nextResourceSetup"].guiActiveEditor = false;
                    Events["nextResourceSetup"].guiActive = false;
                }
                //set the display of the current resource
                if (outputResources.Length == 0)
                {
                    currentResource = PartResourceLibrary.Instance.resourceDefinitions[ResourceName].displayName;
                }
                else
                {
                    currentResource = PartResourceLibrary.Instance.resourceDefinitions[outputResources[numResource].Trim()].displayName;
                    ResourceName = outputResources[numResource].Trim();
                }
            }
        }

        /// <summary>
        /// Update of the resource harvester
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsActivated)
            {
                Events["nextResourceSetup"].guiActiveEditor = false;
                Events["nextResourceSetup"].guiActive = false;
            }
            else
            {
                Events["nextResourceSetup"].guiActiveEditor = ((outputResources != null) && (outputResources.Length > 1));
                Events["nextResourceSetup"].guiActive = ((outputResources != null) && (outputResources.Length > 1));
            }
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
                listener.onResourceChanged(outputResources[numResource]);
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


        //------------------------------------helper-------------------------------



        // Load the switchable resources propellants
        private void loadResoucesInternal(ConfigNode node)
        {
            if (node.HasValue("SwitchableResource"))
            {
                outputResources = node.GetValues("SwitchableResource");
            }
        }

        // Load the needed propellants
        private void loadResouces(ConfigNode node)
        {
            ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");
            int index = part.Modules.IndexOf(this);
            if (index != -1 && index < modules.Length && modules[index].GetValue("name") == moduleName)
            {
                loadResoucesInternal(modules[index]);
            }
        }

        /// <summary>
        /// Get the information about the resource harvester
        /// </summary>
        /// <returns>The information about the harvester</returns>
        public override string GetInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine(ConverterName);
            string type = string.Empty;
            switch (HarvesterType)
            {
                case 0:
                    type = "#autoLOC_6004052";
                    break;
                case 1:
                    type = "#autoLOC_6004053";
                    break;
                case 2:
                    type = "#autoLOC_6004054";
                    break;
                case 3:
                    type = "#autoLOC_6004055";
                    break;
            };

            info.AppendLine(Localizer.Format("#autoLOC_259675", type, (int)(Efficiency*100.0f)));

            //the used resource
            info.AppendLine(Localizer.Format("#autoLOC_259676"));
            List<PartResourceDefinition> usedResources = GetConsumedResources();
            for (int i = 0; i < usedResources.Count; i++)
            {
                info.Append(Localizer.Format("#autoLOC_244197", usedResources[i].displayName, (eInput.Ratio * Efficiency).ToString("0.00")));
            }            

            //the produced resources
            info.AppendLine(Localizer.Format("#autoLOC_259698"));
            for (int i = 0; i < outputResources.Length; i++)
            {
                info.Append(Localizer.Format("#autoLOC_244197", PartResourceLibrary.Instance.resourceDefinitions[outputResources[i].Trim()].displayName, Efficiency.ToString("0.00")));
                if (i < (outputResources.Length-1))
                {
                    info.AppendLine(Localizer.Format("#autoLOC_230521"));
                }
            }

            return info.ToString();                   
        }
    }
}
