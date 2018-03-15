/*
 * Copyright (C) 2018 Nils277 (https://github.com/Nils277)
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

namespace KerbetrotterTools
{
    /// <summary>
    /// This Class allows to toggle the visibilty of a transform (incluive subtransforms) This also affects the colliders
    /// </summary>
    class ModuleKerbetrotterMeshToggle : PartModule
    {
        
        [KSPField]//the names of the transforms
        public string transformNames = string.Empty;

        [KSPField]//Text to show to hide a mesh
        public string showMeshString = Localizer.GetStringByTag("#LOC_KERBETROTTER.meshtoggle.show");

        [KSPField]//Text to show to show a mesh
        public string hideMeshString = Localizer.GetStringByTag("#LOC_KERBETROTTER.meshtoggle.hide");

        [KSPField]//Whether the toggle is available in flight
        public bool availableInFlight = true; 

        [KSPField]//Whether the toggle is available in editor
        public bool availableInEditor = true; 

        //--------------persistent states----------------
        [KSPField(isPersistant = true)]
        public bool transformsVisible = true;

        //the list of models
        List<Transform> transforms;

        //saves whether the visibility has been updated yet or not
        public bool initialized = false;

        /// <summary>
        /// Find the transforms that can be toggled
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Events["toggleMesh"].guiName = Localizer.GetStringByTag("#LOC_KERBETROTTER.meshtoggle.toggle");

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
        /// The update method of the partmodule
        /// </summary>
        /*public void Update()
        {
            if (!initialized)
            {
                updateMeshes();
                initialized = true;
            }
        }*/

        /// <summary>
        /// Event that toggles the visibility of the mesh
        /// </summary>
        [KSPEvent(name = "toggleMesh", guiName = "Toggle Mesh", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = true)]
        public void toggleMesh()
        {
            transformsVisible = !transformsVisible;
            updateMeshes();
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
        }
    }
}
