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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace KerbetrotterTools
{
    [KSPModule("Kerbetrotter Resource Switch")]
    class ModuleKerbetrotterResourceSwitch : ModuleKerbetrotterSwitchMaster, IPartCostModifier, IPartMassModifier, IModuleInfo
    {
        [KSPField]
        public string resourceConfiguration = string.Empty; //template used for config

        [KSPField]
        public float resourceMultiplier = 1.0f; // how to change the resource amount from the template

        [KSPField]
        public bool replaceDefaultResources = false; //Whether to keep the resources that are by default in the part

        [KSPField]
        public bool switchingNeedsEmptyTank = true; //When set to true, fuels can only be switched when when the fuel tank is empty in flight

        [KSPField]
        public bool allowToEmptyTank = true; //When set to try, the user can flush the tank to allow switching

        [KSPField]
        public string textureSwitchModule = string.Empty; //When string is not empty, search for the module to switch textures

        [KSPField]
        public string modelSwitchModule = string.Empty; //When string is not empty, search for the module to switch models

        [KSPField]
        public string particleEmitter = string.Empty; //The emitter for the particles when the fuel is vented

        [KSPField]
        public string moduleID = string.Empty; //The id of the module

        //The fuel that is currently selected
        [KSPField(isPersistant = true)]
        public int selectedResourceID = -1;

        //Saves whether the modules have been initialized
        private bool initialized = false;

        //Saves the costs of the current selected resource
        private float resourceCostsModifier = 0.0f;

        //Saves the weight of the current resources
        private float resourceMassModifier = 0.0f;

        //The list of resources that can be switched in the tank
        private List<KerbetrotterSwitchableResource> switchableResources;

        //The list of resources that are by default in the part
        private List<KerbetrotterDefaultResource> defaultResources;

        //The UI that has to be updated when a resource has changed
        UIPartActionWindow tweakableUI;

        //The emitter for the particles when venting
        KSPParticleEmitter emitter;

        //Indicator if the config is loaded
        private bool configLoaded = false;

        //Saves wheter the resources are currently vented
        private bool dumping = false;



        /// <summary>
        /// Get the switchable resources on load to allow the partInfo to be populated
        /// </summary>
        /// <param name="partNode"> The config node for this partmodule</param>
        public override void OnLoad(ConfigNode partNode)
        {
            base.OnLoad(partNode);

            if (!configLoaded) {
                initSwitchableResources(partNode);
                //Debug.Log("[LYNX] OnLoad");
                configLoaded = true;
            }
        }

        /// <summary>
        /// Initialize all the data in the OnStart method
        /// </summary>
        /// <param name="state">the Startstate of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!initialized)
            {
                //find the confignode for this partModule. NOTE: This only works with 
                //one ModuleLynxFuelSwitch per part
                if (particleEmitter != string.Empty)
                {
                    Transform transform = part.FindModelTransform(particleEmitter);
                    if (transform != null)
                    {
                        emitter = transform.GetComponentInChildren<KSPParticleEmitter>();
                        if (emitter != null)
                        {
                            emitter.emit = false;
                        }
                    }
                }

                
                List<ModuleKerbetrotterResourceSwitch> modules = part.Modules.GetModules<ModuleKerbetrotterResourceSwitch>();
                ConfigNode[] modulesConfigs = part.partInfo.partConfig.GetNodes("MODULE");

                //get the index of this module
                int partModuleIndex = 0;
                for (int i = 0; i < modules.Count; i++)
                {
                    if (modules[i] == this)
                    {
                        partModuleIndex = i;
                        break;
                    }
                }

                //get the corresponding config node
                int configNodeIndex = 0;
                for (int i = 0; i < modulesConfigs.Length; i++)
                {
                    if (modulesConfigs[i].GetValue("name") == moduleName)
                    {
                        if (configNodeIndex == partModuleIndex)
                        {
                            initSwitchableResources(modulesConfigs[i]);
                            initialized = true;
                            break;
                        }
                        configNodeIndex++;
                    }
                }

                if (!initialized)
                {
                    Debug.LogError("[KerbetrotterResourceSwitch] " + moduleName + ": Cannot find valid configNode for this module)");
                }
            }

            //get the default resources of the part
            defaultResources = parseDefaultResources(part.partInfo.partConfig);

            //when no resource is set at the beginning, do this now
            if (selectedResourceID == -1)
            {
                refreshResources(0);
            }
            else
            {
                if (checkSaveConsistency())
                {
                    resourceCostsModifier = (float)switchableResources[selectedResourceID].costModifier;
                    resourceMassModifier = (float)switchableResources[selectedResourceID].massModifier;
                }
            }

            //update the visibility of the GUI
            initGUIVisibility();
            updateGUIVisibility(part);

            //update the texts of the GUI
            updateGUIText(part);

            //init all listeners with the right resource
            updateListener(switchableResources[selectedResourceID].ID);
        }

        /// <summary>
        /// Update for the switchable tank
        /// </summary>
        public void Update()
        {
            //update the visibility of the GUI
            updateGUIVisibility(part);
        }


        /// <summary>
        /// Fixed update to vent the resources when needed
        /// </summary>
        private void FixedUpdate()
        {
            if (dumping)
            {
                if (TimeWarp.CurrentRateIndex != 0)
                {
                    dumping = false;
                    updateDumpingText();
                }
                else
                {
                    KerbetrotterResourceDefinition[] resources = switchableResources[selectedResourceID].resources;

                    //iterate over all resources and vent them slowly
                    int numResources = part.Resources.Count;
                    bool notEmpty = false;
                    for (int i = 0; i < numResources; i++)
                    {
                        if (replaceDefaultResources)
                        {
                            double newAmount = part.Resources[i].amount - part.Resources[i].maxAmount * TimeWarp.deltaTime * 0.05;
                            if (newAmount <= 0.0)
                            {
                                newAmount = 0.0;
                            }
                            else
                            {
                                notEmpty = true;
                            }
                            part.Resources[i].amount = newAmount;
                        }
                        else
                        {
                            for (int j = 0; j < resources.Length; j++)
                            {
                                if (resources[j].name == part.Resources[i].resourceName)
                                {
                                    double defaultAmount = part.Resources[i].maxAmount - resources[j].maxAmount;
                                    double newAmount = part.Resources[i].amount - (part.Resources[i].maxAmount - defaultAmount) * TimeWarp.deltaTime * 0.05;
                                    if (newAmount < defaultAmount)
                                    {
                                        newAmount = defaultAmount;
                                    }
                                    else
                                    {
                                        notEmpty = true;
                                    }
                                    part.Resources[i].amount = newAmount;
                                }
                            }
                        }
                    }

                    //continue dumping when the fueltank is not empty yet
                    dumping = notEmpty;
                    updateDumpingText();
                }

                //update the particle emitter
                updateEmitter();
            }
        }

        //-------------------Editor Switches------------
        [KSPEvent(name = "jettisonResources", guiActive = true, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.resourceswitch.dump")]
        public void jettisonResources()
        {
            dumping = !dumping;
            updateDumpingText();
            updateEmitter();
        }

        [KSPEvent(name = "nextResourceSetup", guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.resourceswitch.next")]
        public void nextResourceSetup()
        {
            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            int newResourceID = selectedResourceID + 1;
            if (newResourceID >= switchableResources.Count)
            {
                newResourceID = 0;
            }
            refreshResources(newResourceID);
        }


        [KSPEvent(name = "prevResourceSetup", guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.resourceswitch.prev")]
        public void prevResourceSetup()
        {
            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            int newResourceID = selectedResourceID - 1;
            if (newResourceID < 0)
            {
                newResourceID = switchableResources.Count - 1;
            }
            refreshResources(newResourceID);
        }

        /// <summary>
        /// Set the text for dumping resources
        /// </summary>
        private void updateDumpingText()
        {
            if (dumping)
            {
                Events["jettisonResources"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.stop");
            }
            else
            {
                Events["jettisonResources"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.dump");
            }
        }

        /// <summary>
        /// Update the particle emitter for the venting
        /// </summary>
        private void updateEmitter()
        {
            if (emitter != null)
            {

                if ((dumping) && (!emitter.emit) && (switchableResources[selectedResourceID].animateVenting))
                {
                    emitter.emit = true;
                }
                else if (((!dumping) || (!switchableResources[selectedResourceID].animateVenting)) && (emitter.emit))
                {
                    emitter.emit = false;
                }
            }
        }

        /// <summary>
        /// Check whether the saves of the part are consistent with the resources in the part
        /// </summary>
        private bool checkSaveConsistency()
        {
            if ((initialized) && (switchableResources.Count > 0))
            {
                //when the saved index is not valid
                if ((selectedResourceID < 0) || (selectedResourceID >= switchableResources.Count)) {
                    selectedResourceID = -1;

                    if (replaceDefaultResources)
                    {
                        part.Resources.Clear();
                    }
                    else
                    {
                        //remove all resources that are not default resources
                        int numResources = part.Resources.Count;
                        List<PartResource> resourcesToRemove = new List<PartResource>();
                        for (int i = 0; i < numResources; i++)
                        {
                            KerbetrotterDefaultResource res = getResourceFromList(part.Resources[i].resourceName, defaultResources);

                            //when the resource is not a default resource, remove it
                            if (res == null)
                            {
                                resourcesToRemove.Add(part.Resources[i]);
                            }
                            //else change its value to the default ones
                            else
                            {
                                part.Resources[i].amount = UtilMath.Min(res.maxAmount, part.Resources[i].amount);
                                part.Resources[i].maxAmount = res.maxAmount;
                                part.Resources[i].isTweakable = res.isTweakable;
                            }
                        }
                        //remove all resources that are scheduled to be removed
                        for (int i = 0; i < resourcesToRemove.Count; i++)
                        {
                            part.RemoveResource(resourcesToRemove[i]);
                        }
                    }

                    Debug.LogWarning("[KerbetrotterResourceSwitch] " + moduleName + ": Resources of part are inconsistent with switchable resource, resetting to default!");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the description shown for this resource 
        /// </summary>
        /// <returns>The description of the module</returns>
        public override string GetInfo()
        {
            StringBuilder info = new StringBuilder();

            info.AppendLine(Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.switchable"));
            info.AppendLine();

            if (switchableResources != null)
            {
                foreach (KerbetrotterSwitchableResource switchableResource in switchableResources)
                {
                    //int count = 0;
                    info.Append("• ");

                    if (switchableResource.resources.Length > 1)
                    {
                        info.AppendLine(switchableResource.guiName);

                        foreach (KerbetrotterResourceDefinition resource in switchableResource.resources)
                        {
                            if (resource.maxAmount > 0)
                            {
                                info.Append("<color=#35DC35>");
                                info.Append("      ");
                                info.Append(resource.maxAmount.ToString("0.0"));
                                info.Append("</color>");
                                info.Append(" ");
                            }
                            info.AppendLine(resource.name);
                        }
                    }
                    else if (switchableResource.resources.Length == 1)
                    {
                        if (switchableResource.resources[0].maxAmount > 0)
                        {
                            info.Append("<color=#35DC35>");
                            info.Append(switchableResource.resources[0].maxAmount.ToString("0.0"));
                            info.Append("</color>");
                            info.Append(" ");
                        }
                        info.AppendLine(switchableResource.guiName);
                    }
                }
            }
            return info.ToString();
        }

        private void initGUIVisibility()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                Events["jettisonResources"].guiActiveEditor = false;
                Events["nextResourceSetup"].guiActiveEditor = availableInEditor;
                Events["prevResourceSetup"].guiActiveEditor = availableInEditor;
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                Events["jettisonResources"].guiActive = availableInFlight && allowToEmptyTank;
                Events["nextResourceSetup"].guiActive = availableInFlight;
                Events["prevResourceSetup"].guiActive = availableInFlight;

                Events["jettisonResources"].guiActiveUnfocused = evaOnly && availableInFlight && allowToEmptyTank;
                Events["nextResourceSetup"].guiActiveUnfocused = evaOnly && availableInFlight;
                Events["prevResourceSetup"].guiActiveUnfocused = evaOnly && availableInFlight;

                Events["jettisonResources"].externalToEVAOnly = evaOnly && availableInFlight && allowToEmptyTank;
                Events["nextResourceSetup"].externalToEVAOnly = evaOnly && availableInFlight;
                Events["prevResourceSetup"].externalToEVAOnly = evaOnly && availableInFlight;
            }
        }


        /// <summary>
        /// Update the visibility of the gui depending on state of the part and its resources
        /// </summary>
        /// <param name="currentPart">The current part</param>
        private void updateGUIVisibility(Part currentPart)
        {
            bool canChange = (switchableResources.Count > 1);

            if ((initialized) && (switchableResources.Count > 0))
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    Events["jettisonResources"].guiActiveEditor = false;
                    Events["nextResourceSetup"].guiActiveEditor = availableInEditor & canChange;
                    Events["prevResourceSetup"].guiActiveEditor = availableInEditor & canChange;
                }
                else if (HighLogic.LoadedSceneIsFlight)
                {
                    bool isEmpty = isTankEmpty(currentPart);
                    if (switchingNeedsEmptyTank)
                    {
                        Events["jettisonResources"].guiActive = availableInFlight && allowToEmptyTank && !isEmpty;
                        Events["nextResourceSetup"].guiActive = availableInFlight && isEmpty && canChange;
                        Events["prevResourceSetup"].guiActive = availableInFlight && isEmpty && canChange;

                        if (evaOnly)
                        {
                            Events["jettisonResources"].guiActiveUnfocused = availableInFlight && allowToEmptyTank && !isEmpty;
                            Events["nextResourceSetup"].guiActiveUnfocused = availableInFlight && isEmpty && canChange;
                            Events["prevResourceSetup"].guiActiveUnfocused = availableInFlight && isEmpty && canChange;
                        }
                    }
                    else
                    {
                        Events["jettisonResources"].guiActive = false;
                        Events["nextResourceSetup"].guiActive = availableInFlight & canChange;
                        Events["prevResourceSetup"].guiActive = availableInFlight & canChange;
                        
                        if (evaOnly)
                        {
                            Events["jettisonResources"].guiActiveUnfocused = false;
                            Events["nextResourceSetup"].guiActiveUnfocused = availableInFlight & canChange;
                            Events["prevResourceSetup"].guiActiveUnfocused = availableInFlight & canChange;
                        }
                    }
                }
                else
                {
                    Events["jettisonResources"].guiActive = false;
                    Events["nextResourceSetup"].guiActive = false;
                    Events["prevResourceSetup"].guiActive = false;
                    Events["jettisonResources"].guiActiveUnfocused = false;
                    Events["nextResourceSetup"].guiActiveUnfocused = false;
                    Events["prevResourceSetup"].guiActiveUnfocused = false;
                }
            }
            else 
            {
                Events["jettisonResources"].guiActive = false;
                Events["nextResourceSetup"].guiActive = false;
                Events["prevResourceSetup"].guiActive = false;
                Events["jettisonResources"].guiActiveUnfocused = false;
                Events["nextResourceSetup"].guiActiveUnfocused = false;
                Events["prevResourceSetup"].guiActiveUnfocused = false;
            }
        }




        /// <summary>
        /// Update the texts displayed in the gui
        /// </summary>
        /// <param name="currentPart"></param>
        private void updateGUIText(Part currentPart)
        {
            if ((initialized) && (switchableResources.Count > 0))
            {
                int nextTank = selectedResourceID + 1;
                if (nextTank >= switchableResources.Count)
                {
                    nextTank = 0;
                }

                int prevTank = selectedResourceID - 1;
                if (prevTank < 0)
                {
                    prevTank = switchableResources.Count - 1;
                }

                Events["nextResourceSetup"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.nextRes") + " " + switchableResources[nextTank].guiName;
                Events["prevResourceSetup"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.prevRes") + " " + switchableResources[prevTank].guiName;
            }
        }

        /// <summary>
        /// Check if the tank is empty
        /// </summary>
        /// <param name="currentPart">The current part</param>
        /// <returns>True when tank is empty, else false</returns>
        private bool isTankEmpty(Part currentPart)
        {
            //when we have an invalid id for the current resource, assume that the tank is empty
            if (selectedResourceID == -1)
            {
                return true;
            }

            int numResources = currentPart.Resources.Count;

            for (int i = 0; i < numResources; i++)
            {
                //when the current tank is not empty (more then 0.1% is in the tank), check it further
                if (currentPart.Resources[i].amount > currentPart.Resources[i].maxAmount / 1000)
                {
                    if (replaceDefaultResources)
                    {
                        return false;
                    }
                    else
                    {
                        KerbetrotterDefaultResource defaultResource = getResourceFromList(currentPart.Resources[i].resourceName, defaultResources);

                        //when this resource is also a default resource
                        if (defaultResource != null)
                        {
                            if (currentPart.Resources[i].amount > defaultResource.maxAmount)
                            {
                                return false;
                            }
                        }
                        //when this resource is not a default resource
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="manual">indicator whether this function is called manually</param>
        /// <param name="newResourceID">the id of the new resource</param>
        private void refreshResources(int newResourceID)
        {
            //switch reh resource in this part
            switchResources(part, newResourceID);

            //when we are in the editor, the symmetry parts need to be updated too!
            if (HighLogic.LoadedSceneIsEditor)
            {
                for (int s = 0; s < part.symmetryCounterparts.Count; s++)
                {
                    //switch the resource of the counterpart
                    switchResources(part.symmetryCounterparts[s], newResourceID);

                    ModuleKerbetrotterResourceSwitch symmetricSwitch = part.symmetryCounterparts[s].GetComponent<ModuleKerbetrotterResourceSwitch>();
                    if (symmetricSwitch != null)
                    {
                        symmetricSwitch.selectedResourceID = selectedResourceID;
                    }
                }
            }

            //update the texts in the gui
            updateGUIText(part);

            //Refresh the ui
            part.updateActionWindow();
        }

        /// <summary>
        /// Get the amount of resource units the default resource has left
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="maxAmount">The maximum amount allowed</param>
        /// <param name="currentPart">The current part</param>
        /// <returns>The amount of the resource</returns>
        private double getDefaultResourceAmount(string name, double maxAmount, Part currentPart)
        {
            for (int i = 0; i < currentPart.Resources.Count; i++)
            {
                if (name == currentPart.Resources[i].resourceName)
                {
                    if (currentPart.Resources[i].amount > maxAmount)
                    {
                        return maxAmount;
                    }
                    else
                    {
                        return currentPart.Resources[i].amount;
                    }
                    
                }
            }
            return 0.0f;
        }


        /// <summary>
        /// Get a resource from a list of resources when availalbe
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="resources">The list of resources</param>
        /// <returns></returns>
        private KerbetrotterDefaultResource getResourceFromList(string name, List<KerbetrotterDefaultResource> resources)
        {
            //check if we remove a default resource
            for (int j = 0; j < resources.Count; j++)
            {
                if (name == resources[j].name)
                {
                    return defaultResources[j];
                }
            }
            return null;
        }


        /// <summary>
        /// Switch the resources from the parts
        /// </summary>
        /// <param name="currentPart">The part which resources have to be switched</param>
        /// <param name="newResourceID">The ID of the new resource</param>
        private void switchResources(Part currentPart, int newResourceID)
        {
            if ((initialized) && (switchableResources.Count > 0))
            {
                //only switch when a valid resourceID is set
                if ((newResourceID >= 0) && (newResourceID < switchableResources.Count))
                {
                    //List<LynxDefaultResource> removedDefaultResources = new List<LynxDefaultResource>();

                    //remove the previous resources from the part
                    if (selectedResourceID != -1)
                    {
                        //when the default resources should be replaced
                        if (replaceDefaultResources)
                        {
                            currentPart.Resources.Clear();
                        }
                        else
                        {
                            //get the list of resources from the last setup
                            KerbetrotterSwitchableResource oldResources = switchableResources[selectedResourceID];
                            List<PartResource> resourcesToRemove = new List<PartResource>();

                            //remove all of the resources that are not default, update the default ones
                            int numResources = currentPart.Resources.Count;
                            for (int i = 0; i < numResources; i++)
                            {
                                PartResource partResource = currentPart.Resources[i];

                                KerbetrotterDefaultResource defaultResource = getResourceFromList(partResource.resourceName, defaultResources);
                                
                                //When the part containes this resource as a default resource, change its values
                                if (defaultResource != null)
                                {
                                    double amount = getDefaultResourceAmount(defaultResource.name, defaultResource.maxAmount, currentPart);
                                    partResource.amount = amount;
                                    partResource.maxAmount = defaultResource.maxAmount;
                                    partResource.isTweakable = defaultResource.isTweakable; 
                                }
                                //else add resource to remove list
                                else
                                {
                                    resourcesToRemove.Add(partResource);
                                }
                            }

                            //remove the resources that are scheduled to be removed
                            for (int i = 0; i < resourcesToRemove.Count; i++)
                            {
                                currentPart.RemoveResource(resourcesToRemove[i]);
                            }
                        }
                    }

                    //update the new resource id
                    selectedResourceID = newResourceID;
                    KerbetrotterResourceDefinition[] newResources = switchableResources[selectedResourceID].resources;

                    //update costs and weights
                    resourceCostsModifier = (float)switchableResources[selectedResourceID].costModifier;
                    resourceMassModifier = (float)switchableResources[selectedResourceID].massModifier;

                    //add all the defined resources to the part
                    for (int i = 0; i < newResources.Length; i++)
                    {
                        //Skip resources with the name Structural (why?)
                        if (newResources[i].name == "Structural")
                        {
                            continue;
                        }

                        double maxAmount = newResources[i].maxAmount;
                        double amount = 0.0;

                        //when in editor, we will set the configured amount
                        if (HighLogic.LoadedSceneIsEditor)
                        {
                            amount = newResources[i].amount;
                        }

                        //get the data of the default resource if available
                        KerbetrotterDefaultResource defaultResource = getResourceFromList(newResources[i].name, defaultResources);
                        
                        //when we have a default resource and do not replace them, update their data
                        if ((!replaceDefaultResources) && (defaultResource != null))
                        {
                            PartResource partResource = currentPart.Resources[defaultResource.name];

                            partResource.maxAmount += newResources[i].maxAmount;
                            partResource.amount += amount;
                            partResource.isTweakable = newResources[i].isTweakable;
                        }
                        //else create and add a new resource
                        else
                        {
                            ConfigNode newResourceNode = new ConfigNode("RESOURCE");
                            newResourceNode.AddValue("name", newResources[i].name);
                            newResourceNode.AddValue("maxAmount", newResources[i].maxAmount);
                            newResourceNode.AddValue("isTweakable", newResources[i].isTweakable);
                            newResourceNode.AddValue("amount", amount);

                            //when we are in the editor, fill the tank with the new amount
                            if (HighLogic.LoadedSceneIsEditor)
                            {
                                newResourceNode.AddValue("amount", newResources[i].amount);
                            }
                            //else the tank is empty
                            else
                            {
                                newResourceNode.AddValue("amount", 0.0f);
                            }

                            currentPart.AddResource(newResourceNode);
                        }
                    }
                    //inform all listeners about the switched resource
                    updateListener(switchableResources[selectedResourceID].ID);
                }
            }
        }

        private void updateUI()
        {
            UIPartActionWindow[] windows = FindObjectsOfType<UIPartActionWindow>();
            foreach (UIPartActionWindow window in windows)
            {
                if (window.part == part)
                {
                    window.ClearList();
                    window.displayDirty = true;
                    return;
                }
            }
        }


        /// <summary>
        /// Get the list of resources that are by default in this part
        /// </summary>
        /// <param name="configNode">The config node of the parts</param>
        /// <returns></returns>
        private List<KerbetrotterDefaultResource> parseDefaultResources(ConfigNode configNode)
        {
            //create the list of resourcedefinitions
            List<KerbetrotterDefaultResource> resources = new List<KerbetrotterDefaultResource>();

            ConfigNode[] nodes = part.partInfo.partConfig.GetNodes("RESOURCE");
            for (int i = 0; i < nodes.Length; i++)
            {
                string name = nodes[i].GetValue("name");
                string amount = nodes[i].GetValue("amount");
                string maxAmount = nodes[i].GetValue("maxAmount");
                string isTweakable = nodes[i].GetValue("isTweakable");

                try {
                    float famount = float.Parse(amount, CultureInfo.InvariantCulture.NumberFormat);
                    float fmaxAmount = float.Parse(maxAmount, CultureInfo.InvariantCulture.NumberFormat);

                    bool bIsWeakable = true;
                    if (isTweakable != null)
                    {
                        bIsWeakable = bool.Parse(isTweakable);
                    }
                    resources.Add(new KerbetrotterDefaultResource(name, famount, fmaxAmount, bIsWeakable));
                }
                catch
                {
                    Debug.LogError(moduleName + ": Error while parsing default resources?)");
                }
            }
            return resources;
        }


        /// <summary>
        /// Parse the config node of this partModule to get all definition for resources
        /// </summary>
        /// <param name="configNode">The config node of this part</param>
        /// <returns>List of switchable resources</returns>
        private List<KerbetrotterSwitchableResource> parseResources(ConfigNode[] resourceNodes)
        {
            //create the list of resources
            List<KerbetrotterSwitchableResource> resources = new List<KerbetrotterSwitchableResource>();

            if ((resourceNodes == null) || (resourceNodes.Length == 0))
            {
                return resources;
            }

            try
            {
                for (int i = 0; i < resourceNodes.Length; i++)
                {
                    string guiName = resourceNodes[i].GetValue("name");

                    //get the cost modifier
                    string costModifier = resourceNodes[i].GetValue("additionalCost");
                    if (string.IsNullOrEmpty(costModifier))
                    {
                        costModifier = "0.0";
                    }
                    float fCostModifier = float.Parse(costModifier, CultureInfo.InvariantCulture.NumberFormat);

                    //Get the mass modifier of this resource
                    string massModifier = resourceNodes[i].GetValue("additionalMass");
                    if (string.IsNullOrEmpty(massModifier))
                    {
                        massModifier = "0.0";
                    }
                    float fMassModifier = float.Parse(massModifier, CultureInfo.InvariantCulture.NumberFormat);

                    //Get the ID of the resource
                    string ID = "DEFAULT";
                    if (resourceNodes[i].HasValue("ID"))
                    {
                        ID = resourceNodes[i].GetValue("ID");
                    }

                    //Saves whether animating the venting is allowed
                    string animateVenting = resourceNodes[i].GetValue("animateVenting");
                    if (string.IsNullOrEmpty(animateVenting))
                    {
                        animateVenting = "true";
                    }
                    bool bAnimateVenting = (animateVenting.ToLower() == "true");

                    ConfigNode[] resourceSubNodes = resourceNodes[i].GetNodes("RESOURCE");
                    List<KerbetrotterResourceDefinition> newResources = new List<KerbetrotterResourceDefinition>();
                    //KerbetrotterResourceDefinition[] newResources = new KerbetrotterResourceDefinition[resourceSubNodes.Length];

                    //int numRes = 0;
                    for (int j = 0; j < resourceSubNodes.Length; j++)
                    {
                        string resourceName = resourceSubNodes[j].GetValue("name");

                        //check of this resource exist
                        if (PartResourceLibrary.Instance.resourceDefinitions[resourceName] != null)
                        {
                            float amount = float.Parse(resourceSubNodes[j].GetValue("amount"), CultureInfo.InvariantCulture.NumberFormat);
                            float maxAmount = float.Parse(resourceSubNodes[j].GetValue("maxAmount"), CultureInfo.InvariantCulture.NumberFormat);

                            string isTweakable = resourceNodes[i].GetValue("isTweakable");
                            if (string.IsNullOrEmpty(isTweakable))
                            {
                                isTweakable = "true";
                            }
                            bool bIsWeakable = bool.Parse(isTweakable);


                            fCostModifier += maxAmount * PartResourceLibrary.Instance.resourceDefinitions[resourceName].unitCost;
                            newResources.Add(new KerbetrotterResourceDefinition(resourceName, amount * resourceMultiplier, maxAmount * resourceMultiplier, bIsWeakable));
                        }
                    }

                    if (newResources.Count > 0)
                    {
                        //add the resource to the list of switchable resources
                        resources.Add(new KerbetrotterSwitchableResource(guiName, ID, fMassModifier * resourceMultiplier, fCostModifier * resourceMultiplier, bAnimateVenting, newResources));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("[KerbetrotterTools]Config node Exception: " + e.Message);
                resources.Clear();
            }
            return resources;
        }

        /// <summary>
        /// Initialize the switchable resources.
        /// Also check for sanity with the resources that are currently available on the part (e.g a missing resource when a resource is removed)
        /// </summary>
        /// <param name="configNode"></param>
        private void initSwitchableResources(ConfigNode configNode)
        {
            ConfigNode[] resourceNodes = null;

            //try to get the config node
            try
            {
                resourceNodes = GameDatabase.Instance.GetConfigNodes(resourceConfiguration);
            }
            catch (Exception e)
            {
                Debug.Log("[KerbetrotterTools]No resource configs found: " + e.Message);
            }

            switchableResources = parseResources(resourceNodes);
        }


        //----------------------------Private classes----------------------------

        /// <summary>
        /// Class that holds all the data for a switchable resource, including mass modifier and cost modifier
        /// </summary>
        public class KerbetrotterSwitchableResource
        {
            public string guiName;
            public string ID;
            public double massModifier;
            public double costModifier;
            public bool animateVenting;
            public KerbetrotterResourceDefinition[] resources;

            public KerbetrotterSwitchableResource(string guiName, string ID, double massModifier, double costModifier, bool animateVenting, List<KerbetrotterResourceDefinition> resources)
            {
                this.guiName = guiName;
                this.ID = ID;

                this.resources = new KerbetrotterResourceDefinition[resources.Count];
                for (int i = 0; i < resources.Count; i++)
                {
                    this.resources[i] = resources[i];
                }
                this.massModifier = massModifier;
                this.costModifier = costModifier;
                this.animateVenting = animateVenting;
            }
        }

        /// <summary>
        /// Class that holds the definition for default resources.
        /// </summary>
        public class KerbetrotterDefaultResource
        {
            public string name;
            public double amount;
            public double maxAmount;
            public bool isTweakable;
            
            public KerbetrotterDefaultResource(string name, double amount, double maxAmount, bool isTweakable)
            {
                this.name = name;
                this.amount = amount;
                this.maxAmount = maxAmount;
                this.isTweakable = isTweakable;
            }
        }

        /// <summary>
        /// Class that holds the definition for one resource.
        /// </summary>
        public class KerbetrotterResourceDefinition
        {
            public string name;
            public double amount;
            public double maxAmount;
            public bool isTweakable;

            public KerbetrotterResourceDefinition(string name, double amount, double maxAmount, bool isTweakable)
            {
                this.name = name;
                this.amount = amount;
                this.maxAmount = maxAmount;
                this.isTweakable = isTweakable;
            }
        }

        //--------------------------Interface for the cost modifier-----------------------

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return resourceCostsModifier;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;

        }

        //--------------------------Interface for the mass modifier-----------------------

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return resourceMassModifier;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        //--------------------------Interface for the module info-----------------------
        public string GetModuleTitle()
        {
            return Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.name");
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetPrimaryField()
        {
            return null;
        }
    }
}