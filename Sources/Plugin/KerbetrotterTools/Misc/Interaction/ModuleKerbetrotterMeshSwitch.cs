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
using KSP.Localization;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// This class allows to switch some of the models from parts
    /// </summary>
    [KSPModule("Kerbetrotter Model Switch")]
    public class ModuleKerbetrotterMeshSwitch : ModuleKerbetrotterBaseInteraction
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The transforms to show and hide
        /// </summary>
        [KSPField(isPersistant = false)]
        public string transforms = string.Empty;

        /// <summary>
        /// The visible names of the transforms to switch
        /// </summary>
        [KSPField(isPersistant = false)]
        public string visibleNames = string.Empty;

        /// <summary>
        /// The name of the switch name
        /// </summary>
        [KSPField(isPersistant = false)]
        public string switchName = "#LOC_KERBETROTTER.meshswitch.model";

        /// <summary>
        /// Holds whether switcher works stand alone
        /// </summary>
        [KSPField(isPersistant = false)]
        public bool standAlone = true;

        /// <summary>
        /// The currently chosen model
        /// </summary>
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KERBETROTTER.meshswitch.model")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int numModel = 0;

        #endregion

        #region-------------------------Private Members----------------------

        //The previous model index
        private int oldModelNum = -1;

        //The base field
        BaseField modelBaseField;

        //The UI for the chooseer
        UI_ChooseOption modelUIChooser;

        //The list of transforms
        private List<List<Transform>> transformsData;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Find the light transforms and lights to dim with the animation
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            modelBaseField = Fields["numModel"];
            modelUIChooser = (UI_ChooseOption)modelBaseField.uiControlEditor;
            modelBaseField.guiName = switchName;

            //find all the lights
            transformsData = new List<List<Transform>>();
            
            //Search in the named transforms for the lights
            if (transforms != string.Empty) 
            {
                string[] transformNames = transforms.Split(',');

                //find all the transforms
                List<Transform> transformsList = new List<Transform>();

                for (int i = 0; i < transformNames.Length; i++)
                {
                    List<Transform> transSetting = new List<Transform>();
                    //get all the transforms
                    transSetting.AddRange(part.FindModelTransforms(transformNames[i].Trim()));

                    transformsData.Add(transSetting);
                }

                string[] visible = visibleNames.Split(',');
                for (int i = 0; i < visible.Length; i++)
                {
                    visible[i] = visible[i].Trim();
                }

                if (visible.Length == transformNames.Length)
                {
                    modelUIChooser.options = visible;
                }
                else
                {
                    //set the changes for the modelchooser
                    modelUIChooser.options = transformNames;
                }

                //when there is only one model, we do not need to show the controls
                if (transformNames.Length < 2)
                {
                    modelBaseField.guiActive = false;
                    modelBaseField.guiActiveEditor = false;
                }

                //Update the visibility of the switch in flight
                Events["switchMesh"].active = availableInFlight;
                Events["switchMesh"].guiName = Localizer.Format("#LOC_KERBETROTTER.action.change", switchName);
                if (evaOnly)
                {
                    Events["switchMesh"].guiActiveUnfocused = availableInFlight;
                    Events["switchMesh"].externalToEVAOnly = availableInFlight;
                }
            }
            else
            {
                Debug.LogError("ModuleKerbetrotterMeshSwitch: No transforms defined!)");
            }
        }

        /// <summary>
        /// Update the lights in the OnUpdate method
        /// </summary>
        public void Update()
        {
            //when the active model changes
            if (oldModelNum != numModel)
            {
                for (int i = 0; i < transformsData.Count; i++)
                {
                    if (i == numModel)
                    {
                        for (int j = 0; j < transformsData[i].Count; j++)
                        {
                            transformsData[i][j].gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < transformsData[i].Count; j++)
                        {
                            transformsData[i][j].gameObject.SetActive(false);
                        }
                    }
                }
                oldModelNum = numModel;
            }
        }

        #endregion

        #region------------------------User Interaction----------------------

        /// <summary>
        /// Event that toggles the visibility of the mesh
        /// </summary>
        [KSPEvent(name = "switchMesh", guiName = "#LOC_KERBETROTTER.meshswitch.model", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void switchMesh()
        {
            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            numModel++;
            if (numModel >= transformsData.Count)
            {
                numModel = 0;
            }
        }

        #endregion
    }
}
