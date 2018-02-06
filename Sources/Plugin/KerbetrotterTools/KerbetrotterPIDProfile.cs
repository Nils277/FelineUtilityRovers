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
using UnityEngine;

namespace KerbetrotterTools
{
    class KerbetrotterPIDProfile : IConfigNode
    {
        //-------------------------Parameters----------------------

        //The name of the engine mode
        private string planet = Localizer.Format("#LOC_KERBETROTTER.engine.profile.default");

        //The proportional part
        private float[] pid = new float[] { 1.0f, 1.0f, 1.0f };

        //whether this profile is the default one
        private bool isDefault = false;

        //---------------------------Constructors------------------------

        /// <summary>
        /// Constructor of the pid profile
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public KerbetrotterPIDProfile(ConfigNode node)
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
        /// Load the engine config
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public void Load(ConfigNode node)
        {
            if (node.name.Equals("PID-PROFILE"))
            {
                ConfigNode.LoadObjectFromConfig(this, node);

                //load the name of the mode
                if (node.HasValue("planet"))
                {
                    planet = node.GetValue("planet");
                }

                if (node.HasValue("isDefault"))
                {
                    isDefault = bool.Parse(node.GetValue("isDefault"));
                }

                //load the propellants
                if (node.HasValue("values"))
                {
                    string[] values = node.GetValue("values").Split(',');
                    if (values.Length == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                pid[i] = float.Parse(values[i].Trim());
                            }
                            catch (Exception e)
                            {
                                pid[i] = 1.0f;
                                Debug.LogError("[LYNX] Cannot load pid value for profile" + e.Message);
                            }
                        }
                    }
                }
            }
        }

        //----------------------------Getter--------------------------

        /// <summary>
        /// The name of the mode
        /// </summary>
        public string Profile
        {
            get
            {
                return planet;
            }
        }

        /// <summary>
        /// Whether this is the default node
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return isDefault;
            }
        }

        /// <summary>
        /// The proportional value
        /// </summary>
        public float P
        {
            get
            {
                return pid[0];
            }
        }

        /// <summary>
        /// The integral value
        /// </summary>
        public float I
        {
            get
            {
                return pid[1];
            }
        }

        /// <summary>
        /// The differential value
        /// </summary>
        public float D
        {
            get
            {
                return pid[2];
            }
        }
    }
}
