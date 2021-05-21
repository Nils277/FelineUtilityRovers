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
using UnityEngine;

namespace KerbetrotterTools.Editor
{
    /// <summary>
    /// Class holding all the filter settings for the editor
    /// </summary>
    class KerbetrotterFilterSettings
    {
        #region-------------------------Public Members----------------------

        //The name of the mod
        public string ModName { get; }

        //Filter for all parts that should be included
        public string IncludeFilter { get; }

        //Filter for all parts that should be excluded
        public string ExcludeFilter { get; }

        //The name of the filter icon
        public string FilterIcon { get; }

        //Whether a separate category should be shown for this mod
        public bool ShowModFilter { get; }

        //Whether parts should have their own function filter
        public bool ShowFunctionFilter { get; }

        //Whether filtering should be disabled when CCK is installed
        public bool DisableForCCK { get; }

        //Whether parts should be filtered into a life support category
        public bool FilterLifeSupport { get; }

        //Whether there should be only one filter for the mod
        public bool ShowInOneFilterOnly { get; }

        //The color of the filter icon
        public Color Color { get; }

        #endregion

        #region-----------------------Public Constructor--------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modName">The name of the mod</param>
        /// <param name="filterIcon">The name of the filter icon</param>
        /// <param name="includeFilter">Filter for all parts that should be included</param>
        /// <param name="excludeFilter">Filter for all parts that should be excluded</param>
        /// <param name="showModFilter">Whether a separate category should be shown for this mod</param>
        /// <param name="showFunctionFilter">/Whether parts should have their own function filter</param>
        /// <param name="disableForCCK">Whether filtering should be disabled when CCK is installed</param>
        /// <param name="filterLifeSupport">Whether parts should be filtered into a life support category</param>
        /// <param name="oneFilterOnly">Whether there should be only one filter for the mod</param>
        /// <param name="color">The color of the filter icon</param>
        public KerbetrotterFilterSettings(string modName, string filterIcon, string includeFilter, string excludeFilter, bool showModFilter, bool showFunctionFilter, bool disableForCCK, bool filterLifeSupport, bool oneFilterOnly, Color color)
        {
            ModName = modName;
            FilterIcon = filterIcon;
            IncludeFilter = includeFilter;
            ExcludeFilter = excludeFilter;
            ShowModFilter = showModFilter;
            ShowInOneFilterOnly = oneFilterOnly;
            ShowFunctionFilter = showFunctionFilter;
            DisableForCCK = disableForCCK;
            FilterLifeSupport = filterLifeSupport;
            Color = color;
        }

        #endregion
    }
}
