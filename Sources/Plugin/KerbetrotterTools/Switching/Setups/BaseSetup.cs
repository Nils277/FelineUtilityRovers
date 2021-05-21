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
using KerbetrotterTools.Misc.Helper;
using System.Globalization;
using UnityEngine;

namespace KerbetrotterTools.Switching.Setups
{
    /// <summary>
    /// Class holding the resource name as well as the ID of the setup
    /// </summary>
    internal class BaseSetup
    {
        #region-------------------------Public Members----------------------

        //The ID of the setup
        public string ID;

        //The GUi visible name of the setup
        public string guiName;

        //The mass change for this setup
        public double massModifier = 0;

        //The cost change for this setup
        public double costModifier = 0;

        //The primary color shown in the change dialog
        public Color primaryColor;

        //The secondary color shown in the change dialog
        public Color secondaryColor;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Constructor of the setup
        /// </summary>
        /// <param name="node">The config not to contruct the setup from</param>
        public BaseSetup(ConfigNode node)
        {
            //load the ID of the mode
            if (node.HasValue("ID"))
            {
                ID = node.GetValue("ID");
            }

            //load the ID of the mode
            if (node.HasValue("visibleName"))
            {
                guiName = node.GetValue("visibleName");
            }
            else if (node.HasValue("name"))
            {
                guiName = node.GetValue("name");
            }
            else
            {
                guiName = ID;
            }

            if (node.HasValue("additionalCost"))
            {
                costModifier = float.Parse(node.GetValue("additionalCost"), CultureInfo.InvariantCulture.NumberFormat);
            }

            if (node.HasValue("additionalMass"))
            {
                massModifier = float.Parse(node.GetValue("additionalMass"), CultureInfo.InvariantCulture.NumberFormat);
            }

            //load the ID of the mode
            if (node.HasValue("primaryColor"))
            {
                primaryColor = ColorParser.parse(node.GetValue("primaryColor"));
            }

            //load the ID of the mode
            if (node.HasValue("secondaryColor"))
            {
                secondaryColor = ColorParser.parse(node.GetValue("secondaryColor"));
            }

            if (primaryColor == null && secondaryColor != null)
            {
                primaryColor = secondaryColor;
            }
            else if (secondaryColor == null && primaryColor != null)
            {
                secondaryColor = primaryColor;
            }
            else if (primaryColor == null && secondaryColor == null)
            {
                primaryColor = Color.white;
                secondaryColor = Color.white;
            }
        }

        /// <summary>
        /// Get the info of the setup. Should be overwritten by sub-classed
        /// </summary>
        /// <param name="name">When true, the name should be shown, else false</param>
        /// <returns>The info shown for this setup</returns>
        public virtual string getInfo(bool name = true)
        {
            return "";
        }

        #endregion
    }
}
