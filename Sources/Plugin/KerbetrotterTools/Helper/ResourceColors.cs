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
using System;
using System.Collections.Generic;

namespace KerbetrotterTools.Misc.Helper
{
    /// <summary>
    /// Static class holding predefined colors for resources used for switching
    /// </summary>
    class ResourceColors
    {
        #region-------------------------Private Members----------------------

        /// <summary>
        /// The static dictionary containing the mapping from a resource name to a color
        /// </summary>
        private static Dictionary<String, KerbetrotterResourceColor> _colors;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Get a color for a resource name
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <returns>The color of the resource</returns>
        public static KerbetrotterResourceColor get(String name)
        {
            if (_colors == null)
            {
                init();
            }
            if (_colors.ContainsKey(name))
            {
                return _colors[name];
            }
            return null;
        }

        /// <summary>
        /// Get whether there is a color defined for a resource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool has(String name)
        {
            if (_colors == null)
            {
                init();
            }
            return _colors.ContainsKey(name);
        }

        #endregion

        #region-------------------------Private Methods----------------------

        /// <summary>
        /// Initialize the resource colors
        /// </summary>
        private static void init()
        {
            ConfigNode[] setupConfig = GameDatabase.Instance.GetConfigNodes("KERBETROTTER_RESOURCE_COLOR");
            _colors = new Dictionary<string, KerbetrotterResourceColor>();
            for (int i = 0; i < setupConfig.Length; i++)
            {
                if (setupConfig[i].HasValue("name"))
                {
                    _colors.Add(setupConfig[i].GetValue("name"), new KerbetrotterResourceColor(setupConfig[i]));
                }
            }
        }

        #endregion
    }
}
