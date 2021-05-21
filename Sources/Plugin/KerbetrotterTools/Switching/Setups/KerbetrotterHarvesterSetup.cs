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

namespace KerbetrotterTools.Switching.Setups
{
    /// <summary>
    /// Setup for a resource harvester
    /// </summary>
    class KerbetrotterHarvesterSetup : BaseSetup
    {
        #region-------------------------Private Members----------------------

        //The name of the resource used by the harvester
        public string resourceName;

        #endregion

        #region-------------------------Public Methods-----------------------

        public KerbetrotterHarvesterSetup(ConfigNode node) : base(node)
        {
            //load the name of the resource to load
            if (node.HasValue("ResourceName"))
            {
                resourceName = node.GetValue("ResourceName");
                //Get the display name
                PartResourceDefinition def = PartResourceLibrary.Instance.resourceDefinitions[resourceName];
                if (def != null)
                {
                    guiName = def.displayName;
                }
                //Get the resource colors for the harvester
                KerbetrotterResourceColor color = ResourceColors.get(resourceName);
                if (color != null)
                {
                    primaryColor = color.Primary;
                    secondaryColor = color.Secondary;
                }
            }
        }

        #endregion
    }
}
