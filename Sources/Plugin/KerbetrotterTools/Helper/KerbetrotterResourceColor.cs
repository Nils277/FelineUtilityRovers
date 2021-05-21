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

namespace KerbetrotterTools.Misc.Helper
{
    /// <summary>
    /// Definition for color for a resource
    /// </summary>
    class KerbetrotterResourceColor
    {
        /// <summary>
        /// The primary color of the resource
        /// </summary>
        public Color Primary { get; }

        /// <summary>
        /// The secondary color of the resource
        /// </summary>
        public Color Secondary { get; }

        /// <summary>
        /// Constructor of the resource color
        /// </summary>
        /// <param name="node">The node containg the color definition</param>
        public KerbetrotterResourceColor(ConfigNode node)
        {
            //load the ID of the mode
            if (node.HasValue("primaryColor"))
            {
                Primary = ColorParser.parse(node.GetValue("primaryColor"));
                Secondary = node.HasValue("secondaryColor") ? ColorParser.parse(node.GetValue("secondaryColor")) : Primary;
            }
            else if (node.HasValue("secondaryColor"))
            {
                Secondary = ColorParser.parse(node.GetValue("secondaryColor"));
                Primary = Secondary;
            }
            else
            {
                Primary = Color.white;
                Secondary = Color.white;
            }
        }
    }
}
