using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// Class to update the orientation and position of the internal model when a hitch or grappling hook is used in the vessel
    /// </summary>
    class ModuleKerbetrotterInternalUpdater : PartModule
    {
        //==========================================
        // Members
        //==========================================

        /// <summary>
        /// The list of internal models that should be updated by this module
        /// </summary>
        private List<InternalData> childrenWithInternal = new List<InternalData>();

        //==========================================
        // Public Interface
        //==========================================

        /// <summary>
        /// Refreshes the list of children and reference positions/rotations
        /// </summary>
        public void refresh()
        {
            initReferences();
        }

        //==========================================
        // Events
        //==========================================

        /// <summary>
        /// Instantiate the Module. Register for changes in the vessel
        /// </summary>
        public override void OnAwake()
        {
            GameEvents.onVesselWasModified.Add(OnReferenceUpdate);
            GameEvents.onVesselChange.Add(OnReferenceUpdate);
            GameEvents.onVesselGoOffRails.Add(OnReferenceUpdate);
        }

        /// <summary>
        /// Unregister from the events when the module is destroyed
        /// </summary>
        public void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(OnReferenceUpdate);
            GameEvents.onVesselChange.Remove(OnReferenceUpdate);
            GameEvents.onVesselGoOffRails.Remove(OnReferenceUpdate);
            childrenWithInternal.Clear();
        }

        /// <summary>
        /// Update the references when the vessel has changed significantly
        /// </summary>
        /// <param name="vessel">The vessel that caused the event</param>
        private void OnReferenceUpdate(Vessel vessel)
        {
            if (vessel == this.vessel)
            {
                initReferences();
            }
        }

        //==========================================
        // Methods
        //==========================================

        /// <summary>
        /// Initialize the module
        /// </summary>
        /// <param name="state">The start state of the part</param>
        public override void OnStart(StartState state)
        {
            initReferences();
        }



        /// <summary>
        /// Updates the position and rotation of the internal models
        /// </summary>
        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                int numChilds = childrenWithInternal.Count;

                //The conjugate is the same as the inverse for unit quaternions but is less computationally expensive
                Quaternion inverseRotation = conjugate(vessel.transform.rotation);

                for (int i = numChilds-1; i >= 0; i--)
                {
                    //=============================================
                    // Update the orientation of the internal model
                    //=============================================
                    Quaternion currentRotation = inverseRotation * childrenWithInternal[i].part.transform.rotation;
                    Quaternion rotationDelta = conjugate(currentRotation) * childrenWithInternal[i].referenceRotation;

                    //swap y and z (bad IVA voodoo) this also adjusts the chirality 
                    float tmpY = rotationDelta.y;
                    rotationDelta.y = rotationDelta.z;
                    rotationDelta.z = tmpY;

                    childrenWithInternal[i].part.internalModel.transform.rotation = childrenWithInternal[i].internalRotation * rotationDelta;

                    //==========================================
                    // Update the position of the internal model
                    //==========================================
                    Vector3 currentPosition = inverseRotation * (vessel.transform.position - childrenWithInternal[i].part.transform.position);
                    Vector3 positionDelta = childrenWithInternal[i].referencePosition - currentPosition;
                    childrenWithInternal[i].part.internalModel.transform.position = childrenWithInternal[i].internalPosition + positionDelta;
                }
            }
        }

        /// <summary>
        /// Recursively finds all children which have an internal model
        /// This function stops at childs that have have this PartModule themselves (All childs of them are updated by this module)
        /// </summary>
        /// <param name="part">The part of which the children should be added</param>
        /// <param name="list">The list of the parts that have an internal model</param>
        /// <param name="inverseRotation">The inverse rotation of the vessel for the references</param>
        private void findPartsWithInternal(Part part, List<InternalData> list, Quaternion inverseRotation)
        {
            Part[] children = part.FindChildParts<Part>(false);

            for (int i = children.Length - 1; i >= 0; i--)
            {
                //when the child has an internal model, save it to the update list
                if (children[i].internalModel != null)
                {
                    InternalData referenceData;

                    referenceData.part = children[i];
                    referenceData.referenceRotation = inverseRotation * referenceData.part.transform.rotation;
                    referenceData.referencePosition = inverseRotation * (vessel.transform.position - referenceData.part.transform.position);
                    referenceData.internalRotation = referenceData.part.internalModel.transform.rotation;
                    referenceData.internalPosition = referenceData.part.internalModel.transform.position;

                    list.Add(referenceData);
                }

                //search in all children if they are do not themselfes have this module
                if (children[i].GetComponent(ClassName) == null)
                {
                    findPartsWithInternal(children[i], list, inverseRotation);
                }
            }
        }

        /// <summary>
        /// Initialize the references and original position/orientation of the internal model
        /// </summary>
        public void initReferences()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                //empty the list if children with an internal model
                childrenWithInternal.Clear();

                //The conjugate is the same as the inverse for unit quaternions but is less computationally expensive
                Quaternion inverseRotation = conjugate(vessel.transform.rotation);

                //initialize all the children with an internal model
                findPartsWithInternal(part, childrenWithInternal, inverseRotation);
            }
        }

        /// <summary>
        /// Create the conjugate of the quaternion
        /// This is the same as the inverse for unit qaternions but is less expensive
        /// </summary>
        /// <param name="quat">The quaternion to conjugate</param>
        /// <returns>The conjugated quaternion</returns>
        private Quaternion conjugate(Quaternion quat)
        {
            Quaternion result = quat;
            result.x = -result.x;
            result.y = -result.y;
            result.z = -result.z;
            return result;
        }

        //==========================================
        // Methods
        //==========================================

        /// <summary>
        /// Struct holding all the data needed to update the internal models
        /// </summary>
        private struct InternalData
        {
            /// <summary>
            /// The reference used to get the changed relative rotation of the part
            /// </summary>
            public Quaternion referenceRotation;

            /// <summary>
            /// The initial rotation of the internal model
            /// </summary>
            public Quaternion internalRotation;

            /// <summary>
            /// The reference used to calculate the changed relative position of the part
            /// </summary>
            public Vector3 referencePosition;

            /// <summary>
            /// The initial position of the internal model
            /// </summary>
            public Vector3 internalPosition;

            /// <summary>
            /// The part that contains the internal model
            /// </summary>
            public Part part;
        }
    }
}
