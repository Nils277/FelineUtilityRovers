using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// This class allows to switch some of the models from parts
    /// </summary>
    public class ModuleKerbetrotterConstrainedLookAt : PartModule
    {
        //the orientation to copy
        [KSPField(isPersistant = false)]
        public string sourceTransformName = string.Empty;

        //the destination for the orientation
        [KSPField(isPersistant = false)]
        public string destinationTransformName = string.Empty;

        //the reference rotation
        [KSPField(isPersistant = false)]
        public string referenceTransformName = string.Empty;

        //The axis which should be constrained
        [KSPField(isPersistant = false)]
        public Axis constrainedAxis = Axis.X;

        //The source transform
        private Transform source;

        //The desination transform
        private Transform destination;

        //The reference transform
        private Transform referenceTransform;

        //whether the module is valid
        private bool valid = false;

        /// <summary>
        /// Initialize the module
        /// </summary>
        /// <param name="state">The start state of the part</param>
        public override void OnStart(StartState state)
        {
            source = part.FindModelTransform(sourceTransformName);
            destination = part.FindModelTransform(destinationTransformName);
            referenceTransform = part.FindModelTransform(referenceTransformName);

            valid = (source != null) & (destination != null) & (referenceTransform != null);
        }

        /// <summary>
        /// Update for every physicss tic
        /// </summary>
        public virtual void FixedUpdate()
        {
            if ((!HighLogic.LoadedSceneIsFlight) || (!valid))
            {
                return;
            }

            Vector3 vec = Quaternion.Inverse(referenceTransform.rotation) * source.forward;
            vec.x = (constrainedAxis == Axis.X)? 0 : vec.x;
            vec.y = (constrainedAxis == Axis.X)? vec.y : 0;

            destination.LookAt(destination.position + referenceTransform.rotation * vec);
            //limit rotation around z and (inluding y because of gimbal lock)
            if (constrainedAxis == Axis.X)
            {
                destination.Rotate(Vector3.forward, -destination.localRotation.eulerAngles.z - destination.localRotation.eulerAngles.y);
            }
            else
            {
                destination.Rotate(Vector3.forward, -destination.localRotation.eulerAngles.z);
            }
        }

        public enum Axis
        {
            X,
            Y
        }
    }
}
