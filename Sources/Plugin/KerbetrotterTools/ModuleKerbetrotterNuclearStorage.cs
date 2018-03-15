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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterNuclearStorage : PartModule
    {
        //-------------------------Public settings------------------------

        /// <summary>
        /// The name of the resource that is used as fuel
        /// </summary>
        [KSPField]
        public string fuelResource = string.Empty;

        /// <summary>
        /// The minimal level an engineer must have to transfer the fuel
        /// </summary>
        [KSPField]
        public int minLevelFuelTransfer = 1;

        /// <summary>
        /// The name of the resource that is used as waste
        /// </summary>
        [KSPField]
        public string wasteResource = string.Empty;

        /// <summary>
        /// The minimal level an engineer must have to transfer the waste
        /// </summary>
        [KSPField]
        public int minLevelWasteTransfer = 3;

        /// <summary>
        /// The skill needed for transfer
        /// </summary>
        [KSPField]
        public string neededSkill = Localizer.Format("#autoLOC_500103");

        /// <summary>
        /// The speed of the transfer
        /// </summary>
        [KSPField]
        public float transferRate = 35.0f;

        //------------------------Private state------------------------

        //The visible state of the stransfer for the user
        [KSPField(guiActive = true, guiName = "#LOC_KERBETROTTER.nuclearfuel.transferstate")]
        private string status = string.Empty;

        //The default highlight color for the parts
        private Color defaultColor;

        //The highlight color of the parts for transfer
        private Color transferColor;

        //The current state of the module
        private Transferstate state = Transferstate.IDLE;

        //The index of the targe to transfer the resources to 
        private int targetIndex = -1;

        //List of parts to transfer to
        private List<Part> transferCandidates = new List<Part>();

        //The part to transfer fuel to
        private Part transferTarget = null;

        //saves whether the part can store nuclear waste
        private bool hasWasteStorage = false;

        //saves whether the part con store nuclear fuel
        private bool hasFuelStorage = false;

        //List of resource converter
        private List<BaseConverter> converter = null;

        //--------------------------Interaction-----------------------

        /// <summary>
        /// Init the transfer of the nuclear fuel
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.fuel.transfer", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void initTransferFuel()
        {
            if (!checkCrew(minLevelFuelTransfer))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.skill", minLevelFuelTransfer, neededSkill, PartResourceLibrary.Instance.GetDefinition(fuelResource).displayName), 2f, ScreenMessageStyle.UPPER_CENTER));
            }
            else if (!isSave())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.running"), 2f, ScreenMessageStyle.UPPER_CENTER));
            }
            else if (!hasResource(fuelResource))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.noFuel", PartResourceLibrary.Instance.GetDefinition(fuelResource).displayName), 2f, ScreenMessageStyle.UPPER_CENTER));
            }
            else
            {
                transferCandidates = getTransferCandidates(fuelResource);

                if ((transferCandidates != null) && (transferCandidates.Count > 0)) {
                    state = Transferstate.SELECTING_FUEL;
                    status = Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.status.selecting", PartResourceLibrary.Instance.GetDefinition(fuelResource).displayName);
                    nextTarget();
                    updateUI();
                }
                else
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.noTarget", PartResourceLibrary.Instance.GetDefinition(fuelResource).displayName), 2f, ScreenMessageStyle.UPPER_CENTER));
                }
            }

        }

        /// <summary>
        /// Init of the transfer of the nuclear waste
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.fuel.transfer", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void initTransferWaste()
        {
            if (!checkCrew(minLevelWasteTransfer))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.skill", minLevelWasteTransfer, neededSkill, PartResourceLibrary.Instance.GetDefinition(wasteResource).displayName), 2f, ScreenMessageStyle.UPPER_CENTER));
            }
            else if (!isSave())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.running"), 2f, ScreenMessageStyle.UPPER_CENTER));
            }
            else if (!hasResource(wasteResource))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.noFuel", PartResourceLibrary.Instance.GetDefinition(wasteResource).displayName), 2f, ScreenMessageStyle.UPPER_CENTER));
            }
            else
            {
                transferCandidates = getTransferCandidates(wasteResource);

                if ((transferCandidates != null) && (transferCandidates.Count > 0))
                {
                    state = Transferstate.SELECTING_WASTE;
                    status = Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.status.selecting", PartResourceLibrary.Instance.GetDefinition(wasteResource).displayName);
                    nextTarget();
                    updateUI();
                }
                else
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.error.noTarget", PartResourceLibrary.Instance.GetDefinition(wasteResource).displayName), 2f, ScreenMessageStyle.UPPER_CENTER));
                }
            }
        }

        /// <summary>
        /// Set the next target for the transfer
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.target.next", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void nextTarget()
        {
            if (targetIndex != -1)
            {
                if ((transferCandidates != null) && (targetIndex < transferCandidates.Count))
                {
                    setTarget(transferCandidates[targetIndex], false);
                }
            }
            targetIndex++;
            if (targetIndex >= transferCandidates.Count)
            {
                targetIndex = 0;
            }
            setTarget(transferCandidates[targetIndex], true);

            updateUI();
        }

        /// <summary>
        /// Set the previous target for the transfer
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.target.prev", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void prevTarget()
        {
            if (targetIndex != -1)
            {
                if ((transferCandidates != null) && (targetIndex < transferCandidates.Count))
                {
                    setTarget(transferCandidates[targetIndex], false);
                }
            }
            targetIndex--;
            if (targetIndex < 0)
            {
                targetIndex = transferCandidates.Count-1;
            }
            setTarget(transferCandidates[targetIndex], true);

            updateUI();
        }

        /// <summary>
        /// Start the transfer of the resource
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.startTransfer", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void startTransfer()
        {
            if (state == Transferstate.SELECTING_FUEL)
            {
                transferTarget = transferCandidates[targetIndex];
                transferCandidates.Clear();
                state = Transferstate.TRANSFERING_FUEL;
                status = Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.status.transfering", PartResourceLibrary.Instance.GetDefinition(fuelResource).displayName);
            }
            else if (state == Transferstate.SELECTING_WASTE)
            {
                transferTarget = transferCandidates[targetIndex];
                transferCandidates.Clear();
                state = Transferstate.TRANSFERING_WASTE;
                status = Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.status.transfering", PartResourceLibrary.Instance.GetDefinition(wasteResource).displayName);
            }
            updateUI();
        }

        /// <summary>
        /// Stop the transfer of the resource
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.stopTransfer", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void stopTransfer()
        {
            cancelTransfer();
        }

        /// <summary>
        /// Cancel the transfer of the resource
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.nuclearfuel.cancelTransfer", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void cancelTransfer()
        {
            if ((transferCandidates != null) && (targetIndex >= 0) && (targetIndex < transferCandidates.Count))
            {
                setTarget(transferCandidates[targetIndex], false);
                transferCandidates.Clear();
                targetIndex = -1;
            }
            if (transferTarget != null)
            {
                setTarget(transferTarget, false);
                transferTarget = null;
            }
            state = Transferstate.IDLE;
            status = string.Empty;
            updateUI();
        }

        //---------------------Life Cycle----------------------------

        /// <summary>
        /// The start method of the part
        /// </summary>
        /// <param name="state">The StartState of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            GameEvents.onPartActionUIDismiss.Add(onDismiss);
            GameEvents.onVesselChange.Add(onVesselChange);

            //
            Events["initTransferFuel"].guiName = Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.fuel.transfer", PartResourceLibrary.Instance.GetDefinition(fuelResource).displayName);
            Events["initTransferWaste"].guiName = Localizer.Format("#LOC_KERBETROTTER.nuclearfuel.fuel.transfer", PartResourceLibrary.Instance.GetDefinition(wasteResource).displayName);

            //init the colors for highlight and transfer
            defaultColor = new Color(part.highlightColor.r, part.highlightColor.g, part.highlightColor.b, part.highlightColor.a);
            transferColor = new Color(0.33f, 0.635f, 0.87f, part.highlightColor.a);

            hasWasteStorage = part.Resources.Contains(wasteResource);
            hasFuelStorage = part.Resources.Contains(fuelResource);

            converter = part.FindModulesImplementing<BaseConverter>();

            updateUI();
        }

        /// <summary>
        /// Remove as game event listeners when destroyed
        /// </summary>
        public void OnDestroy()
        {
            transferCandidates.Clear();
            GameEvents.onPartActionUIDismiss.Remove(onDismiss);
            GameEvents.onVesselChange.Remove(onVesselChange);
        }



        /**
         * The update method of the module
         */
        public void Update()
        {
            if (transferTarget != null)
            {
                if (state == Transferstate.TRANSFERING_FUEL)
                {
                    double amount = Math.Min(transferRate * TimeWarp.deltaTime, part.Resources[fuelResource].amount);
                    double transferedAmount = receive(transferTarget, fuelResource, amount);

                    part.Resources[fuelResource].amount -= transferedAmount;
                    if (part.Resources[fuelResource].amount < 0)
                    {
                        part.Resources[fuelResource].amount = 0;
                    }


                    if ((amount > transferedAmount) || (part.Resources[fuelResource].amount == 0)) {
                        cancelTransfer();
                    }
                }
                else if (state == Transferstate.TRANSFERING_WASTE)
                {
                    double amount = Math.Min(transferRate * TimeWarp.deltaTime, part.Resources[wasteResource].amount);
                    double transferedAmount = receive(transferTarget, wasteResource, amount);

                    part.Resources[wasteResource].amount -= transferedAmount;
                    if (part.Resources[wasteResource].amount < 0)
                    {
                        part.Resources[wasteResource].amount = 0;
                    }

                    if ((amount > transferedAmount) || (part.Resources[wasteResource].amount == 0))
                    {
                        cancelTransfer();
                    }

                }
            }

            if (transferTarget != null)
            {
                transferTarget.Highlight(true);
            }
        }

        //----------------------Game Events---------------------
        /// <summary>
        /// Called when the UI of the part has been dismissed.
        /// Cancels the transfer of the nuclear fuel
        /// </summary>
        /// <param name="data"></param>
        private void onDismiss(Part part)
        {
            if (this.part == part)
            {
                cancelTransfer();
            }
        }

        private void onVesselChange(Vessel vessel)
        {
            if (vessel = this.vessel)
            {
                cancelTransfer();
            }
        }

        //---------------------Helper---------------------------

        /// <summary>
        /// Check whether the crew of the vessel allows for transfer
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private bool checkCrew(int level)
        {
            List<ProtoCrewMember> crew = part.vessel.GetVesselCrew();
            for (int i = 0; i < crew.Count; i++)
            {
                if ((crew[i].experienceTrait.Title == neededSkill) && (crew[i].experienceLevel >= level))
                {
                    return true;
                }
                //Debug.Log("[KPBS] Title:" + crew[i].experienceTrait.Title + " Desc:" + crew[i].experienceTrait.Description  + " Needed: " + neededSkill);
            }
            return false;
        }

        /// <summary>
        /// Update the visibility of the UI
        /// </summary>
        public void updateUI()
        {
            bool nextVisible = transferCandidates != null && transferCandidates.Count > 1;

            Events["initTransferFuel"].guiActive = (state == Transferstate.IDLE) && hasFuelStorage;
            Events["initTransferWaste"].guiActive = (state == Transferstate.IDLE) && hasWasteStorage;
            Events["nextTarget"].guiActive = nextVisible && ((state == Transferstate.SELECTING_FUEL) || (state == Transferstate.SELECTING_WASTE));
            Events["prevTarget"].guiActive = nextVisible && ((state == Transferstate.SELECTING_FUEL) || (state == Transferstate.SELECTING_WASTE));
            Events["startTransfer"].guiActive = (state == Transferstate.SELECTING_FUEL) || (state == Transferstate.SELECTING_WASTE);
            Events["cancelTransfer"].guiActive = (state == Transferstate.SELECTING_FUEL) || (state == Transferstate.SELECTING_WASTE);
            Events["stopTransfer"].guiActive = (state == Transferstate.TRANSFERING_FUEL) || (state == Transferstate.TRANSFERING_WASTE);
            Fields["status"].guiActive = (state != Transferstate.IDLE);
        }

        /// <summary>
        /// Receive nuclear fuel. 
        /// </summary>
        /// <param name="amount">The amount received</param>
        /// <returns>The amount that can be added</returns>
        public double receive(Part target, string resource,double amount)
        {
            if (!target.Resources.Contains(resource))
            {
                return 0.0;
            }

            double newAmount = Math.Min(target.Resources[resource].maxAmount - target.Resources[resource].amount, amount);
            target.Resources[resource].amount += newAmount;
            if (target.Resources[resource].amount > target.Resources[resource].maxAmount)
            {
                target.Resources[resource].amount = target.Resources[resource].maxAmount;
            }

            return newAmount;
        }

        /// <summary>
        /// Returns whether the part of this module can receive the specified resource
        /// </summary>
        /// <returns>True, when waste can be received, else false</returns>
        public bool canReceive(Part targetPart, string resource)
        {
            if (!targetPart.Resources.Contains(resource))
            {
                return false;
            }
            else
            {
                if (targetPart.Resources[resource].amount < targetPart.Resources[resource].maxAmount)
                {
                    List<BaseConverter> converter = targetPart.FindModulesImplementing<BaseConverter>();

                    if (converter == null)
                    {
                        return true;
                    }

                    for (int i = 0; i < converter.Count; i++)
                    {
                        if (converter[i].ModuleIsActive())
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns wheter there is a converter currently running in this part
        /// </summary>
        /// <returns>True when a converter is running, else false</returns>
        private bool isSave()
        {
            if (converter == null)
            {
                return true;
            }

            for (int i = 0; i < converter.Count; i++)
            {
                if (converter[i].ModuleIsActive())
                {
                    return false;
                }

            }
            return true;
        }

        /// <summary>
        /// Returns whether this part can transfer fuel
        /// </summary>
        /// <returns>True, when fuel can be received, else false</returns>
        public bool hasResource(string resource)
        {
            return part.Resources.Contains(resource) && (part.Resources[resource].amount > 0);
        }

        
        /// <summary>
        /// Get the list of all possible targets for nuclear fuel
        /// </summary>
        /// <returns></returns>
        private List<Part> getTransferCandidates(string fuel)
        {
            List<Part> candidates = new List<Part>();

            for (int i = 0; i < vessel.parts.Count; i++)
            {
                if (vessel.parts[i] != part && canReceive(vessel.parts[i], fuel))
                {
                    candidates.Add(vessel.parts[i]);
                }
            }

            //List<ModuleKerbetrotterNuclearStorage> modules = vessel  //vessel.FindPartModulesImplementing<ModuleKerbetrotterNuclearStorage>();
            //Debug.Log("[KPBS] Found Modules: " + modules.Count);

            //List<ModuleKerbetrotterNuclearStorage> candidates = new List<ModuleKerbetrotterNuclearStorage>();
            //for (int i = 0; i < modules.Count; i++)
            //{
                //if (modules[i].canReceive(fuel) && modules[i] != this)
               // {
                    //Debug.Log("[KPBS] Adding module: " + i);
                    //candidates.Add(modules[i]);
                //}
            //}
            return candidates;
        }

        private void setTarget(Part target, bool isTarget)
        {
            if (isTarget)
            {
                //Debug.Log("LYNX_NUCLEAR: Setting Target: " + target.name);
                target.highlightColor.r = transferColor.r;
                target.highlightColor.g = transferColor.g;
                target.highlightColor.b = transferColor.b;
                target.Highlight(true);
            }
            //set the default highlight color
            else
            {
                //Debug.Log("LYNX_NUCLEAR: Resetting Target: " + target.name);
                target.highlightColor.r = defaultColor.r;
                target.highlightColor.g = defaultColor.g;
                target.highlightColor.b = defaultColor.b;
                target.Highlight(false);
            }
        }

        //---------------------Data---------------------------

        /// <summary>
        /// The state of the transfer of the module
        /// </summary>
        private enum Transferstate
        {
            IDLE,
            SELECTING_FUEL,
            TRANSFERING_FUEL,
            SELECTING_WASTE,
            TRANSFERING_WASTE
        }
    }
}
