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

            //Debug.Log("[LNYX] " + (targetTransform != null) + " " + (sourceTransform != null));
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
