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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using KSP.Localization;
using KerbetrotterTools.Switching.Setups;
using KerbetrotterTools.Switching;
using static KerbetrotterTools.Switching.Setups.KerbetrotterResourceSetup;

namespace KerbetrotterTools
{
    [KSPModule("Kerbetrotter Resource Switch")]
    class ModuleKerbetrotterResourceSwitch : ModuleKerbetrotterSwitch<KerbetrotterResourceSetup>, IPartCostModifier, IPartMassModifier, IModuleInfo
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The name of the resource configuration used for this part
        /// </summary>
        [KSPField]
        public string resourceConfiguration = string.Empty;

        /// <summary>
        /// Multiplier for the amount of the resoure
        /// </summary>
        [KSPField]
        public float resourceMultiplier = 1.0f;

        /// <summary>
        /// Whether to keep the resources that are by default in the part
        /// </summary>
        [KSPField]
        public bool replaceDefaultResources = false;

        /// <summary>
        /// When set to true, fuels can only be switched when when the fuel tank is empty in flight
        /// </summary>
        [KSPField]
        public bool switchingNeedsEmptyTank = true;

        /// <summary>
        /// When set to true, the user can flush the tank to allow switching
        /// </summary>
        [KSPField]
        public bool allowToEmptyTank = true;

        /// <summary>
        /// The emitter for the particles when the fuel is vented
        /// </summary>
        [KSPField]
        public string particleEmitter = string.Empty;

        /// <summary>
        /// The id of the module
        /// </summary>
        [KSPField]
        public string moduleID = string.Empty;

        /// <summary>
        /// The fuel that is currently selected. DEPRECATED, DO NOT USE
        /// </summary>
        [KSPField(isPersistant = true)]
        public int selectedResourceID = -1;

        #endregion

        #region-------------------------Private Members----------------------

        //Saves whether the modules have been initialized
        private bool initialized = false;

        //Saves the costs of the current selected resource
        private float resourceCostsModifier = 0.0f;

        //Saves the weight of the current resources
        private float resourceMassModifier = 0.0f;

        //The list of resource that are by default in the part
        private List<KerbetrotterDefaultResource> defaultResources;

        //The emitter for the particles when venting
        KSPParticleEmitter emitter;

        //Indicator if the config is loaded
        private float mMaxFilling = 0.0f;

        //Saves wheter the resources are currently vented
        private bool dumping = false;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Initialize all the data in the OnStart method
        /// </summary>
        /// <param name="state">the Startstate of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (!initialized)
            {
                //find the confignode for this partModule. NOTE: This only works with one ModuleLynxFuelSwitch per part
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

                //get the default resources of the part
                defaultResources = parseDefaultResources(part.partInfo.partConfig);
                initialized = true;
            }
            
            //when the setup if from the older version of this switch
            if (selectedResourceID != -1)
            {
                if ((selectedResourceID >= 0) && (selectedResourceID < setups.Count))
                {
                    selectedSetupID = setups[selectedResourceID].ID;
                    selectedSetup = selectedResourceID;
                    selectedResourceID = -1;
                }
            }

            //when no resource is set at the beginning, do this now
            if (selectedSetupID == string.Empty)
            {
                selectedSetupID = setups[0].ID;
                selectedSetup = 0;
                refreshResources(selectedSetup);
            }
            else
            {
                if (checkSaveConsistency())
                {
                    resourceCostsModifier = (float)setups[selectedSetup].costModifier;
                    resourceMassModifier = (float)setups[selectedSetup].massModifier;
                }
            }

            //update the visibility of the GUI
            initGUI();
            updateGUIVisibility(part, true);

            //init all listeners with the right resource
            updateListener(selectedSetupID);
        }

        /// <summary>
        /// Update for the switchable tank
        /// </summary>
        public void Update()
        {
            //update the visibility of the GUI
            updateGUIVisibility(part, true);
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
                    KerbetrotterResourceDefinition[] resources = setups[selectedSetup].resources;

                    //iterate over all resources and vent them slowly
                    int numResources = part.Resources.Count;
                    bool notEmpty = false;
                    for (int i = 0; i < numResources; i++)
                    {
                        if (replaceDefaultResources)
                        {
                            double newAmount = part.Resources[i].amount - part.Resources[i].maxAmount * TimeWarp.deltaTime * 0.15;
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
                                    double newAmount = part.Resources[i].amount - (part.Resources[i].maxAmount - defaultAmount) * TimeWarp.deltaTime * 0.15;
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

        [KSPEvent(name = "jettisonResources", guiActive = true, guiActiveEditor = false, externalToEVAOnly = true, guiName = "#LOC_KERBETROTTER.resourceswitch.dump")]
        public void jettisonResources()
        {
            dumping = !dumping;
            updateDumpingText();
            updateEmitter();
        }

        #endregion

        #region-----------------------Resource Switching---------------------

        /// <summary>
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="manual">indicator whether this function is called manually</param>
        /// <param name="newResourceID">the id of the new resource</param>
        private void refreshResources(int newResourceID)
        {
            //switch reh resource in this part
            switchResources(part, newResourceID);

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
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="selected">The ID of the new resource</param>
        protected override void updateSetup(string selected)
        {
            //switch the resource in this part
            refreshResources(selectedSetup);
            base.updateSetup(selected);
        }

        /// <summary>
        /// Refresh the resources of the tank
        /// </summary>
        /// <param name="selected">The ID of the new resource</param>
        public override void updateSetup(int selected)
        {
            base.updateSetup(selected);
            refreshResources(selected);
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        /// <summary>
        /// Initialize the switchable resources.
        /// Also check for sanity with the resources that are currently available on the part (e.g a missing resource when a resource is removed)
        /// </summary>
        /// <param name="configNode"></param>
        protected override void loadSwitchSetups()
        {
            try
            {
                ConfigNode[] setupConfig = GameDatabase.Instance.GetConfigNodes(resourceConfiguration);
                loadSetups(setupConfig);
            }
            catch (Exception e)
            {
                Debug.LogError("[KerbetrotterTools:ResourceSwitch] Error while reading switch configuration: " + e.Message);
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Enhanced class to load the setups
        /// </summary>
        /// <param name="setups"></param>
        protected override void loadSetups(ConfigNode[] configSetups)
        {
            for (int i = 0; i < configSetups.Length; i++)
            {
                KerbetrotterResourceSetup setup = new KerbetrotterResourceSetup(configSetups[i]);
                setups.Add(setup);
            }
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
            if ((initialized) && (setups.Count > 0))
            {
                //only switch when a valid resourceID is set
                if ((newResourceID >= 0) && (newResourceID < setups.Count))
                {
                    //remove the previous resources from the part
                    if (selectedSetup != -1)
                    {
                        //when the default resources should be replaced
                        if (replaceDefaultResources)
                        {
                            currentPart.Resources.dict.Clear();
                            currentPart.SimulationResources?.dict.Clear();
                        }
                        else
                        {
                            //get the list of resources from the last setup
                            KerbetrotterResourceSetup oldResources = setups[selectedSetup];
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
                                PartResourceDefinition resourceDefinition = PartResourceLibrary.Instance.resourceDefinitions[resourcesToRemove[i].resourceName];

                                currentPart.Resources.dict.Remove(resourceDefinition.name.GetHashCode());
                                currentPart.SimulationResources?.dict.Remove(resourceDefinition.name.GetHashCode());
                            }
                        }
                    }

                    //update the new resource id
                    selectedSetup = newResourceID;
                    KerbetrotterResourceDefinition[] newResources = setups[selectedSetup].resources;

                    //update costs and weights
                    resourceCostsModifier = (float)setups[selectedSetup].costModifier;
                    resourceMassModifier = (float)setups[selectedSetup].massModifier;

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
                            PartResourceDefinition definition = PartResourceLibrary.Instance.resourceDefinitions[newResources[i].name];

                            PartResource resource = new PartResource(currentPart);
                            resource.SetInfo(definition);
                            resource.maxAmount = newResources[i].maxAmount;
                            resource.amount = amount;
                            resource.flowState = true;
                            resource.isTweakable = definition.isTweakable;
                            resource.isVisible = definition.isVisible;
                            resource.hideFlow = false;

                            currentPart.Resources.dict.Add(definition.name.GetHashCode(), resource);

                            PartResource simulationResource = new PartResource(resource);
                            simulationResource.simulationResource = true;
                            currentPart.SimulationResources?.dict.Add(definition.name.GetHashCode(), simulationResource);

                            resource.flowMode = PartResource.FlowMode.Both; //needed to fix log spam of [PartSet]: Failed to add Resource...

                            GameEvents.onPartResourceListChange.Fire(currentPart);
                        }
                    }
                }
            }
        }

        #endregion

        #region-----------------------------Helper---------------------------

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

                if ((dumping) && (!emitter.emit) && (setups[selectedSetup].animateVenting))
                {
                    emitter.emit = true;
                }
                else if (((!dumping) || (!setups[selectedSetup].animateVenting)) && (emitter.emit))
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
            if ((initialized) && (setups.Count > 0))
            {
                //when the saved index is not valid
                if ((selectedSetup < 0) || (selectedSetup >= setups.Count))
                {
                    selectedSetup = -1;

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
        /// Initialize the GUI
        /// </summary>
        private void initGUI()
        {
            if (HighLogic.LoadedSceneIsFlight && availableInFlight && allowToEmptyTank)
            {
                Events["jettisonResources"].guiActive = true;
                Events["jettisonResources"].guiActiveUnfocused = evaOnly;
                Events["jettisonResources"].externalToEVAOnly = evaOnly;
            }
            else
            {
                Events["jettisonResources"].guiActiveEditor = false;
            }
        }


        /// <summary>
        /// Update the visibility of the gui depending on state of the part and its resources
        /// </summary>
        /// <param name="currentPart">The current part</param>
        private void updateGUIVisibility(Part currentPart, bool visible)
        {
            base.updateMenuVisibility(visible);
            bool isEmpty = isTankEmpty(currentPart);
            Events["jettisonResources"].guiActive = HighLogic.LoadedSceneIsFlight && availableInFlight & allowToEmptyTank & !isEmpty;
            if (evaOnly)
            {
                Events["jettisonResources"].guiActiveUnfocused = availableInFlight && (setups != null) && (setups.Count > 1);
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
            if (selectedSetup == -1)
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
        /// Update the GUI
        /// </summary>
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

                try
                {
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
        /// Get the maximal filling of the switchable resources
        /// </summary>
        /// <returns></returns>
        private float getMaxFilling()
        {
            float maxFilling = 0.0f;
            for (int i = 0; i < part.Resources.Count; i++)
            {
                if (replaceDefaultResources)
                {
                    maxFilling = Math.Max(maxFilling, (float)(part.Resources[i].amount / part.Resources[i].maxAmount));
                }
                else if (getResourceFromList(part.Resources[i].resourceName, defaultResources) == null)
                {
                    maxFilling = Math.Max(maxFilling, (float)(part.Resources[i].amount/ part.Resources[i].maxAmount));
                }
            }
            return maxFilling;
        }

        #endregion

        #region------------------------UI Interaction------------------------

        /// <summary>
        /// Get the description shown for this resource 
        /// </summary>
        /// <returns>The description of the module</returns>
        public override string GetInfo()
        {
            StringBuilder info = new StringBuilder();

            info.AppendLine(Localizer.GetStringByTag("#LOC_KERBETROTTER.resourceswitch.switchable"));
            info.AppendLine();

            if (setups != null)
            {
                foreach (KerbetrotterResourceSetup setup in setups)
                {
                    info.Append("• " + setup.getInfo());
                }
            }
            return info.ToString();
        }

        /// <summary>
        /// Get a warning message from the module when switching affects something
        /// </summary>
        /// <returns></returns>
        public override string getWarning()
        {
            if (HighLogic.LoadedSceneIsFlight && !isTankEmpty(part))
            {
                return Localizer.Format("#LOC_KERBETROTTER.resourceswitch.error_not_empty");
            }
            return base.getWarning();
        }

        public override bool needsPreparation()
        {
            Debug.Log("Resoruce Switch needs Preparation");
            return !(HighLogic.LoadedSceneIsEditor || isTankEmpty(part));
        }

        /// <summary>
        /// Get the progress of the preparation 
        /// </summary>
        /// <returns>The progress of the preparation. Any value >= 1.0 means preparation is done</returns>
        public override float preparationProgress()
        {
            if (dumping)
            {
                return 1.0f - (getMaxFilling() / mMaxFilling);
            }
            return 1.0f;
        }

        /// <summary>
        /// Start the preparation of the switching
        /// </summary>
        public override void startPreparation()
        {
            mMaxFilling = getMaxFilling();
            if (!dumping)
            {
                jettisonResources();
            }
        }

        /// <summary>
        /// Abort the preparation of the switching
        /// </summary>
        public override void abortPreparation()
        {
            if (dumping)
            {
                jettisonResources();
            }
        }

        #endregion

        #region------------------------IPartCostModifier---------------------

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return resourceCostsModifier;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;

        }

        #endregion

        #region------------------------IPartMassModifier---------------------

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return resourceMassModifier;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        #endregion

        #region---------------------------IModuleInfo------------------------

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

        #endregion

        #region----------------------------Classes---------------------------

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

        #endregion
    }
}