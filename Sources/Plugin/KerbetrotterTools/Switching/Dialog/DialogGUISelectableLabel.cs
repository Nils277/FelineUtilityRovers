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

namespace KerbetrotterTools.Switching.Dialog
{
    /// <summary>
    /// Label that changes its color depending on the selection
    /// </summary>
    class DialogGUISelectableLabel : DialogGUILabel
    {
        #region-------------------------Private Members----------------------

        //Class updating the label
        private LabelUpdater mUpdater;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Constructor of the 
        /// </summary>
        /// <param name="updater">Updater for the label</param>
        /// <param name="style">The style of the part</param>
        /// <param name="expandW">Whether the width should be expanded</param>
        /// <param name="expandH">Whether the height should be expanded</param>
        public DialogGUISelectableLabel(LabelUpdater updater, UIStyle style, bool expandW = false, bool expandH = false) : base(updater.text , style, expandW, expandH)
        {
            mUpdater = updater;
        }

        /// <summary>
        /// Update the selection of the label
        /// </summary>
        /// <param name="selection">The index of the selection</param>
        public void updateSelection(int selection)
        {
            mUpdater.updateSelection(selection);
        }

        /// <summary>
        /// Set the text of the label
        /// </summary>
        /// <param name="text">The new text of the label</param>
        public void setText(String text)
        {
            mUpdater.setText(text);
        }

        #endregion

        #region-----------------------------Classes--------------------------

        /// <summary>
        /// Class to update the label
        /// </summary>
        public class LabelUpdater
        {
            #region-------------------------Private Members----------------------

            //The text of the label
            private String mText = "";

            //The id of the optiion
            private int mId;

            //Whether the option this label belongs to is selectd
            private bool mSelected = false;

            #endregion

            #region-------------------------Public Methods-----------------------

            /// <summary>
            /// Constrcutor of the updater
            /// </summary>
            /// <param name="text">The text to show</param>
            /// <param name="id">The ID of the option</param>
            public LabelUpdater(String text, int id)
            {
                mText = text;
                mId = id;
            }

            /// <summary>
            /// Update the selection
            /// </summary>
            /// <param name="selection">The ID of the selection</param>
            public void updateSelection(int selection)
            {
                mSelected = selection == mId;
            }

            /// <summary>
            /// Set the text of the label
            /// </summary>
            /// <param name="text">The new text</param>
            public void setText(string text)
            {
                mText = text;
            }

            /// <summary>
            /// Get the text of the label
            /// </summary>
            /// <returns>The text of the label</returns>
            public string text()
            {
                if (!mSelected)
                {
                    return mText;
                }
                return "<color=#aff20bff>" + mText + "</color>";

            }

            #endregion
        }

        #endregion
    }
}
