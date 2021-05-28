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

namespace KerbetrotterTools
{
    /// <summary>
    /// This module rotates a transform towards another. On of the axes will not rotate
    /// </summary>
    public class ModuleKerbetrotterConstrainedLookAt : PartModule
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The name of the transform that should be used to copy the rotation from
        /// </summary>
        [KSPField(isPersistant = false)]
        public string sourceTransformName = string.Empty;

        /// <summary>
        /// The name of the destination transform
        /// </summary>
        [KSPField(isPersistant = false)]
        public string destinationTransformName = string.Empty;

        /// <summary>
        /// The reference rotation transform
        /// </summary>
        [KSPField(isPersistant = false)]
        public string referenceTransformName = string.Empty;

        /// <summary>
        /// The axis which should be constrained
        /// </summary>
        [KSPField(isPersistant = false)]
        public Axis constrainedAxis = Axis.X;

        #endregion

        #region-------------------------Private Members----------------------

        //The source transform
        private Transform source;

        //The desination transform
        private Transform destination;

        //The reference transform
        private Transform referenceTransform;

        //whether the module is valid
        private bool valid = false;

        #endregion

        #region---------------------------Life Cycle-------------------------

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

        #endregion

        #region-----------------------------Enums----------------------------

        //The axis that should be contrained
        public enum Axis
        {
            X,
            Y,
            Z
        }

        #endregion
    }
}
