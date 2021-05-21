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
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools.Switching.Dialog
{
    /// <summary>
    /// Dialog showing the progress of the switching progress, e.g. the preparation for switching
    /// </summary>
    /// <typeparam name="T">The setup type</typeparam>
    class SwitchingProgressDialog<T> where T : BaseSetup
    {
        #region-------------------------Private Members----------------------

        //The selection the the module should be switched to
        private int mSelection = -1;

        //The switching module
        private ModuleKerbetrotterSwitch<T> mSwitch;

        //The dialog that is shown
        private PopupDialog mDialog;

        //The dialog that is shown
        private bool mInProgress;

        //The progress title:
        private string mProgressString = Localizer.Format("#autoLOC_475347");

        //The state of the progress to show
        private string mProgressState = "";

        #endregion

        #region-------------------------Public Methods-----------------------

        //Contructors of the this dialog
        public SwitchingProgressDialog(ModuleKerbetrotterSwitch<T> moduleSwitch, int selection)
        {
            mSwitch = moduleSwitch;
            mSelection = selection;
        }

        /// <summary>
        /// Method to show the dialg
        /// </summary>
        /// <param name="setups">The possible setups</param>
        /// <param name="message">The message to show to to the user</param>
        public void show(string from, string to)
        {
            //create the dialog itself
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            UISkinDef skin = UISkinManager.defaultSkin;

            //style of the main label
            UIStyle labelStyle = new UIStyle(skin.window);
            labelStyle.fontSize = 14;
            labelStyle.alignment = TextAnchor.MiddleLeft;

            UIStyle acceptStyle = new UIStyle(skin.button);
            acceptStyle.normal = skin.box.normal;

            Texture2D arrow = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Assets/Textures/arrow", false);

            //Toggle button and progress
            dialog.Add(new DialogGUIVerticalLayout(false, false, 5, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft,
                    new DialogGUIBase[] {
                        new DialogGUIHorizontalLayout(false, false, 8, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft, new DialogGUIBase[] {
                            new DialogGUILabel(from, skin.textArea),
                            new DialogGUIImage(new Vector2(17,9), new Vector2(0,0), Color.white, arrow),
                            new DialogGUILabel(to, skin.textArea),
                        }),
                        new DialogGUILabel("<color=#ff9000> " + mSwitch.getWarning() + "</color>", skin.textField, true),   //message
                        new DialogGUIProgressbar(272, getProgress),                                                         //progress
                        new DialogGUILabel(getStatus, skin.textArea, true),                                                 //message
                        new DialogGUIHorizontalLayout(true, false, 0.0f, new RectOffset(4, 4, 4, 0), TextAnchor.MiddleCenter, new DialogGUIBase[]
                        {
                            new DialogGUIButton(Localizer.Format("#autoLOC_174783"), onDismiss, 110,30, true),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton(Localizer.Format("#LOC_KERBETROTTER.switch.configure"), apply, getEnabled, 110, 30, false, acceptStyle),
                        })
                    })
            );
            
            //buttons to cancel or apply the the resource change
            mDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog(
               "SwitcherDialog",
               "",
               mSwitch.switchingTitle,
               skin,
               new Rect(0.5f, 0.5f, 300, -1),
               dialog.ToArray()), false, skin);
            mDialog.OnDismiss = onDismiss;
        }

        #endregion

        #region------------------------User Interaction----------------------

        /// <summary>
        /// Method called when the cancel button was clicked
        /// </summary>
        public void onDismiss()
        {
            mSwitch.abortPreparation();
        }

        /// <summary>
        /// Method called when the configure button was clicked
        /// </summary>
        public void apply()
        {
            mInProgress = true;
            mProgressState = mSwitch.switchingProgress;
            mSwitch.startPreparation();
        }

        #endregion-
    
        #region------------------------Callbacks for UI----------------------

        /// <summary>
        /// Get whether the apply button should be enabled
        /// </summary>
        /// <returns>When true, button should be enabled, else false</returns>
        public bool getEnabled()
        {
            return !mInProgress;
        }

        /// <summary>
        /// Get the string displaying the status of the switching
        /// </summary>
        /// <returns></returns>
        public string getStatus()
        {
            return " " + mProgressString + " " + mProgressState;
        }

        /// <summary>
        /// Get the progress of the switching
        /// </summary>
        /// <returns>The progress between 0 and 1</returns>
        public float getProgress()
        {
            if (mInProgress)
            {
                float progress = mSwitch.preparationProgress();
                if (progress >= 1.0f)
                {
                    mInProgress = false;
                    mSwitch.updateSetup(mSelection);
                    mDialog.Dismiss();
                }
                return progress;
            }
            return 0.0f;
        }

        #endregion
    }
}
