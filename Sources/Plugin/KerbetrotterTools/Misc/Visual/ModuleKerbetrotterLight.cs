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
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// This class extends the ModuleColorChanger and adds the ability to dim lights with the animation
    /// Helpful for parts without a legacy animation for emissives that want to toggle lights 
    /// </summary>
    [KSPModule("Kerbetrotter Light")]
    public class ModuleKerbetrotterLight : ModuleColorChanger
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The transforms with the lights
        /// </summary>
        [KSPField(isPersistant = false)]
        public string lightTransforms = string.Empty;

        #endregion

        #region-------------------------Private Members----------------------

        //The list lights
        private List<Light> lights;
        
        //The intensities of the lights
        private List<float> intensities;

        //The previous state
        private float prevState = -1.0f;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Update the lights in the OnUpdate method
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
            updateLights(currentRateState);
        }

        /// <summary>
        /// Find the light transforms and lights to dim with the animation
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //find all the lights
            lights = new List<Light>();
            intensities = new List<float>();

            //Search in the named transforms for the lights
            if (lightTransforms != string.Empty)
            {
                string[] transformNames = lightTransforms.Split(',');

                //find all the transforms
                List<Transform> transforms = new List<Transform>();
                for (int i = 0; i < transformNames.Length; i++)
                {
                    transforms.AddRange(part.FindModelTransforms(transformNames[i]));
                }

                //get all the lights and save their intensities for animation
                for (int i = 0; i < transforms.Count; i++)
                {
                    Light[] newLights = transforms[i].gameObject.GetComponentsInChildren<Light>();

                    for (int j = 0; j < newLights.Length; j++)
                    {
                        if (newLights[j] != null) {
                            lights.Add(newLights[j]);
                            intensities.Add(newLights[j].intensity);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("ModuleKerbetrotterLight: No light transform defined!)");
            }
            //update the initial state of the lights
            updateLights(currentRateState);
        }

        #endregion

        #region-------------------------Private Methods----------------------

        /// <summary>
        /// Update the lights
        /// </summary>
        /// <param name="state">the new state of the animation</param>
        private void updateLights(float state)
        {
            //only change something when the state has changed
            if (state != prevState)
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    //Fully disabled the lights when off
                    if (state == 0.0)
                    {
                        if (lights[i].enabled)
                        {
                            lights[i].enabled = false;
                            lights[i].intensity = 0;
                        }
                    }
                    else
                    {
                        if (lights[i].enabled == false)
                        {
                            lights[i].enabled = true;
                        }
                        lights[i].intensity = intensities[i] * state;
                    }
                }
                prevState = state;
            }
        }

        #endregion
    }
}
