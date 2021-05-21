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
using KerbetrotterTools.Switching.Setups;
using KSP.Localization;

namespace KerbetrotterTools
{
    class KerbetrotterConverterSetup : BaseSetup
    {
        #region-------------------------Public Members----------------------

        //The name of the converter
        private string[] converters = null;

        //whether this profile is the default one
        private bool isDefault = false;

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Constructor of the pid profile
        /// </summary>
        /// <param name="node">The config node to load from</param>
        public KerbetrotterConverterSetup(ConfigNode node) : base(node)
        {
            //load the name of the converter
            if (node.HasValue("converter"))
            {
                converters = node.GetValue("converter").Split(',');
                for (int i = 0; i < converters.Length; i++)
                {
                    converters[i] = Localizer.Format(converters[i]);
                }
            }

            //load whether the mode is default
            if (node.HasValue("isDefault"))
            {
                isDefault = bool.Parse(node.GetValue("isDefault"));
            }
        }

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Get whether this setupt contains a converter
        /// </summary>
        /// <param name="converter"></param>
        /// <returns>True when the setup contains the converte, else false</returns>
        public bool contains(string converter)
        {
            if (converters == null)
            {
                return false;
            }
            foreach (string c in converters)
            {
                if (c.Equals(converter)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the info of the setup. Should be overwritten by sub-classed
        /// </summary>
        /// <param name="name">When true, the name should be shown, else false</param>
        /// <returns>The info shown for this setup</returns>
        public override string getInfo(bool name = true)
        {
            string info = Localizer.Format("#autoLOC_7001227") + ":\n";
            foreach (string converter in converters)
            {
                info += "  " + converter + "\n";
            }
            return info;
        }

        #endregion
    }
}
