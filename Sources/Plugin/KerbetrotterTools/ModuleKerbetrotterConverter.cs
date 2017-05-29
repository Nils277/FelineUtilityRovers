using KSP.Localization;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterConverter : ModuleResourceConverter, IModuleInfo
    {

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Conversion Speed", guiUnits = "%"), UI_FloatRange(minValue = 10f, maxValue = 100f, stepIncrement = 10f)]
        public float productionSpeed = 100;

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetModuleTitle()
        {
            return Localizer.GetStringByTag("#autoLoc_6003053");
        }

        public string GetPrimaryField()
        {
            return null;
        }

        /// <summary>
        /// Translate the field
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Fields["productionSpeed"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.converter.speed");
        }

        // Prepare the recipe with regard to the amount of crew in this module
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
    }
}
