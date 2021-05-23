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
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbetrotterTools.Switching.Setups;
using System;

using static KerbetrotterTools.Switching.Dialog.DialogGUISelectableLabel;
using static KerbetrotterTools.Switching.Dialog.TooltipGUIExtensions;

namespace KerbetrotterTools.Switching.Dialog
{
    /// <summary>
    /// Dialog showing the swichting options to the user
    /// </summary>
    /// <typeparam name="T">The type if the switched setup</typeparam>
    class SwitcherDialog<T> where T: BaseSetup
    {
        #region-------------------------Private Members----------------------

        //The currently selected setup
        private int mSelection = -1;

        //The switch that should be switched
        private ModuleKerbetrotterSwitch<T> mSwitch;

        //The dialog
        private PopupDialog mDialog;

        //The available options
        private List<DialogGUIBase> mOptions = new List<DialogGUIBase>();

        //The list of possible setups
        private List<T> mSetups;

        //The string showin the costs or change for switching
        private string mCostsString = string.Empty;

        //Holds whether switching is possible
        private bool mCanSwitch = true;

        //Mask for the color of the setups
        private Color[] mask;

        //Background for selected options
        private Color32[] back;

        //Arrow between current resource and next resource
        private Texture2D arrow;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Contructors of the this dialog
        /// </summary>
        /// <param name="moduleSwitch">The module that should be switched</param>
        public SwitcherDialog(ModuleKerbetrotterSwitch<T> moduleSwitch)
        {
            mSwitch = moduleSwitch;

            //load the mask for the icons
            mask = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Assets/Textures/icon_resource", false).GetPixels();
            back = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Assets/Textures/selected_bg", false).GetPixels32();
            arrow = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Assets/Textures/arrow", false);
        }

        /// <summary>
        /// Show the dialog for switching
        /// </summary>
        /// <param name="setups">The list of available setups</param>
        public void show(List<T> setups)
        {
            GameEvents.onPartActionUIDismiss.Add(onPAWDismiss);

            mSetups = setups;
            mCanSwitch = mSwitch.canSwitch();
            mCostsString = string.Empty;
            UISkinDef skin = UISkinManager.defaultSkin;

            //create the dialog itself
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();

            Color32[] trans = new Color32[back.Length];
            for (int i = 0; i < trans.Length; i++)
            {
                trans[i] = Color.clear;
            }

            UIStyle labelStyle = new UIStyle(skin.window);
            labelStyle.fontSize = 14;
            labelStyle.alignment = TextAnchor.MiddleLeft;

            UIStyle optionStyle = new UIStyle(skin.button);
            optionStyle.fontSize = 14;
            optionStyle.alignment = TextAnchor.MiddleLeft;

            UIStyle acceptStyle = new UIStyle(skin.button);
            acceptStyle.normal = skin.box.normal;

            dialog.Add(new DialogGUILabel("<color=#f0f0f0> " + mSwitch.part.partInfo.title + "</color>", labelStyle, false));

            int count = setups.Count;
            DialogGUIBase[] scrollList = new DialogGUIBase[count + 1];
            scrollList[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.PreferredSize, true);

            //Iterate over all switchable settings
            for (int i = 0; i < count; i++)
            {
                DialogGUISelectableLabel label = new DialogGUISelectableLabel(new LabelUpdater(setups[i].guiName, i), optionStyle, true);
                DialogGUIImage icon = new DialogGUIImage(new Vector2(30, 30), new Vector2(0, 0), Color.white, createColorPreview(setups[i], mask));
                icon.size = new Vector2(28, 28);
                DialogGUIBase button;
                if (i == mSwitch.SelectedSetup)
                {
                    button = WithTooltip(new DialogGUIOptionButton(label, i, 270, 32, generateBackground(trans), back, new DialogGUIBase[] {
                    new DialogGUIHorizontalLayout(false, false, 10, new RectOffset(12,12,4,4), TextAnchor.MiddleCenter,
                    new DialogGUIBase[] { icon, label })}), setups[i].guiName, setups[i].getInfo(false));
                }
                else
                {
                    button = WithTooltip(new DialogGUIOptionButton(label, switchTo, i, 270, 32, generateBackground(trans), trans, back, new DialogGUIBase[] {
                    new DialogGUIHorizontalLayout(false, false, 10, new RectOffset(12,12,4,4), TextAnchor.MiddleCenter,
                    new DialogGUIBase[] { icon, label })}), setups[i].guiName, setups[i].getInfo(false));
                }

                mOptions.Add(button);
                scrollList[i + 1] = button;
            }

            dialog.Add(new DialogGUIScrollList(new Vector2(250, 230), false, true, 
                new DialogGUIVerticalLayout(10, 100, 0, new RectOffset(1, 1, 1, 1), TextAnchor.UpperLeft, scrollList)));

            String guiName = setups[mSwitch.SelectedSetup].guiName;
            
            //when there are 
            if (HighLogic.LoadedSceneIsEditor || mSwitch.switchingCosts())
            {
                dialog.Add(new DialogGUIVerticalLayout(50, 50, 4, new RectOffset(6, 6, 3, 0), TextAnchor.UpperLeft, new DialogGUIBase[] {
                    new DialogGUILabel(Localizer.Format("#LOC_KERBETROTTER.switch.change"), labelStyle),
                    new DialogGUIHorizontalLayout(false, false, 8, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft, new DialogGUIBase[] {
                        new DialogGUILabel(guiName, skin.textArea),
                        new DialogGUIImage(new Vector2(17,9), new Vector2(0,0), Color.white, arrow),
                        new DialogGUILabel(getSelectedName, skin.textArea),
                    }),
                    new DialogGUISpace(3),
                    HighLogic.LoadedSceneIsEditor? new DialogGUILabel(getInfluence, skin.textArea) : new DialogGUILabel(getCosts, labelStyle)}
                    )
                );
            }
            else
            {
                dialog.Add(new DialogGUIVerticalLayout(50, 50, 4, new RectOffset(6, 6, 3, 0), TextAnchor.UpperLeft, new DialogGUIBase[] {
                    new DialogGUILabel("Change: ", labelStyle),
                    new DialogGUIHorizontalLayout(false, false, 8, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft, new DialogGUIBase[] {
                        new DialogGUILabel(guiName, skin.textArea),
                        new DialogGUIImage(new Vector2(17,9), new Vector2(0,0), Color.white, arrow),
                        new DialogGUILabel(getSelectedName, skin.textArea)}
                        )
                    })
                );
            }

            if (mCanSwitch && !String.IsNullOrEmpty(mSwitch.getWarning()))
            {
                dialog.Add(new DialogGUILabel("<color=#ff9000>  " + mSwitch.getWarning() +  "</color>", skin.textField));
            }

            //buttons to cancel or apply the the resource change
            dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBase[]
            {
                new DialogGUIButton(Localizer.Format("#autoLOC_174783"), dismiss, 110,30, false),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton(Localizer.Format(mSwitch.needsPreparation()? "#autoLOC_465260" : "#LOC_KERBETROTTER.switch.configure"), apply, enabled, 110,30, false, acceptStyle),
            }));

            mDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog(
               "SwitcherDialog",
               "",
               mSwitch.switchingTitle,
               skin,
               new Rect(0.5f, 0.5f, 300, -1),
               dialog.ToArray()), false, skin);
            mDialog.OnDismiss = onDismiss;

            if (setups.Count == 2)
            {
                switchTo(1 - mSwitch.SelectedSetup);
            }
        }

        /// <summary>
        /// Get whether the dialog is showing at the moment
        /// </summary>
        /// <returns>When true, dialog is showing, else false</returns>
        public bool isShowing()
        {
            return mDialog != null;
        }

        /// <summary>
        /// Callback used for when the PAW of the part closes
        /// </summary>
        /// <param name="part"></param>
        public void onPAWDismiss(Part part)
        {
            if (part == mSwitch.part)
            {
                dismiss();
            }
        }

        #endregion

        #region------------------------User Interaction----------------------

        /// <summary>
        /// Called when the cancel button is pressed
        /// </summary>
        public void dismiss()
        {
            mOptions.Clear();
            mSelection = -1;
            mCostsString = string.Empty;
            if (mDialog != null)
            {
                mDialog.Dismiss();
                mDialog = null;
            }
        }

        /// <summary>
        /// Called when the configure button is clicked
        /// </summary>
        private void apply()
        {
            if (mSwitch.needsPreparation())
            {
                SwitchingProgressDialog<T> dialog = new SwitchingProgressDialog<T>(mSwitch, mSelection);
                dialog.show(mSetups[mSwitch.SelectedSetup].guiName, mSetups[mSelection].guiName);
            }
            else
            {
                mSwitch.updateSetup(mSelection);
            }
            dismiss();
        }

        /// <summary>
        /// Method called when the user clicks on a switching option
        /// </summary>
        /// <param name="index">The index of the selected option</param>
        private void switchTo(int index)
        {
            mSelection = index;
            mCostsString = "";
            foreach (DialogGUIOptionButton btn in mOptions)
            {
                btn.updateSeleced(mSelection);
            }
        }

        #endregion

        #region------------------------Callbacks for UI----------------------

        /// <summary>
        /// Get the name of the selected setup
        /// </summary>
        /// <returns>The name of the selected setup</returns>
        public string getSelectedName()
        {
            if (mSelection == -1)
            {
                return "- please select -";
            }
            return mSetups[mSelection].guiName;
        }

        /// <summary>
        /// Get whether the configure button is enabled
        /// </summary>
        /// <returns>true when the button is enabled, else false</returns>
        private bool enabled()
        {
            return mCanSwitch && mSelection != -1;
        }

        /// <summary>
        /// Get the influence the selected setup will have (used in the editor)
        /// </summary>
        /// <returns>The influence of the setup</returns>
        public string getInfluence()
        {
            if (string.IsNullOrEmpty(mCostsString))
            {
                string costs;
                string mass;
                if (mSelection == -1)
                {
                    costs = "-";
                    mass = "-";
                }
                else
                {
                    double costDiff = (mSetups[mSelection].costModifier - mSetups[mSwitch.SelectedSetup].costModifier);
                    if (Math.Abs(costDiff) < 0.001)
                    {
                        costs = Localizer.Format("#autoLOC_8004183");
                    }
                    else
                    {
                        costs = (costDiff > 0 ? "+" : "") + costDiff.ToString("0.00");
                    }


                    double massDiff = mSetups[mSelection].massModifier - mSetups[mSwitch.SelectedSetup].massModifier;
                    double absMassDiff = Math.Abs(massDiff);
                    String sign = massDiff > 0 ? "+" : "-";
                    if (absMassDiff < 0.00001)
                    {
                        mass = Localizer.Format("#autoLOC_8004183");
                    }
                    else if (absMassDiff < 1)
                    {
                        mass = Localizer.Format("#autoLOC_5050023", sign + absMassDiff.ToString("0.000"));
                    }
                    else if (absMassDiff < 10)
                    {
                        mass = Localizer.Format("#autoLOC_5050023", sign + absMassDiff.ToString("0.00"));
                    }
                    else
                    {
                        mass = Localizer.Format("#autoLOC_5050023", sign + absMassDiff.ToString("0.0"));
                    }
                }

                StringBuilder info = new StringBuilder();
                info.Append("<b><color=#acfffc>").Append(Localizer.Format("#autoLOC_900529")).Append(": ").Append(mass).AppendLine("</color>");
                info.Append("<color=#ffd200>").Append(Localizer.Format("#autoLOC_900528")).Append(": ").Append(costs).Append("</color></b>");
                mCostsString = info.ToString();
            }

            return mCostsString;
        }

        /// <summary>
        /// Get the string showing the costs of switching (in flight)
        /// </summary>
        /// <returns>The string showign the costs of switching</returns>
        private string getCosts()
        {
            if (string.IsNullOrEmpty(mCostsString))
            {
                mCostsString = mSwitch.getCostsDescription();
            }
            return mCostsString;
        }

        /// <summary>
        /// Called when the dialog is dismissed, remove from events
        /// </summary>
        public void onDismiss()
        {
            GameEvents.onPartActionUIDismiss.Remove(onPAWDismiss);
        }

        #endregion

        #region-------------------------Helper Methods-----------------------

        /// <summary>
        /// Generate the sprite for the background of an option
        /// </summary>
        /// <param name="trans">The texture for the normal state</param>
        /// <returns>The sprite for the background of the option</returns>
        private Sprite generateBackground(Color32[] trans)
        {
            Texture2D transparentTex = new Texture2D(24, 32);
            transparentTex.SetPixels32(trans);
            transparentTex.Apply();
            return Sprite.Create(transparentTex, new Rect(0, 0, 24, 32), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }

        /// <summary>
        /// Create a color preview of a setup
        /// </summary>
        /// <param name="setup">The setup</param>
        /// <param name="mask">The mask to create the preview image</param>
        /// <returns></returns>
        private Texture2D createColorPreview(BaseSetup setup, Color[] mask)
        {
            Color[] res = new Color[mask.Length];
            for (int x = 0; x < 28; x++)
            {
                for (int y = 0; y < 28; y++)
                {
                    int i = y * 28 + x;
                    res[i] = Color.Lerp(setup.primaryColor, setup.secondaryColor, mask[i].r);
                    if (y < 15)
                    {
                        float darken = Math.Max(0.7f, 1.0f - ((15 - y) / 12.0f));
                        res[i].r = darken * res[i].r;
                        res[i].g = darken * res[i].g;
                        res[i].b = darken * res[i].b;
                    }
                    else if (y > 18)
                    {
                        float lighten = (y - 18) / 8.0f;
                        res[i].r = Math.Min(1.0f, res[i].r + lighten);
                        res[i].g = Math.Min(1.0f, res[i].g + lighten);
                        res[i].b = Math.Min(1.0f, res[i].b + lighten);
                    }
                    res[i].a = mask[i].a;
                }
            }
            Texture2D image = new Texture2D(28, 28);
            image.SetPixels(res);
            image.Apply(false);
            return image;
        }

        #endregion
    }
}
