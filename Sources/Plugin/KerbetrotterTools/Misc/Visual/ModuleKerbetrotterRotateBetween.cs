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
    /// Class to rotate one transform between two other transforms
    /// </summary>
    class ModuleKerbetrotterRotateBetween : PartModule
    {
        #region-------------------------Module Settings----------------------

        /// <summary>
        /// The name of the first reference transform
        /// </summary>
        [KSPField(isPersistant = false)]
        public string fromRotation = string.Empty;

        /// <summary>
        /// The name of the second reference transform
        /// </summary>
        [KSPField(isPersistant = false)]
        public string toRotation = string.Empty;

        /// <summary>
        /// Value for the interpolation between the targets
        /// </summary>
        [KSPField(isPersistant = false)]
        public string targetValues = string.Empty;

        /// <summary>
        /// The transform that should be rotated
        /// </summary>
        [KSPField(isPersistant = false)]
        public string targets = string.Empty;

        /// <summary>
        /// The transform that should be rotated
        /// </summary>
        [KSPField(isPersistant = false)]
        public bool useSlerp = false;

        #endregion

        #region-------------------------Private Members----------------------

        //saves wheter all transforms are found for the interpolation
        private bool isValid = false;

        //the first source for the rotation
        private Transform fromTransform = null;

        //the second source for the rotation
        private Transform toTransform = null;

        //the list of targets for the rotation
        private List<RotationTarget> rotationTargets = new List<RotationTarget>();

        //the number of targets
        private int numTargets = 0;

        #endregion

        #region---------------------------Life Cycle-------------------------

        //Called when the part is instantiated
        //Initializes the transforms
        public override void OnStart(StartState state)
        {
            fromTransform = KSPUtil.FindInPartModel(transform, fromRotation);
            toTransform = KSPUtil.FindInPartModel(transform, toRotation);

            string[] rotationValues = targetValues.Split(',');
            string[] rotationTargetNames = targets.Split(',');

            rotationTargets.Clear();

            //get all the rotation targets
            if (rotationValues.Length == rotationTargetNames.Length)
            {
                for (int i = 0; i < rotationValues.Length; i++)
                {
                    RotationTarget rotationTarget = new RotationTarget();
                    rotationTarget.target = KSPUtil.FindInPartModel(transform, rotationTargetNames[i].Replace(" ",string.Empty));

                    bool valid = true;
                    //try to get the value for the interpolation
                    try
                    {
                        rotationTarget.value = float.Parse(rotationValues[i].Replace(" ", string.Empty));
                    }
                    catch
                    {
                        valid = false;
                    }

                    //when we have 
                    if ((rotationTarget.target != null) && (valid))
                    {
                        rotationTargets.Add(rotationTarget);
                    }
                }
            }

            numTargets = rotationTargets.Count;

            //valid when all transforms have been found
            isValid = (fromTransform) & (toTransform);
        }

        /// <summary>
        /// Updates the rotation of the target
        /// </summary>
        public void Update()
        {
            //Do nothing when the transform is invalid
            if (!isValid)
            {
                return;
            }

            //Rotate the targets
            for (int i = 0; i < numTargets; i++)
            {
                if (useSlerp)
                {
                    rotationTargets[i].target.rotation = Quaternion.Slerp(fromTransform.rotation, toTransform.rotation, rotationTargets[i].value);
                }
                else
                {
                    rotationTargets[i].target.rotation = Quaternion.Lerp(fromTransform.rotation, toTransform.rotation, rotationTargets[i].value);
                }
            }
        }

        #endregion

        #region----------------------------Structs---------------------------

        /// <summary>
        /// The target for the rotation
        /// </summary>
        private struct RotationTarget
        {
            //The target transform
            public Transform target;

            //The target value
            public float value;
        }

        #endregion
    }
}
