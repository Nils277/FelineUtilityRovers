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
using System.Globalization;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Module to shift textures. Mostly triggered by other modules (e.g. for switching)
    /// </summary>
    class ModuleKerbetrotterTextureShift : PartModule, ISwitchListener
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The name of the transforms to apply the thrust to
        /// </summary>
        [KSPField]
        public string transformNames = string.Empty;

        /// <summary>
        /// The name of the texture that should be changed
        /// </summary>
        [KSPField]
        public string textureName = string.Empty;

        /// <summary>
        /// The id of the module
        /// </summary>
        [KSPField]
        public string setupGroup = string.Empty;

        #endregion

        #region-------------------------Private Members----------------------

        //The material containing the texture
        //private List<Material> materials = new List<Material>();

        //The list of resources that can be switched in the tank
        private Dictionary<string, Vector2> setups = new Dictionary<string, Vector2>();

        //The current setup of the texture switch
        private string currentSetup = string.Empty;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Find the material and resource switch when started
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            loadSetups(part.partInfo.partConfig);

            /*materials.Clear();
            string[] transforms = transformNames.Split(',');
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform textureTransform = part.FindModelTransform(transforms[i].Trim());
                if (textureTransform != null)
                {
                    materials.Add(textureTransform.GetComponent<Renderer>().material);
                }
            }*/

            //set the used setup if already set
            if (currentSetup != string.Empty)
            {
                onSwitch(currentSetup);
            }

        }

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Get the setup group this switcher belongs to
        /// </summary>
        /// <returns></returns>
        public string getSetup()
        {
            return setupGroup;
        }

        /// <summary>
        /// Called when this module should switch
        /// </summary>
        /// <param name="setup">The name of the new setup to switch to</param>
        public void onSwitch(string setup)
        {
            currentSetup = setup;

            List<Material> materials = getMaterials();

            if (materials.Count > 0)
            {
                if (setups.ContainsKey(setup))
                {
                    Vector2 offset = setups[setup];
                    for (int i = 0; i < materials.Count; i++)
                    {
                        materials[i].SetTextureOffset(textureName, offset);
                    }
                }
                else
                {
                    for (int i = 0; i < materials.Count; i++)
                    {
                        materials[i].SetTextureOffset(textureName, new Vector2(0, 0));
                    }
                }
                
            }
        }

        #endregion

        #region-------------------------Helper Methods-----------------------

        /// <summary>
        /// Initialize the switchable resources.
        /// </summary>
        private void loadSetups(ConfigNode node)
        {
            setups.Clear();
            ConfigNode[] modules = node.GetNodes("MODULE");
            int index = part.Modules.IndexOf(this);

            if (index != -1 && index < modules.Length && modules[index].GetValue("name") == moduleName)
            {
                ConfigNode[] propConfig = modules[index].GetNodes("SETUP");
                for (int i = 0; i < propConfig.Length; i++)
                {
                    if (propConfig[i].HasValue("ID"))
                    {
                        string ID = propConfig[i].GetValue("ID").Trim();
                        Vector2 offset = new Vector2(0, 0);
                        //load the name of the resource to load
                        if (propConfig[i].HasValue("Offset"))
                        {
                            
                            string[] offests = propConfig[i].GetValue("Offset").Split(',');
                            try
                            {
                                offset.x = float.Parse(offests[0], CultureInfo.InvariantCulture.NumberFormat);
                                offset.y = float.Parse(offests[1], CultureInfo.InvariantCulture.NumberFormat);
                            }
                            catch
                            {
                                Debug.LogError("[KerbetrotterTools:TextureShift] Invalid definition of offset for: " + ID);
                            }
                        }
                        setups.Add(ID, offset);
                    }
                }
            }
            else
            {
                Debug.LogError("[KerbetrotterTools:TextureShift] Cannot load setups");
            }
        }

        /// <summary>
        /// Get the list of materials that are available
        /// </summary>
        /// <returns></returns>
        private List<Material> getMaterials()
        {
            List<Material> materials = new List<Material>();
            string[] transforms = transformNames.Split(',');
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform[] textureTransforms = part.FindModelTransforms(transforms[i].Trim());
                foreach (Transform textureTransform in textureTransforms)
                {
                    Renderer[] renderers = textureTransform.GetComponents<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        materials.Add(renderer.material);
                    }
                }
            }

            return materials;
        }

        #endregion
    }
}
