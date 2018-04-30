using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterInternalSwitch : PartModule, ModuleKerbetrotterMeshToggle.MeshToggleListener
    {
        [KSPField]//the names of the transforms visible when disabled
        public string disabledTransformName = string.Empty;

        [KSPField]//the names of the transforms visible when enabled
        public string enabledTransformName = string.Empty;

        [KSPField]//the names of the transforms
        public int MeshToggleIndex = 0;

        //the list of disabled models
        private List<Transform> disabledTransforms = new List<Transform>();

        //the list of enabled models
        private List<Transform> enalbedTransforms = new List<Transform>();

        private bool stateEnabled = true;

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
                Debug.Log("[Kerbetrotter] Found switcher");
                switcher[MeshToggleIndex].addListener(this);
                stateEnabled = switcher[MeshToggleIndex].transformsVisible;
            }
            else
            {
                Debug.LogError("[Kerbetrotter] Did not find switcher");
            }
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

        private void onVesselChange(Vessel v)
        {
            if (v == vessel)
            {
                refresh();
                meshToggled(stateEnabled);
            }
        }

        public void meshToggled(bool enabled)
        {
            Debug.Log("[Kerbetrotter] Toggle of IVA mesh: " + enabled);
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
    }
}
