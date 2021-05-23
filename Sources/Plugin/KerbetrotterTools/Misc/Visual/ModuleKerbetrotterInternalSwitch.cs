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
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Module switching between to internal models (required for JSIATP)
    /// </summary>
    class ModuleKerbetrotterInternalSwitch : PartModule, ModuleKerbetrotterMeshToggle.MeshToggleListener
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The names of the transforms visible when disabled
        /// </summary>
        [KSPField]
        public string disabledTransformName = string.Empty;

        /// <summary>
        /// The names of the transforms visible when enabled
        /// </summary>
        [KSPField]
        public string enabledTransformName = string.Empty;

        /// <summary>
        /// The index of the toggled mesh
        /// </summary>
        [KSPField]
        public int MeshToggleIndex = 0;

        #endregion

        #region-------------------------Private Members----------------------

        //the list of disabled models
        private List<Transform> disabledTransforms = new List<Transform>();

        //the list of enabled models
        private List<Transform> enalbedTransforms = new List<Transform>();

        //whether the part is enabled or not
        private bool stateEnabled = true;

        #endregion

        #region----------------------------Life Cycle------------------------

        /// <summary>
        /// Find the transforms that can be toggled
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            string[] disabledGroupNames = disabledTransformName.Split(',');
            string[] enabledGroupNames = enabledTransformName.Split(',');
            disabledTransforms.Clear();
            enalbedTransforms.Clear();

            refresh();
            meshToggled(stateEnabled);
        }

        /// <summary>
        /// Register for events when the main body changed
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();
            GameEvents.onVesselChange.Add(onVesselChange);
        }

        /// <summary>
        /// Free all resources when the part is destroyed
        /// </summary>
        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(onVesselChange);
        }

        #endregion

        #region-------------------------Public Methods-----------------------

        public void meshToggled(bool enabled)
        {
            stateEnabled = enabled;
            for (int i = 0; i < disabledTransforms.Count; i++)
            {
                disabledTransforms[i].gameObject.SetActive(!stateEnabled);
            }

            for (int i = 0; i < enalbedTransforms.Count; i++)
            {
                enalbedTransforms[i].gameObject.SetActive(stateEnabled);
            }
        }

        #endregion

        #region-------------------------Private Methods----------------------

        /// <summary>
        /// Refresh the visibility
        /// </summary>
        private void refresh()
        {
            string[] disabledGroupNames = disabledTransformName.Split(',');
            string[] enabledGroupNames = enabledTransformName.Split(',');
            disabledTransforms.Clear();
            enalbedTransforms.Clear();

            if (part.internalModel != null)
            {
                for (int k = 0; k < disabledGroupNames.Length; k++)
                {
                    if (disabledGroupNames[k].Trim().Length > 0)
                    {
                        Transform disTransform = part.internalModel.FindModelTransform(disabledGroupNames[k].Trim());

                        if (disTransform != null)
                        {
                            disabledTransforms.Add(disTransform);
                        }
                        else
                        {
                            Debug.LogError("[Kerbetrotter] Transform not found: " + disabledGroupNames[k].Trim());
                        }
                    }

                }
                for (int k = 0; k < enabledGroupNames.Length; k++)
                {
                    if (enabledGroupNames[k].Trim().Length > 0)
                    {
                        Transform enTransform = part.internalModel.FindModelTransform(enabledGroupNames[k].Trim());

                        if (enTransform != null)
                        {
                            enalbedTransforms.Add(enTransform);
                        }
                        else
                        {
                            Debug.LogError("[Kerbetrotter] Transform not found: " + enabledGroupNames[k].Trim());
                        }
                    }
                }
            }

            List<ModuleKerbetrotterMeshToggle> switcher = part.FindModulesImplementing<ModuleKerbetrotterMeshToggle>();
            if ((switcher.Count > 0) && (MeshToggleIndex < switcher.Count))
            {
                switcher[MeshToggleIndex].addListener(this);
                stateEnabled = switcher[MeshToggleIndex].transformsVisible;
            }
            else
            {
                Debug.LogError("[Kerbetrotter] Did not find switcher");
            }
        }

        /// <summary>
        /// Called when the active vessel has changed
        /// </summary>
        /// <param name="v">The now active vessel</param>
        private void onVesselChange(Vessel v)
        {
            if (v == vessel)
            {
                refresh();
                meshToggled(stateEnabled);
            }
        }

        #endregion
    }
}
