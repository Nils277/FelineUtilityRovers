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
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KerbetrotterTools
{
    /// <summary>
    /// This class extends the ModuleColorChanges and adds the ability to dim lights with the animation
    /// Helpful for parts without a legacy animation for emissives that want to toggle lights 
    /// </summary>
    [KSPModule("Kerbetrotter Light")]
    public class ModuleKerbetrotterMultiLight : ModuleColorChanger
    {
        //The transforms to show and hide
        [KSPField(isPersistant = false)]
        public string transforms = string.Empty;

        [KSPField(isPersistant = false)]
        public string visibleNames = string.Empty;

        [KSPField(isPersistant = false)]
        public int noLightNum = -1;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Type")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int numModel = 0;
        private int oldModelNum = -1;

        BaseField modelBaseField;
        UI_ChooseOption modelUIChooser;

        //The list lights
        private List<LightSetting> lightSettings;

        //The previous state
        private float prevState = -1.0f;

        /// <summary>
        /// Update the lights in the OnUpdate method
        /// </summary>
        public override void Update()
        {
            base.Update();
            updateLights(currentRateState);
        }

        /// <summary>
        /// Find the light transforms and lights to dim with the animation
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            modelBaseField = Fields["numModel"];
            modelBaseField.guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.light.type");
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
                    lightSetting.transforms.AddRange(part.FindModelTransforms(transformNames[i].Trim()));

                    //get all the lights in the transforms
                    for (int j = 0; j < lightSetting.transforms.Count; j++)
                    {
                        lightSetting.lights.AddRange(lightSetting.transforms[j].gameObject.GetComponentsInChildren<Light>());
                    }

                    //get all the intensities of the lights
                    for (int j = 0; j < lightSetting.lights.Count; j++)
                    {
                        lightSetting.intensities.Add(lightSetting.lights[j].intensity);
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
            }
            else
            {
                Debug.LogError("ModuleKerbetrotterMultiLight: No light transform defined!)");
            }
            //update the initial state of the lights
            updateLights(currentRateState);
        }

        /// <summary>
        /// Update the lights
        /// </summary>
        /// <param name="state">the new state of the animation</param>
        private void updateLights(float state)
        {
            if (lightSettings == null || lightSettings.Count == 0)
            {
                return;
            }

            //when the active model changes
            if (oldModelNum != numModel)
            {
                for (int i = 0; i < lightSettings.Count; i++)
                {
                    if (i == numModel)
                    {
                        for (int j = 0; j < lightSettings[i].transforms.Count; j++)
                        {
                            lightSettings[i].transforms[j].gameObject.SetActive(true);
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

                        for (int j = 0; j < lightSettings[i].lights.Count; j++)
                        {
                            lightSettings[i].lights[j].enabled = true;
                            lightSettings[i].lights[j].intensity = lightSettings[i].intensities[j] * state;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < lightSettings[i].transforms.Count; j++)
                        {
                            lightSettings[i].transforms[j].gameObject.SetActive(false);
                        }
                        for (int j = 0; j < lightSettings[i].lights.Count; j++)
                        {
                            lightSettings[i].lights[j].enabled = false;
                            lightSettings[i].lights[j].intensity = 0;
                        }
                    }
                }
                prevState = -1;
                oldModelNum = numModel;
            }
            
            //chane the intensities of the lights
            if (state != prevState)
            {
                for (int i = 0; i < lightSettings[numModel].lights.Count; i++)
                {
                    //Fully disabled the lights when off
                    if (state == 0.0)
                    {
                            lightSettings[numModel].lights[i].enabled = false;
                    }
                    else
                    {
                        if (lightSettings[numModel].lights[i].enabled == false)
                        {
                            lightSettings[numModel].lights[i].enabled = true;
                        }

                        lightSettings[numModel].lights[i].intensity = lightSettings[numModel].intensities[i] * state;
                    }
                }
                prevState = state;
            }
        }


        private class LightSetting
        {
            public LightSetting()
            {
                lights = new List<Light>();
                transforms = new List<Transform>();
                intensities = new List<float>();
            }

            public List<Light> lights;
            public List<Transform> transforms;
            public List<float> intensities;
        }
    }
}
