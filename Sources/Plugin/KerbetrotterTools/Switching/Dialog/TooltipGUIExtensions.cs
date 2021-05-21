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
using KSP.UI.TooltipTypes;

namespace KerbetrotterTools.Switching.Dialog
{
    /// <summary>
    /// Utilities to simplify use of tooltips with DialogGUI*-based UIs.
    /// </summary>
    public static class TooltipGUIExtensions
    {
        //Static prefab for the tooltips
        private static readonly Tooltip_TitleAndText tooltipPrefab = AssetBase.GetPrefab<Tooltip_TitleAndText>("Tooltip_TitleAndText");

        /// <summary>
        /// Create a tooltip object for a given GameObject, containing both
        /// a title and a subtitle.
        /// </summary>
        /// <param name="gameObj">GameObject to which the tooltip should be added</param>
        /// <param name="title">Highlighted text for the tooltip</param>
        /// <param name="text">Less emphasized text for the tooltip</param>
        public static bool SetTooltip(this GameObject gameObj, string title, string text)
        {
            if (gameObj != null)
            {
                TooltipController_TitleAndText tooltip = (gameObj?.GetComponent<TooltipController_TitleAndText>() ?? gameObj?.AddComponent<TooltipController_TitleAndText>());
                if (tooltip != null)
                {
                    tooltip.prefab = tooltipPrefab;
                    tooltip.prefab.label.alpha = 1.0f;
                    tooltip.titleString = title;
                    tooltip.textString = text;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add a tooltip to to a DialogGUIBase item
        /// </summary>
        /// <param name="gb">DialogGUI* object that needs a tooltip</param>
        /// <returns>
        /// The same object that was passed in
        /// </returns>
        public static DialogGUIBase WithTooltip(DialogGUIBase item, string title = "", string text = "")
        {
            //When there is no description, return
            if (string.IsNullOrEmpty(text))
            {
                return item;
            }

            item.OnUpdate = () => {
                if (item.uiItem != null && item.uiItem.SetTooltip(title, text))
                {
                    item.OnUpdate = () => { };
                }
            };
            return item;
        }
    }
}
