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
    /// <summary>
    /// Hide specified transforms
    /// </summary>
    class ModuleKerbetrotterMeshHide : PartModule
    {
        
        [KSPField]//the names of the transforms
        public string transformNames = string.Empty;

        /// <summary>
        /// Find the transforms and hide them
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            string[] transformGroupNames = transformNames.Split(',');

            //----------------------------------------------------------
            //hide all transforms that are found
            //----------------------------------------------------------
            for (int k = 0; k < transformGroupNames.Length; k++)
            {
                Transform[] transforms = part.FindModelTransforms(transformGroupNames[k].Trim());
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
