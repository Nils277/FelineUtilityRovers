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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KerbetrotterTools.Switching.Setups
{
    /// <summary>
    /// Class that holds all the data for a switchable resource, including mass modifier and cost modifier
    /// </summary>
    class KerbetrotterResourceSetup : BaseSetup
    {
        #region-------------------------Private Members----------------------

        //holds whether the venting should be animated
        public bool animateVenting = true;

        //holds the definitions of the resources
        public KerbetrotterResourceDefinition[] resources;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Constructor of the resource setup
        /// </summary>
        /// <param name="node">The config not to contruct the setup from</param>
        public KerbetrotterResourceSetup(ConfigNode node, float multiplier) : base(node)
        {
            if (node.HasValue("animateVenting"))
            {
                animateVenting = node.GetValue("animateVenting").ToLower() == "true";
            }
            ConfigNode[] resourceSubNodes = node.GetNodes("RESOURCE");
            List<KerbetrotterResourceDefinition> newResources = new List<KerbetrotterResourceDefinition>();

            resources = new KerbetrotterResourceDefinition[resourceSubNodes.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                KerbetrotterResourceDefinition resourceDefinition = new KerbetrotterResourceDefinition(resourceSubNodes[i], multiplier);
                resources[i] = resourceDefinition;
                costModifier += resourceDefinition.cost;
            }

            //when no other color is specified use the resource colors if available
            if (!node.HasValue("primaryColor") && !node.HasValue("secondaryColor") && (resources.Length > 0))
            {
                KerbetrotterResourceColor colors1 = ResourceColors.get(resources[0].name);
                if (colors1 != null)
                {
                    primaryColor = colors1.Primary;
                    if (resources.Length > 1 && ResourceColors.has(resources[1].name))
                    {
                        secondaryColor = ResourceColors.get(resources[1].name).Primary;
                    }
                    else
                    {
                        secondaryColor = colors1.Secondary;
                    }
                }
            }
        }

        /// <summary>
        /// Get the info of the setup. Should be overwritten by sub-classed
        /// </summary>
        /// <param name="name">When true, the name should be shown, else false</param>
        /// <returns>The info shown for this setup</returns>
        public override string getInfo(bool name = true)
        {
            StringBuilder info = new StringBuilder();
            if (resources.Length > 1)
            {
                if (name)
                {
                    info.AppendLine(guiName);
                }

                foreach (KerbetrotterResourceDefinition resource in resources)
                {
                    if (resource.maxAmount > 0)
                    {
                        info.Append("<color=#35DC35FF>");
                        if (name)
                        {
                            info.Append("      ");
                        }
                        info.Append(resource.maxAmount.ToString("0.0"));
                        info.Append("</color>");
                        info.Append(" ");
                    }
                    info.AppendLine(resource.name);
                }
            }
            else if (resources.Length == 1)
            {
                if (resources[0].maxAmount > 0)
                {
                    info.Append("<color=#35DC35FF>");
                    info.Append(resources[0].maxAmount.ToString("0.0"));
                    info.Append("</color>");
                    info.Append(" ");
                }
                info.AppendLine(resources[0].name);
            }

            return info.ToString();
        }

        #endregion

        #region-----------------------------Classes--------------------------

        /// <summary>
        /// Class that holds the definition for one resource.
        /// </summary>
        public class KerbetrotterResourceDefinition
        {
            #region-------------------------Public Members----------------------

            //The name of the resource
            public string name;

            //The amount of the resource
            public double amount;

            //The maximal amount of the resource
            public double maxAmount;

            //Whether the resource is tweakable or not
            public bool isTweakable = true;

            //The cost of the resource
            public float cost = 0;

            #endregion

            #region-------------------------Public Methods-----------------------

            public KerbetrotterResourceDefinition(ConfigNode node, double multiplier)
            {
                if (node.HasValue("name"))
                {
                    name = node.GetValue("name");
                }

                //check of this resource exist
                if (PartResourceLibrary.Instance.resourceDefinitions[name] != null)
                {
                    if (node.HasValue("amount"))
                    {
                        amount = float.Parse(node.GetValue("amount"), CultureInfo.InvariantCulture.NumberFormat) * multiplier;
                    }
                    if (node.HasValue("maxAmount"))
                    {
                        maxAmount = float.Parse(node.GetValue("maxAmount"), CultureInfo.InvariantCulture.NumberFormat) * multiplier;
                    }

                    if (node.HasValue("isTweakable"))
                    {
                        isTweakable = bool.Parse(node.GetValue("isTweakable"));
                    }
                    cost = (float)(maxAmount * PartResourceLibrary.Instance.resourceDefinitions[name].unitCost) * (float)multiplier;
                }
            }

            #endregion
        }
        #endregion
    }
}