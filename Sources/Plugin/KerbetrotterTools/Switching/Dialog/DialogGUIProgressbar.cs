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
using UnityEngine;

namespace KerbetrotterTools.Switching.Dialog
{
    /// <summary>
    /// UI element showing a progress bar, currently with a fixed height
    /// </summary>
    class DialogGUIProgressbar : DialogGUIImage
    {
        #region-------------------------Private Members----------------------

        //The progress for the start
        private Color[] mProgressStart = new Color[78];

        //The progress for the end
        private Color[] mProgressMid = new Color[13];

        //The progress in the middle
        private Color[] mProgressEnd = new Color[78];

        //The last progress
        private int lastProgress = 0;

        //The current progress
        private Func<float> mProgressCallback;

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Contructor of the Progress bar
        /// </summary>
        /// <param name="width">The width of the part</param>
        /// <param name="getProgress">The callback to get the progress between 0 and 1</param>
        public DialogGUIProgressbar(int width, Func<float> getProgress) : base(new Vector2(width, 13), new Vector2(0,0), Color.white, new Texture2D(width, 13))
        {
            mProgressCallback = getProgress;
            Texture2D background = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Assets/Textures/progress_back", false);
            Texture2D progress = GameDatabase.Instance.GetTexture("KerbetrotterLtd/000_KerbetrotterTools/Assets/Textures/progress_active", false);

            Texture2D tex = (Texture2D)image;
            Color[] backMid = new Color[13];
            //prepare textures for the progress bar
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    int index = y * 6 + x;
                    mProgressStart[index] = progress.GetPixel(x, y);
                    mProgressEnd[index] = progress.GetPixel(x + 7, y);

                    //draw the background
                    tex.SetPixel(x, y, background.GetPixel(x, y));
                    tex.SetPixel(x + 266, y, background.GetPixel(x + 7, y));
                }
                mProgressMid[y] = progress.GetPixel(6, y);
                backMid[y] = background.GetPixel(6, y);
            }
            //draw the background
            for (int x = 6; x < 266; x++)
            {
                tex.SetPixels(x, 0, 1, 13, backMid);
            }
            tex.Apply();
        }

        /// <summary>
        /// Update the progress bar und draw the new progress if it changed
        /// </summary>
        public override void Update()
        {
            base.Update();
            float progress = Math.Min(1.0f, Math.Max(0.0f, mProgressCallback()));

            //update the progress bar when it has changed
            int pos = (int)(progress * 272);
            if (pos > lastProgress)
            {
                Texture2D tex = (Texture2D)image;
                for (int x = lastProgress; x < pos; x++)
                {
                    //the beginning of the line
                    if (x < 6)
                    {
                        for (int y = 0; y < 13; y++)
                        {
                            int index = y * 6 + x;
                            tex.SetPixel(x, y, mProgressStart[index]);
                        }
                    }
                    //in the center
                    else if (x < 266)
                    {
                        tex.SetPixels(x, 0, 1, 13, mProgressMid);
                    }
                    //at the end
                    else
                    {
                        for (int y = 0; y < 13; y++)
                        {
                            int index = y * 6 + (x-266);
                            tex.SetPixel(x, y, mProgressEnd[index]);
                        }
                    }
                }
                tex.Apply();
            }
        }

        #endregion
    }
}
