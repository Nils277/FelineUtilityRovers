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

namespace KerbetrotterTools
{
    class ModuleKerbetrotterCopyRotation : PartModule
    {
        //==================================================
        //Public fields for the configs
        //==================================================

        /// <summary>
        /// The name of the transform which rotation is copied
        /// </summary>
        [KSPField(isPersistant = false)]
        public string fromTransform = string.Empty;

        /// <summary>
        /// The name of the target to which the rotation is applied
        /// </summary>
        [KSPField(isPersistant = false)]
        public string toTransform = string.Empty;

        //==================================================
        //Internal Members
        //==================================================

        //The transform of the target
        private Transform targetTransform;

        //The transform of the source
        private Transform sourceTransform;

        //Saves if the trans form is valid
        private bool valid = false;

        //==================================================
        //Life Cycle
        //==================================================

        //Initialize the meshes and so on
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            targetTransform = KSPUtil.FindInPartModel(transform, toTransform.Replace(" ", string.Empty));
            sourceTransform = KSPUtil.FindInPartModel(transform, fromTransform.Replace(" ", string.Empty));

            valid = (targetTransform != null) & (sourceTransform != null);
        }

        /// <summary>
        /// Updates the rotation of fixed mesh
        /// </summary>
        public void Update()
        {
            if ((HighLogic.LoadedSceneIsFlight) && (valid))
            {
                targetTransform.rotation = sourceTransform.rotation;
            }
        }

    }
}
