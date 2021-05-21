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
using System.Collections.Generic;
using KSP.Localization;
using KerbetrotterTools.Misc.Gameplay;

namespace KerbetrotterTools
{
    /// <summary>
    /// This Class allows to toggle the visibilty of a transform (incluive subtransforms) This also affects the colliders
    /// </summary>
    class ModuleKerbetrotterMeshToggle : ModuleKerbetrotterBaseInteraction
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The names of the transforms
        /// </summary>
        [KSPField]
        public string transformNames = string.Empty;

        /// <summary>
        /// Text to show to hide a mesh
        /// </summary>
        [KSPField]
        public string showMeshString = Localizer.GetStringByTag("#LOC_KERBETROTTER.meshtoggle.show");

        /// <summary>
        /// Text to show to show a mesh
        /// </summary>
        [KSPField]
        public string hideMeshString = Localizer.GetStringByTag("#LOC_KERBETROTTER.meshtoggle.hide");

        //--------------persistent states----------------
        [KSPField(isPersistant = true)]
        public bool transformsVisible = true;

        #endregion

        #region------------------------Private Members-----------------------

        //The list of models
        private List<Transform> transforms;

        //Listener for when the mesh was toggled
        private List<MeshToggleListener> listeners = new List<MeshToggleListener>();

        #endregion

        #region---------------------------Life Cycle-------------------------

        /// <summary>
        /// Find the transforms that can be toggled
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            string[] transformGroupNames = transformNames.Split(',');
            transforms = new List<Transform>();

            //----------------------------------------------------------
            //create the list of transforms to be made toggleble
            //----------------------------------------------------------
            for (int k = 0; k < transformGroupNames.Length; k++)
            {
                transforms.AddRange(part.FindModelTransforms(transformGroupNames[k].Trim()));
            }

            updateMeshes();
        }

        /// <summary>
        /// Free all resources when the part is destroyed
        /// </summary>
        public void OnDestroy()
        {
            listeners.Clear();
        }

        #endregion

        #region------------------------User Interaction----------------------

        /// <summary>
        /// Event that toggles the visibility of the mesh
        /// </summary>
        [KSPEvent(name = "toggleMesh", guiName = "#LOC_KERBETROTTER.meshtoggle.toggle", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = true)]
        public void toggleMesh()
        {
            if (!actionPossible())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(mActionError, 2f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }
            transformsVisible = !transformsVisible;
            updateMeshes();
        }

        #endregion

        #region-------------------------Public Methods-----------------------

        /// <summary>
        /// Add a listener for changes
        /// </summary>
        /// <param name="listener">The listener to add</param>
        public void addListener(MeshToggleListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Remove a listener for updates
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void removeListener(MeshToggleListener listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }

        #endregion

        #region-------------------------Helper Methods-----------------------

        /// <summary>
        /// Update the visibility of the GUI
        /// </summary>
        private void updateGUI()
        {
            //when there is only one model, we do not need to show the controls
            if (transforms.Count < 1)
            {
                Events["toggleMesh"].active = false;
            }
            //when there are two models make the controls appear as a switch between two parts
            else
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    Events["toggleMesh"].active = availableInEditor;
                }
                else if (HighLogic.LoadedSceneIsFlight)
                {
                    Events["toggleMesh"].active = availableInFlight;
                    if (evaOnly)
                    {
                        Events["toggleMesh"].guiActiveUnfocused = availableInFlight;
                        Events["toggleMesh"].externalToEVAOnly = availableInFlight;
                    }
                }
                else
                {
                    Events["toggleMesh"].active = false;
                    return;
                }

                if (transformsVisible)
                {
                    Events["toggleMesh"].guiName = hideMeshString;
                }
                else
                {
                    Events["toggleMesh"].guiName = showMeshString;
                }

            }
        }

        /// <summary>
        /// Update the meshes of the part
        /// </summary>
        private void updateMeshes()
        {
            int numTransforms = transforms.Count;
            for (int i = 0; i < numTransforms; i++)
            {
                transforms[i].gameObject.SetActive(transformsVisible);
            }
            updateGUI();

            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].meshToggled(transformsVisible);
            }
        }

        #endregion

        #region---------------------------Interfaces-------------------------

        /// <summary>
        /// Listener for when a mesh was toggled
        /// </summary>
        public interface MeshToggleListener
        {
            /// <summary>
            /// Called when the mesh visibility was toggled
            /// </summary>
            /// <param name="enabled">WHen true, mesh is visible, else false</param>
            void meshToggled(bool enabled);
        }

        #endregion
    }
}
