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

namespace KerbetrotterTools
{
    /// <summary>
    /// This Class allows to toggle the visibilty of a transform (incluive subtransforms) This also affects the colliders
    /// </summary>
    class ModuleKerbetrotterEditorMesh : PartModule
    {
        
        [KSPField]//the names of the transforms
        public string transformNames = string.Empty;

        [KSPField]//Whether the toggle is available in flight
        public bool availableInFlight = true; 

        [KSPField]//Whether the toggle is available in editor
        public bool availableInEditor = true; 

        //--------------persistent states----------------
        [KSPField(isPersistant = true)]
        public bool transformsVisible = true;

        //the list of models
        List<Transform> transforms;

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
        /// Update the meshes of the part
        /// </summary>
        private void updateMeshes()
        {
            bool visible = false;

            if (HighLogic.LoadedSceneIsFlight)
            {
                visible |= availableInFlight;
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                visible |= availableInEditor;
            }

            int numTransforms = transforms.Count;
            for (int i = 0; i < numTransforms; i++)
            {
                transforms[i].gameObject.SetActive(visible);
            }
        }
    }
}
