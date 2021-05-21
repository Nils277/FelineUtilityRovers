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
using System.Globalization;
using UnityEngine;

namespace KerbetrotterTools.Misc.Helper
{
    /// <summary>
    /// Helper class to parts a color for a resource, part or similar
    /// </summary>
    class ColorParser
    {
        /// <summary>
        /// Parse a color from a string
        /// </summary>
        /// <param name="colorStr">The string representing the color</param>
        /// <returns>The color, white if the string cannot be parsed</returns>
        public static Color parse(String colorStr)
        {
            if (!string.IsNullOrEmpty(colorStr))
            {
                //when we have an html string
                if (colorStr.StartsWith("#")) {
                    Color color;
                    return (ColorUtility.TryParseHtmlString(colorStr, out color))? color : Color.white;
                }
                //else try to split the color into separate parts
                string[] colorStrings = colorStr.Split(',');
                if (colorStrings.Length == 3)
                {
                    try
                    {
                        float r = float.Parse(colorStrings[0], CultureInfo.InvariantCulture.NumberFormat);
                        float g = float.Parse(colorStrings[1], CultureInfo.InvariantCulture.NumberFormat);
                        float b = float.Parse(colorStrings[2], CultureInfo.InvariantCulture.NumberFormat);
                        Color color = new Color(r, g, b);
                    }
                    catch
                    {
                        Debug.LogError("[KerbetrotterTools] Invalid color definition");
                    }
                }
            }
            return Color.white;
        }
    }
}
