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

namespace KerbetrotterTools.Switching.Dialog
{
    /// <summary>
    /// Class for an option in a list, can toggle between checked and unchecked
    /// </summary>
    class DialogGUIOptionButton : DialogGUIButton
    {
        #region-------------------------Private Members----------------------

        //The index of this option
        private int mIndex = 0;

        //when true, this option is selection
        private bool mSelected = false;

        //Callback for the selection of this module
        private Callback<int> mCallback;

        //The texture to use when this option is selected
        private Color32[] mSelectedTex;

        //The texture to use when this option is not selected
        private Color32[] mNormalTex;

        //Tne label that is shown withing this option
        private DialogGUISelectableLabel mLabel;

        //Holds whether this option is enabled or not
        private bool disabled = false;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Constructor of the option
        /// </summary>
        /// <param name="label">The label with the text</param>
        /// <param name="onSelected">Callback to call when this button is clicked</param>
        /// <param name="index">The index of this option</param>
        /// <param name="w">The width of the UI part</param>
        /// <param name="h">The height of this UI part</param>
        /// <param name="background">The sprite containing the background of this UI part</param>
        /// <param name="normal">The texture to use when this option is not selected</param>
        /// <param name="selected">The texture to use when this option is not selected</param>
        /// <param name="options">Child UI parts</param>
        public DialogGUIOptionButton(DialogGUISelectableLabel label, Callback<int> onSelected, int index, float w, float h, Sprite background,
            Color32[] normal, Color32[] selected, params DialogGUIBase[] options) : base("", null, w, h, false, options)
        {
            mIndex = index;
            mCallback = onSelected;
            image = background;
            mSelectedTex = selected;
            mNormalTex = normal;
            mLabel = label;
        }

        /// <summary>
        /// Constructor of the option
        /// </summary>
        /// <param name="label">The label with the text</param>
        /// <param name="index">The index of this option</param>
        /// <param name="w">The width of the UI part</param>
        /// <param name="h">The height of this UI part</param>
        /// <param name="background">The sprite containing the background of this UI part</param>
        /// <param name="normal">The texture to use when this option is not selected</param>
        /// <param name="selected">The texture to use when this option is not selected</param>
        /// <param name="options">Child UI parts</param>
        public DialogGUIOptionButton(DialogGUISelectableLabel label, int index, float w, float h, Sprite background,
            Color32[] selected, params DialogGUIBase[] options) : base("", null, w, h, false, options)
        {
            mIndex = index;
            mSelectedTex = selected;
            mLabel = label;
            image = background;
            OptionInteractableCondition = isSelectable;
            image.texture.SetPixels32(selected);
            image.texture.Apply();
            disabled = true;
        }

        /// <summary>
        /// Called when the button was clicked
        /// </summary>
        public override void OptionSelected()
        {
            mCallback?.Invoke(mIndex);
        }

        /// <summary>
        /// Update the selected state of this option
        /// </summary>
        /// <param name="selected">The ID of the selected option</param>
        public void updateSeleced(int selected)
        {
            if (!disabled)
            {
                mLabel.updateSelection(selected);

                bool old = mSelected;
                mSelected = selected == mIndex;
                if (old != mSelected)
                {
                    image.texture.SetPixels32(mSelected ? mSelectedTex : mNormalTex);
                    image.texture.Apply();
                }
            }
        }

        #endregion

        #region-------------------------Private Methods----------------------

        private bool isSelectable()
        {
            return false;
        }

        #endregion
    }
}
