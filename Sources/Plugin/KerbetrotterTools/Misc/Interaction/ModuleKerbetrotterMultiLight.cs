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
    /// This class extends the ModuleColorChanges and adds the ability to dim lights with the animation
    /// Helpful for parts without a legacy animation for emissives that want to toggle lights 
    /// </summary>
    [KSPModule("Kerbetrotter Multi Light")]
    public class ModuleKerbetrotterMultiLight : ModuleKerbetrotterBaseInteraction
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The transforms to show and hide
        /// </summary>
        [KSPField(isPersistant = false)]
        public string transforms = string.Empty;

        /// <summary>
        /// The visible names of the lights
        /// </summary>
        [KSPField(isPersistant = false)]
        public string visibleNames = string.Empty;

        /// <summary>
        /// The number of the settings that does not have lights
        /// </summary>
        [KSPField(isPersistant = false)]
        public int noLightNum = -1;

        /// <summary>
        /// The number of the current model with a light
        /// </summary>
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KERBETROTTER.light.type")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int numModel = 0;

        #endregion

        #region-------------------------Private Members----------------------

        //The old number of the chosen model
        private int oldModelNum = -1;

        //Base field of the model
        BaseField modelBaseField;

        //Chooser for the UI option
        UI_ChooseOption modelUIChooser;

        //The list lights
        private List<LightSetting> lightSettings;

        //The previous state
        private float prevState = -1.0f;

        //The module for the color changer
        private ModuleColorChanger mColorChanger;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Find the light transforms and lights to dim with the animation
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //find the module changing the colors
            List<ModuleColorChanger> colorChanger = part.FindModulesImplementing<ModuleColorChanger>();
            foreach (ModuleColorChanger changer in colorChanger)
            {
                if (changer.defaultActionGroup == KSPActionGroup.Light)
                {
                    mColorChanger = changer;
                    break;
                }
            }

            modelBaseField = Fields["numModel"];
            modelUIChooser = (UI_ChooseOption)modelBaseField.uiControlEditor;

            //find all the lights
            lightSettings = new List<LightSetting>();
            
            //Search in the named transforms for the lights
            if (transforms != string.Empty) 
            {
                string[] transformNames = transforms.Split(',');

                //find all the transforms
                List<Transform> transformsList = new List<Transform>();

                for (int i = 0; i < transformNames.Length; i++)
                {
                    LightSetting lightSetting = new LightSetting();

                    //get all the transforms
                    lightSetting.Transforms.AddRange(part.FindModelTransforms(transformNames[i].Trim()));

                    //get all the lights in the transforms
                    for (int j = 0; j < lightSetting.Transforms.Count; j++)
                    {
                        lightSetting.Lights.AddRange(lightSetting.Transforms[j].gameObject.GetComponentsInChildren<Light>());
                    }

                    //get all the intensities of the lights
                    for (int j = 0; j < lightSetting.Lights.Count; j++)
                    {
                        lightSetting.Intensities.Add(lightSetting.Lights[j].intensity);
                    }

                    lightSettings.Add(lightSetting);
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
                Events["switchLight"].active = availableInFlight;
                Events["switchLight"].guiName = Localizer.Format("#LOC_KERBETROTTER.action.change", "#autoLOC_900793");
                if (evaOnly)
                {
                    Events["switchLight"].guiActiveUnfocused = availableInFlight;
                    Events["switchLight"].externalToEVAOnly = availableInFlight;
                }
            }
            else
            {
                Events["switchLight"].active = false;
                Debug.LogError("ModuleKerbetrotterMultiLight: No light transform defined!)");
            }
            //update the initial state of the lights
            updateLights();
        }

        /// <summary>
        /// Update the lights in the OnUpdate method
        /// </summary>
        public void Update()
        {
            updateLights();
        }

        #endregion

        #region------------------------User Interaction----------------------

        /// <summary>
        /// Event that toggles the visibility of the mesh
        /// </summary>
        [KSPEvent(name = "switchLight", guiName = "#LOC_KERBETROTTER.lightswitch.configure", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void switchLight()
        {
            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }

            numModel++;
            if (numModel >= lightSettings.Count)
            {
                numModel = 0;
            }
        }

        #endregion

        #region-------------------------Private Methods----------------------

        /// <summary>
        /// Update the lights
        /// </summary>
        /// <param name="state">the new state of the animation</param>
        private void updateLights()
        {
            if (lightSettings == null || lightSettings.Count == 0  || mColorChanger == null)
            {
                return;
            }

            float state = mColorChanger.CurrentRateState;

            //when the active model changes
            if (oldModelNum != numModel)
            {
                for (int i = 0; i < lightSettings.Count; i++)
                {
                    if (i == numModel)
                    {
                        for (int j = 0; j < lightSettings[i].Transforms.Count; j++)
                        {
                            lightSettings[i].Transforms[j].gameObject.SetActive(true);
                        }

                        if (Events["ToggleEvent"] != null)
                        {
                            if (i != noLightNum)
                            {
                                Events["ToggleEvent"].guiActive = true;
                                Events["ToggleEvent"].guiActiveEditor = true;
                            }
                            else
                            {
                                Events["ToggleEvent"].guiActive = false;
                                Events["ToggleEvent"].guiActiveEditor = false;
                            }
                        }

                        for (int j = 0; j < lightSettings[i].Lights.Count; j++)
                        {
                            lightSettings[i].Lights[j].enabled = true;
                            lightSettings[i].Lights[j].intensity = lightSettings[i].Intensities[j] * state;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < lightSettings[i].Transforms.Count; j++)
                        {
                            lightSettings[i].Transforms[j].gameObject.SetActive(false);
                        }
                        for (int j = 0; j < lightSettings[i].Lights.Count; j++)
                        {
                            lightSettings[i].Lights[j].enabled = false;
                            lightSettings[i].Lights[j].intensity = 0;
                        }
                    }
                }
                prevState = -1;
                oldModelNum = numModel;
            }
            
            //chane the intensities of the lights
            if (state != prevState)
            {
                for (int i = 0; i < lightSettings[numModel].Lights.Count; i++)
                {
                    //Fully disabled the lights when off
                    if (state == 0.0)
                    {
                            lightSettings[numModel].Lights[i].enabled = false;
                    }
                    else
                    {
                        if (lightSettings[numModel].Lights[i].enabled == false)
                        {
                            lightSettings[numModel].Lights[i].enabled = true;
                        }

                        lightSettings[numModel].Lights[i].intensity = lightSettings[numModel].Intensities[i] * state;
                    }
                }
                prevState = state;
            }
        }

        #endregion

        #region-----------------------------Classes--------------------------

        /// <summary>
        /// Class holding a setting of the lights
        /// </summary>
        private class LightSetting
        {
            /// <summary>
            /// Constructor of the class
            /// </summary>
            public LightSetting()
            {
                Lights = new List<Light>();
                Transforms = new List<Transform>();
                Intensities = new List<float>();
            }

            /// <summary>
            /// The lights affected by this setting
            /// </summary>
            public List<Light> Lights { get; }

            /// <summary>
            /// The transforms affected by this setting
            /// </summary>
            public List<Transform> Transforms { get; }

            /// <summary>
            /// The intensities of the affected lights
            /// </summary>
            public List<float> Intensities { get; }
        }

        #endregion
    }
}
