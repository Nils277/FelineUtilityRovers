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
using UnityEngine;

namespace KerbetrotterTools
{
    class KerbetrotterConverterSetup : IConfigNode
    {
        //-------------------------Parameters----------------------

        //The name of the converter
        private string[] converters = null;

        //The ID of this setup
        private string id = string.Empty;

        //Gui name of the converter
        private string guiName = string.Empty;

        //whether this profile is the default one
        private bool isDefault = false;

        //---------------------------Constructors------------------------

        /// <summary>
        /// Constructor of the pid profile
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public KerbetrotterConverterSetup(ConfigNode node)
        {
            Load(node);
        }

        /// <summary>
        /// Save the engine config
        /// </summary>
        /// <param name="node">The config to save to</param>
        public void Save(ConfigNode node)
        {
            ConfigNode.CreateConfigFromObject(this, node);
        }

        /// <summary>
        /// Get whether this setupt contains a converter
        /// </summary>
        /// <param name="converter"></param>
        /// <returns>True when the setup contains the converte, else false</returns>
        public bool contains(string converter)
        {
            if (converters == null)
            {
                return false;
            }
            foreach (string c in converters)
            {
                if (c.Equals(converter)) {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Load the engine config
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public void Load(ConfigNode node)
        {
            if (node.name.Equals("SETUP"))
            {
                ConfigNode.LoadObjectFromConfig(this, node);

                //load the name of the mode
                if (node.HasValue("ID"))
                {
                    id = node.GetValue("ID");
                }

                if (node.HasValue("converter"))
                {
                    converters = node.GetValue("converter").Split(',');
                    for (int i = 0; i < converters.Length; i++)
                    {
                        converters[i] = Localizer.Format(converters[i]);
                        Debug.Log("[KerbetrotterTools:ConverterSwitch] Found converter: " + converters[i]);
                    }
                }

                //load whether the mode is default
                if (node.HasValue("isDefault"))
                {
                    isDefault = bool.Parse(node.GetValue("isDefault"));
                }

                //load the gui name
                if (node.HasValue("guiName"))
                {
                    guiName = Localizer.Format(node.GetValue("guiName"));
                }
            }
        }

        //----------------------------Getter--------------------------

        /// <summary>
        /// The name of the mode
        /// </summary>
        public string ID
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// Get the gui visible name of the setup
        /// </summary>
        public string GuiName
        {
            get
            {
                return guiName;
            }
        }

        /// <summary>
        /// Return whether this part is the default
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return isDefault;
            }
        }
    }
}
