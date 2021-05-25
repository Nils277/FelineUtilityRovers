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

using KerbetrotterTools.Misc.Gameplay;
using KerbetrotterTools.Switching.Dialog;
using KerbetrotterTools.Switching.Setups;
using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools.Switching
{
    /// <summary>
    /// Base class for the switching functionality
    /// </summary>
    /// <typeparam name="T">The type of setup to use for switching</typeparam>
    class ModuleKerbetrotterSwitch<T> : ModuleKerbetrotterBaseInteraction where T: BaseSetup
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The resource required to reconfigure the module
        /// </summary>
        [KSPField]
        public string requiredResource = string.Empty;

        /// <summary>
        /// The required amound to reconfigure the switch
        /// </summary>
        [KSPField]
        public double requiredAmount = 0;

        /// <summary>
        /// The setup Group this switch belongs to
        /// </summary>
        [KSPField]
        public string setupGroup = "None";

        /// <summary>
        /// Whether symmetric parts are also affected
        /// </summary>
        [KSPField]
        public bool affectSymmetry = true;

        /// <summary>
        /// The title of the switching dialog
        /// </summary>
        [KSPField]
        public string switchingTitle = Localizer.Format("#autoLOC_465671");

        /// <summary>
        /// The string for switching preparation
        /// </summary>
        [KSPField]
        public string switchingProgress = Localizer.Format("#autoLOC_218513");

        /// <summary>
        /// Saves the ID of the currently selected setup
        /// </summary>
        [KSPField(isPersistant = true)]
        public string selectedSetupID = string.Empty;

        /// <summary>
        /// The displayed type of setup
        /// </summary>
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.switch.current")] //
        public string activeSetup = string.Empty;

        #endregion

        #region--------------------------Public Members----------------------

        //The delegate to listen for switches
        public delegate void OnSwitch(string setup);

        #endregion

        #region--------------------------Private members---------------------

        //The list of listeners for a switch
        private List<OnSwitch> mListener = new List<OnSwitch>();

        //The list of setups
        protected List<T> setups = new List<T>();

        //Holds whether the setups are already loaded
        private bool mConfigLoaded = false;

        //The currently selected setup
        protected int selectedSetup = 0;
        public int SelectedSetup
        {
            get{ return selectedSetup;}
        }

        //Saves whether the setup can be changed at the moment
        [KSPField(isPersistant = true)]
        protected bool changable = true;

        //String for error for insufficient resources
        private String mResourceError = String.Empty;

        //The dialog to swith a setup
        private SwitcherDialog<T> mDialog;

        #endregion

        #region----------------------------Life Cycle------------------------


        /// <summary>
        /// Get the switchable resources on load to allow the partInfo to be populated
        /// </summary>
        /// <param name="partNode"> The config node for this partmodule</param>
        public override void OnLoad(ConfigNode partNode)
        {
            base.OnLoad(partNode);

            if (!mConfigLoaded)
            {
                loadSwitchSetups();
                mConfigLoaded = true;
            }
        }

        /// <summary>
        /// Start method of the module
        /// </summary>
        /// <param name="state">The start state</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!mConfigLoaded)
            {
                loadSwitchSetups();
                mConfigLoaded = true;
            }

            mDialog = new SwitcherDialog<T>(this);

            //Find all the listener for switches of this module
            mListener.Clear();
            List<ISwitchListener> modules = part.FindModulesImplementing<ISwitchListener>();
            if (modules != null)
            {
                for (int i = 0; i < modules.Count; i++)
                {
                    if (modules[i].getSetup() == setupGroup)
                    {
                        mListener.Add(modules[i].onSwitch);
                    }
                }
            }

            //find the index of the selected setup
            selectedSetup = getIndexFromID(selectedSetupID);
            activeSetup = setups[selectedSetup].guiName;

            //when there is only one setup, do not show the switches
            if (setups.Count < 2)
            {
                Fields["activeSetup"].guiActive = false;
                Fields["activeSetup"].guiActiveEditor = false;
                Events["switchSetup"].guiActive = false;
                Events["switchSetup"].guiActiveEditor = false;
            }
            else
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    Events["switchSetup"].guiActiveEditor = availableInEditor;
                }
                else if (HighLogic.LoadedSceneIsFlight)
                {
                    Events["switchSetup"].guiActive = availableInFlight;
                    Events["switchSetup"].guiActiveUnfocused = evaOnly && availableInFlight;
                    Events["switchSetup"].externalToEVAOnly = evaOnly && availableInFlight;
                }
            }

            updateMenuVisibility(changable);
        }

        /// <summary>
        /// Set the next setup
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.switch.configure", guiActive = true, guiActiveEditor = true, externalToEVAOnly = true)]
        public void switchSetup()
        {
            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }
            if (setups == null || setups.Count < 2)
            {
                return;
            }
            mDialog.show(setups);
        }

        /// <summary>
        /// Clear all objects when this module is destroyed
        /// </summary>
        virtual public void OnDestroy()
        {
            mListener.Clear();
        }

        #endregion

        #region---------------------Hooks for child classes------------------

        /// <summary>
        /// Get a Warninn during swithcing
        /// </summary>
        /// <returns>A warning, or null of there is none</returns>
        public virtual string getWarning()
        {
            return null;
        }

        /// <summary>
        /// Called when an update is triggered.
        /// This method is meant to be implemented by inheriting classes
        /// </summary>
        /// <param name="setupIndex">the index of the new setup</param>
        public virtual void updateSetup(int setupIndex)
        {
            //deduct the costs
            if (!string.IsNullOrEmpty(requiredResource) && requiredAmount > 0)
            {
                if ("Funds".Equals(requiredResource))
                {
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        Funding.Instance.AddFunds(-requiredAmount, TransactionReasons.None);
                    }
                }
                else
                {
                    part.RequestResource(requiredResource, requiredAmount);
                }
            }

            selectedSetup = setupIndex;
            selectedSetupID = setups[selectedSetup].ID;
            updateSymmetricParts();
            updateListener(selectedSetupID);
            activeSetup = setups[selectedSetup].guiName;
        }

        /// <summary>
        /// Called when an update is triggered.
        /// This method is meant to be implemented by inheriting classes
        /// </summary>
        /// <param name="ID">The id of the new setup</param>
        protected virtual void updateSetup(string setupID)
        {
        }

        /// <summary>
        /// Method to load the setups. Should be overwritten by child classes
        /// </summary>
        /// <param name="setups">The config node array with setups</param>
        protected virtual void loadSetups(ConfigNode[] configSetups)
        {
            for (int i = 0; i < configSetups.Length; i++)
            {
                BaseSetup setup = new BaseSetup(configSetups[i]);
                setups.Add((T)setup);
            }
        }

        /// <summary>
        /// Get whether the switch needs some sort of preparation before switching
        /// </summary>
        /// <returns>When true, a preparation is needed, else false</returns>
        public virtual bool needsPreparation()
        {
            return false;
        }

        /// <summary>
        /// Get the progress of the preparation 
        /// </summary>
        /// <returns>The progress of the preparation. Any value >= 1.0 means preparation is done</returns>
        public virtual float preparationProgress()
        {
            return 1.0f;
        }

        /// <summary>
        /// Start the preparation of the switching
        /// </summary>
        public virtual void startPreparation()
        {

        }

        /// <summary>
        /// Abort the preparation of the switching
        /// </summary>
        public virtual void abortPreparation()
        {

        }

        #endregion

        #region--------------------------UI Interface------------------------

        /// <summary>
        /// Get the description of the costs for switching. Alternatively when not possible the error message
        /// </summary>
        /// <returns>The string describing the required costs or switching</returns>
        public string getCostsDescription()
        {
            if (string.IsNullOrEmpty(requiredResource) || requiredAmount <= 0)
            {
                return "";
            }
            //when funds are required
            if ("Funds".Equals(requiredResource))
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    if (Funding.Instance.Funds >= requiredAmount)
                    {
                        return "<color=#acfffc>" + Localizer.Format("#autoLOC_8100136") + ": " + (int)requiredAmount + " " + Localizer.Format("#autoLOC_6002218") + "</color>";
                    }
                    return "<color=#ff0000>" + Localizer.Format("#autoLOC_6001043", Localizer.Format("#autoLOC_6002218"), (int)Funding.Instance.Funds, (int)requiredAmount) + "</color>";
                }
                return "";
            }
            else
            {
                PartResourceDefinition def = PartResourceLibrary.Instance.GetDefinition(requiredResource);
                if (def != null)
                {
                    double possibleAmount = part.RequestResource(requiredResource, requiredAmount, true);
                    if ((requiredAmount - possibleAmount) < 0.001)
                    {
                        return "<color=#acfffc>" + Localizer.Format("#autoLOC_8100136") + ": " + requiredAmount + " " + def.displayName + "</color>";
                    }
                    return "<color=#ff0000>" + Localizer.Format("#autoLOC_6001043", def.displayName, (int)possibleAmount, (int)requiredAmount) + "</color>";
                }
            }
            return "ERROR: Invalid resource definition";
        }

        /// <summary>
        /// Get whether switching costs
        /// </summary>
        /// <returns>When true, the switching has a cost, else false</returns>
        public bool switchingCosts()
        {
            if (string.IsNullOrEmpty(requiredResource) || requiredAmount <= 0)
            {
                return false;
            }
            return !("Funds".Equals(requiredResource)) || HighLogic.CurrentGame.Mode == Game.Modes.CAREER;
        }

        /// <summary>
        /// Get the name of the setup with the specified ID
        /// </summary>
        /// <param name="Index">The index of the setup</param>
        /// <returns>The visible name of the setup</returns>
        protected virtual string getVisibleName(int num)
        {
            return setups[num].guiName;
        }

        /// <summary>
        /// Get whether switching is currently possible. E.g. if the required resources or funds are available
        /// </summary>
        /// <returns>When true, switching is possible, else false</returns>
        public bool canSwitch()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                return true;
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                if (string.IsNullOrEmpty(requiredResource) || requiredAmount <= 0)
                {
                    return true;
                }

                if ("Funds".Equals(requiredResource))
                {
                    //only applies for career mode
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        return Funding.Instance.Funds >= requiredAmount;
                    }
                    return true;
                }
                else
                {
                    return (requiredAmount - part.RequestResource(requiredResource, requiredAmount, true) < 0.001);
                }
            }
            return false;
        }

        #endregion

        #region-------------------------Helper Methods-----------------------

        /// <summary>
        /// Method to used when a symmetric part updates its setup in the editor
        /// </summary>
        /// <param name="ID">The ID of the Setup</param>
        protected void updateFromSymmetry(string ID)
        {
            selectedSetupID = ID;
            selectedSetup = getIndexFromID(selectedSetupID);
            updateSetup(selectedSetupID);
            updateListener(selectedSetupID);
        }

        /// <summary>
        /// Update the visibility of the GUI
        /// </summary>
        /// <param name="visible">Update whether the menu for switching should be shown or not</param>
        protected virtual void updateMenuVisibility(bool visible)
        {
            bool show = visible && (setups != null) && (setups.Count > 1);

            if (HighLogic.LoadedSceneIsEditor)
            {
                Events["switchSetup"].guiActiveEditor = availableInEditor & show;
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                Events["switchSetup"].guiActive = availableInFlight & show;
                if (evaOnly)
                {
                    Events["switchSetup"].guiActiveUnfocused = availableInFlight & show;
                    Events["switchSetup"].externalToEVAOnly = availableInFlight & show;
                }
            }
        }

        /// <summary>
        /// Triggers the update of all listeners to changes in the switch
        /// </summary>
        /// <param name="newSetup">The new setup of the switch</param>
        protected void updateListener(string newSetup)
        {
            for (int i = 0; i < mListener.Count; i++)
            {
                mListener[i](newSetup);
            }
        }

        /// <summary>
        /// Get the index of a setup from its ID
        /// </summary>
        /// <param name="ID">The ID of the setup</param>
        /// <returns>The index of the setup</returns>
        private int getIndexFromID(string ID)
        {
            for (int i = 0; i < setups.Count; i++)
            {
                if (setups[i].ID == ID)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// Load the setup of the switch
        /// </summary>
        protected virtual void loadSwitchSetups()
        {
            if (mConfigLoaded || part.partInfo == null)
            {
                return;
            }

            try
            {
                ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");

                int index = part.Modules.IndexOf(this);
                if (index != -1 && index < modules.Length && modules[index].GetValue("name") == moduleName)
                {
                    ConfigNode[] setupConfig = modules[index].GetNodes("SETUP");
                    loadSetups(setupConfig);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KerbetrotterTools:Switch] Unable to load switch configuration: " + e.Message);
            }
        }

        /// <summary>
        /// Update all symmetric parts in the editor if enabled
        /// </summary>
        private void updateSymmetricParts()
        {
            //switch the symmetric parts
            if ((affectSymmetry) && (HighLogic.LoadedSceneIsEditor))
            {
                for (int s = 0; s < part.symmetryCounterparts.Count; s++)
                {
                    ModuleKerbetrotterSwitch<T>[] symmetricSwitches = part.symmetryCounterparts[s].GetComponents<ModuleKerbetrotterSwitch<T>>();
                    if (symmetricSwitches != null)
                    {
                        for (int i = 0; i < symmetricSwitches.Length; i++)
                        {
                            if (symmetricSwitches[i].setupGroup == setupGroup)
                            {
                                symmetricSwitches[i].updateFromSymmetry(selectedSetupID);
                            }
                        }
                    }
                }
            }
        }
        
        #endregion
    }
}
