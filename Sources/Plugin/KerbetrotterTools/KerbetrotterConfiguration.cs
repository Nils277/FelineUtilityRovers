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
using System;
using System.Globalization;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Configuration class to load the settigs for filter 
    /// </summary>
    class KerbetrotterConfiguration
    {
        //static instance of itself
        private static KerbetrotterConfiguration kerbetrotterConfig;

        //get the instance of this config file
        public static KerbetrotterConfiguration Instance()
        {
            if (kerbetrotterConfig == null)
                kerbetrotterConfig = new KerbetrotterConfiguration();
            return kerbetrotterConfig;
        }

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


        // The constructor for this class reading the settings
        private KerbetrotterConfiguration()
        {
            Debug.Log("[KerbetrotterTools]Init settings");

            ConfigNode[] nodes = null;
           

            //try to get the config node
            try
            {
                nodes = GameDatabase.Instance.GetConfigNodes("KerbetrotterFilterConfig");
            }
            catch (Exception e)
            {
                Debug.Log("[KerbetrotterTools]Config node Exception: " + e.Message);
            }

            //when ne node is null, report an error
            if (nodes == null)
            {
                Debug.Log("[KerbetrotterTools] ERROR config node is null");
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

                    string colorStr = nodes[i].GetValue("iconColor");
                    if (!string.IsNullOrEmpty(colorStr))
                    {
                        string[] colorStrings = colorStr.Split(',');
                        if (colorStrings.Length == 3)
                        {
                            try
                            {
                                float r = float.Parse(colorStrings[0], CultureInfo.InvariantCulture.NumberFormat);
                                float g = float.Parse(colorStrings[1], CultureInfo.InvariantCulture.NumberFormat);
                                float b = float.Parse(colorStrings[2], CultureInfo.InvariantCulture.NumberFormat);
                                color = new Color(r, g, b);
                            }
                            catch
                            {
                                Debug.LogError("[KerbetrotterTools] Invalid color definition");
                            }
                        }
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
                Debug.LogError("[KerbetrotterTools] ERROR config node argument is null " + exception.Message);
            }
            catch (FormatException exception)
            {
                Debug.LogError("[KerbetrotterTools] ERROR config node argument malformed " + exception.Message);
            }

        }
    }

    class KerbetrotterFilterSettings
    {
        private string modName;
        private string includeFilter;
        private string excludeFilter;
        private string filterIcon;
        private bool showModFilter;
        private bool showFunctionFilter;
        private bool disableForCCK;
        private bool filterLifeSupport;
        private bool oneFilterOnly;
        private Color color;

        public KerbetrotterFilterSettings(string modName, string filterIcon, string includeFilter, string excludeFilter, bool showModFilter, bool showFunctionFilter, bool disableForCCK, bool filterLifeSupport, bool oneFilterOnly, Color color)
        {
            this.modName = modName;
            this.filterIcon = filterIcon;
            this.includeFilter = includeFilter;
            this.excludeFilter = excludeFilter;
            this.showModFilter = showModFilter;
            this.oneFilterOnly = oneFilterOnly;
            this.showFunctionFilter = showFunctionFilter;
            this.disableForCCK = disableForCCK;
            this.filterLifeSupport = filterLifeSupport;
            this.color = color;
        }

        public Color Color
        {
            get
            {
                return color;
            }
        }

        public bool FilterLifeSupport
        {
            get
            {
                return filterLifeSupport;
            }
        }

        public string ModName
        {
            get
            {
                return modName;
            }
        }

        public bool DisableForCCK
        {
            get
            {
                return disableForCCK;
            }
        }

        public string IncludeFilter
        {
            get
            {
                return includeFilter;
            }
        }

        public string ExcludeFilter
        {
            get
            {
                return excludeFilter;
            }
        }

        public string FilterIcon
        {
            get
            {
                return filterIcon;
            }
        }

        public bool ShowModFilter
        {
            get
            {
                return showModFilter;
            }
        }

        public bool ShowFunctionFilter
        {
            get
            {
                return showFunctionFilter;
            }
        }

        public bool ShowInOneFilterOnly
        {
            get
            {
                return oneFilterOnly;
            }
        }
    }
}
