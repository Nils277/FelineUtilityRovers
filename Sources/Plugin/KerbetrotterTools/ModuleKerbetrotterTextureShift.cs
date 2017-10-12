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
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterTextureShift : PartModule, IResourceChangeListener
    {
        /// <summary>
        /// The name of the transforms to apply the thrust to
        /// </summary>
        [KSPField]
        public string transformName = string.Empty;

        /// <summary>
        /// The name of the texture that should be changed
        /// </summary>
        [KSPField]
        public string textureName = string.Empty;

        /// <summary>
        /// The id of the module
        /// </summary>
        [KSPField]
        public string moduleID = string.Empty;

        /// <summary>
        /// The used resource configuration
        /// </summary>
        [KSPField]
        public string resourceConfiguration = string.Empty;

        //The material containing the texture
        private Material material;

        //The list of resources that can be switched in the tank
        private Dictionary<string, Vector2> switchableResources;

        //the resource switch module
        private IConfigurableResourceModule resourceSwitch;

        /// <summary>
        /// Find the material and resource switch when started
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            initSwitchableResources();

            Transform textureTransform = part.FindModelTransform(transformName);
            if (textureTransform != null)
            {
                material = textureTransform.GetComponent<Renderer>().material;
            }

            for (int i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i] is IConfigurableResourceModule)
                {
                    resourceSwitch = (IConfigurableResourceModule)part.Modules[i];
                    resourceSwitch.addResourceChangeListener(this);
                    break;
                }
            }
        }

        /// <summary>
        /// Free all resources when the part is destroyed
        /// </summary>
        public void OnDestroy()
        {
            if (resourceSwitch != null)
            {
                resourceSwitch.removeResourceChangeListener(this);
                resourceSwitch = null;
            }
        }

        /// <summary>
        /// Called when the resource of the part has changed
        /// </summary>
        /// <param name="name">The name of the new resource</param>
        public void onResourceChanged(string name)
        {
            if (material != null)
            {
                if (switchableResources.ContainsKey(name)) { 
                    Vector2 offset = switchableResources[name];
                    if (offset != null)
                    {
                        material.SetTextureOffset(textureName, offset);
                    }
                    else
                    {
                        material.SetTextureOffset(textureName, new Vector2(0.0f, 0.0f));
                    }
                }
                else
                {
                    material.SetTextureOffset(textureName, new Vector2(0.0f, 0.0f));
                }
            }
        }

        /// <summary>
        /// Initialize the switchable resources.
        /// </summary>
        private void initSwitchableResources()
        {
            //ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");

            bool valid = false;
            
            List<ModuleKerbetrotterTextureShift> modules = part.Modules.GetModules<ModuleKerbetrotterTextureShift>();
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
                        string[] definitions = modulesConfigs[i].GetValues("textureOffset");
                        switchableResources = parseResources(definitions);
                        valid = true;
                        break;
                    }
                    configNodeIndex++;
                }
            }

            if (!valid)
            {
                Debug.LogError("[ModuleKerbetrotterTextureShift] " + moduleName + ": Cannot find valid configNode for this module");
            }
        }

        /// <summary>
        /// Parse the config node of this partModule to get all definition for resources
        /// </summary>
        /// <param name="configNode">The config node of this part</param>
        /// <returns>List of switchable resources</returns>
        private Dictionary<string, Vector2> parseResources(string[] definitions)
        {
            //create the list of resources
            Dictionary<string, Vector2> resources = new Dictionary<string, Vector2>();

            if ((definitions == null) || (definitions.Length == 0))
            {
                return resources;
            }


            for (int i = 0; i < definitions.Length; i++)
            {
                string[] values = definitions[i].Split(',');
                if (values.Length == 3)
                {
                    string ID = values[0].Trim();
                    Vector2 offset = new Vector2(0.0f, 0.0f);
                    try
                    {
                        offset.x = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        offset.y = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);
                    }
                    finally
                    {
                        resources.Add(ID, offset);
                    }
                }
            }
            return resources;
        }


    }
}
