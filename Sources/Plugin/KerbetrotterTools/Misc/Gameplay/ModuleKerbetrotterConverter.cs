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
using KSP.Localization;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Converter extension to limit the converter speed
    /// </summary>
    class ModuleKerbetrotterConverter : ModuleResourceConverter, IModuleInfo
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The production speed of the converter
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Conversion Speed", guiUnits = "%"), UI_FloatRange(minValue = 10f, maxValue = 100f, stepIncrement = 10f)]
        public float productionSpeed = 100;

        #endregion

        #region----------------------------Life Cycle------------------------

        /// <summary>
        /// Translate the field
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Fields["productionSpeed"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.converter.speed");
        }

        #endregion

        #region--------------------------Functionality-----------------------

        /// <summary>
        /// Prepare the recipe with regard to the amount of crew in this module
        /// </summary>
        /// <param name="deltatime">The delta time since the last update</param>
        /// <returns>The conversion recipe</returns>
        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {
            ConversionRecipe recipe = base.PrepareRecipe(deltatime);

            

            if (recipe != null)
            {
                //change the rate of the inputs
                for (int i = 0; i < recipe.Inputs.Count; i++)
                {
                    ResourceRatio res = recipe.Inputs[i];
                    res.Ratio = inputList[i].Ratio * (productionSpeed / 100f);
                    recipe.Inputs[i] = res;
                }
                //change the rate of the outputs
                for (int i = 0; i < recipe.Outputs.Count; i++)
                {
                    ResourceRatio res = recipe.Outputs[i];
                    res.Ratio = outputList[i].Ratio * (productionSpeed / 100f);
                    recipe.Outputs[i] = res;
                }
                //change the value of the requirements
                for (int i = 0; i < recipe.Requirements.Count; i++)
                {
                    ResourceRatio res = recipe.Requirements[i];
                    res.Ratio = reqList[i].Ratio * (productionSpeed / 100f);
                    recipe.Requirements[i] = res;
                }
            }

            return recipe;
        }

        #endregion

        #region---------------------------IModuleInfo------------------------

        /// <summary>
        /// Get the Callback for drawing the module
        /// </summary>
        /// <returns>Callback for drawng the module</returns>
        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        /// <summary>
        /// Get the title of the module
        /// </summary>
        /// <returns>The title of the module</returns>
        public string GetModuleTitle()
        {
            return Localizer.GetStringByTag("#autoLoc_6003053");
        }

        /// <summary>
        /// Get the primary field of the module
        /// </summary>
        /// <returns>The primary field</returns>
        public string GetPrimaryField()
        {
            return null;
        }

        #endregion
    }
}
