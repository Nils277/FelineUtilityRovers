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
using KerbetrotterTools.Editor;
using KerbetrotterTools.Misc.Helper;
using System;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Configuration class to load the settigs for filter 
    /// </summary>
    class KerbetrotterConfiguration
    {
        #region--------------------------Static Members----------------------
        
        //static instance of itself
        private static KerbetrotterConfiguration kerbetrotterConfig;

        //get the instance of this config file
        public static KerbetrotterConfiguration Instance()
        {
            if (kerbetrotterConfig == null)
                kerbetrotterConfig = new KerbetrotterConfiguration();
            return kerbetrotterConfig;
        }

        #endregion

        #region-------------------------Private Members----------------------


        //list of config nodes for the filter
        private KerbetrotterFilterSettings[] filterSettings;

        //Get the settings of the filter
        public KerbetrotterFilterSettings[] FilterSettings
        {
            get
            {
                return filterSettings;
            }
        }

        #endregion

        #region-------------------------Private Methods----------------------

        /// <summary>
        /// Private hidden constructor of this class
        /// </summary>
        private KerbetrotterConfiguration()
        {
            ConfigNode[] nodes = null;

            //try to get the config node
            try
            {
                nodes = GameDatabase.Instance.GetConfigNodes("KerbetrotterFilterConfig");
            }
            catch (Exception e)
            {
                Debug.LogError("[KerbetrotterTools:Config] Config node Exception: " + e.Message);
                Debug.LogException(e);
            }

            //when ne node is null, report an error
            if (nodes == null)
            {
                Debug.LogError("[KerbetrotterTools:Config] KerbetrotterConfiguration: config node is null");
            }

            //try to read and set all the settings
            try
            {
                filterSettings = new KerbetrotterFilterSettings[nodes.Length];

                for (int i = 0; i < nodes.Length; i++)
                {
                    bool showModFilter = bool.Parse(nodes[i].GetValue("showModCategory"));
                    bool showSeparateFunctionCategory = bool.Parse(nodes[i].GetValue("separateFunctionFilter"));

                    string modName = nodes[i].GetValue("name");
                    string includedFilter = nodes[i].GetValue("includeFilter");
                    string excludedFilter = nodes[i].GetValue("excludeFilter");

                    bool disableForCCK = false;

                    string exlcudeWithCCKStr = nodes[i].GetValue("disableForCCK");
                    if (!string.IsNullOrEmpty(exlcudeWithCCKStr))
                    {
                        disableForCCK = bool.Parse(exlcudeWithCCKStr);
                    }

                    Color color = new Color(0.5f, 0.5f, 0.5f);

                    if (nodes[i].HasValue("iconColor"))
                    {
                        color = ColorParser.parse(nodes[i].GetValue("iconColor"));
                    }

                    bool filterLifeSupport = false;
                    string filterLifeSupportStr = nodes[i].GetValue("filterLifeSupport");
                    if (!string.IsNullOrEmpty(filterLifeSupportStr))
                    {
                        filterLifeSupport = bool.Parse(filterLifeSupportStr);
                    }

                    bool oneFilterOnly = false;
                    string oneFilterOnlyString = nodes[i].GetValue("showInOneCategoryOnly");
                    if (!string.IsNullOrEmpty(oneFilterOnlyString))
                    {
                        oneFilterOnly = bool.Parse(oneFilterOnlyString);
                    }

                    string filterIcon = nodes[i].GetValue("filterIcon");

                    filterSettings[i] = new KerbetrotterFilterSettings(modName, filterIcon, includedFilter, excludedFilter, showModFilter, showSeparateFunctionCategory, disableForCCK, filterLifeSupport, oneFilterOnly, color);
                }
            }
            catch (ArgumentNullException exception)
            {
                Debug.LogError("[KerbetrotterTools:Config] KerbetrotterConfiguration config node argument is null " + exception.Message);
                Debug.LogException(exception);
            }
            catch (FormatException exception)
            {
                Debug.LogError("[KerbetrotterTools:Config] KerbetrotterConfiguration config node argument malformed " + exception.Message);
                Debug.LogException(exception);
            }

        }

        #endregion
    }
}
