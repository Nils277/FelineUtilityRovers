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
using System;
using UnityEngine;
using KSP.Localization;
using System.Collections;

namespace KerbetrotterTools
{
    /// <summary>
    /// The base class for the hitches of the kerbatrottertools. It implements all functionality for a generic hitch. Including spring, damping and rotational limits.
    /// <c>ModuleKerbetrotterHitchBase</c> also procvides functions to lock and unlock the hitch.
    /// A class extending <c>ModuleKerbetrotterHitchBase</c> is needed for additional functionality, like rotating parts or restrictions to when the hitch is usable
    /// This Hitch used the <c>ModuleIRServo.cs</c> code from the InfernalRobotics from sirkut and Ziw as a reference implementation
    /// </summary>
    public class ModuleKerbetrotterHitch : PartModule, IJointLockState, IModuleInfo
    {
        //==================================================
        //Public fields for the configs
        //==================================================

        /// <summary>
        /// The names of the transforms that are used as reference
        /// </summary>
        [KSPField(isPersistant = false)]
        public string referenceTransformNames = string.Empty;

        /// <summary>
        /// The names of the corresponding nodes for the references
        /// </summary>
        [KSPField(isPersistant = false)]
        public string referenceNodeNames = string.Empty;

        [KSPField(isPersistant = false)]
        public string visibleTransformNames = string.Empty;

        /// <summary>
        /// The spring strength of the joint
        /// </summary>
        [KSPField(isPersistant = false)]
        public float jointSpring = 25;

        /// <summary>
        /// The damping of the joint
        /// </summary>
        [KSPField(isPersistant = false)]
        public float jointDamping = 1;

        /// <summary>
        /// The limits for the rotation of the joint
        /// An axis is limited when the value is positive, free when the value is negative and locked when the value is 0
        /// </summary>
        [KSPField(isPersistant = false)]
        public Vector3 jointLimits = new Vector3(10, 10, 10);

        /// <summary>
        /// The joints can be locked when it is less than <c>maxSnapDistance</c> from the original rotation
        /// </summary>
        [KSPField(isPersistant = false)]
        public float maxSnapDistance = 5;

        //==================================================
        //User Interaction 
        //==================================================
        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.hitch.status")]
        public string status = string.Empty;

        //The slider for the spring of the hitch
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.hitch.spring"), UI_FloatRange(minValue = 1f, maxValue = 200f, stepIncrement = 1f)]
        public float jointSpringValue = 1.0f;

        //The slider for the damping of the hitch
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.hitch.damping"), UI_FloatRange(minValue = 0.1f, maxValue = 3.0f, stepIncrement = 0.1f)]
        public float jointDampingValue = 1.0f;

        //==================================================
        //Internal Members
        //==================================================

        //The joints used for the hitch
        protected ConfigurableJoint joint;

        //The original joint saved for the timewarp
        protected ConfigurableJoint originalJoint;

        //The transform of the reference
        protected Transform[] ReferenceTransforms = new Transform[2];

        //The transform of the reference
        private Transform[] VisibleTransforms = new Transform[2];

        //The rotation of the first reference
        [KSPField(isPersistant = true)]
        public Quaternion rotation1 = Quaternion.identity;

        //The rotation of the second reference
        [KSPField(isPersistant = true)]
        public Quaternion rotation2 = Quaternion.identity;

        //The original rotation of the part
        private Quaternion initialPartRotation;

        /// <summary>
        /// Saves whether the joints spring and damping have been initialized
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool initialized = false;

        //the first delta of the rotation from the last rotation
        private Quaternion[] rotationDeltas = new Quaternion[2];

        //The last value of the spring of the hitch
        private float lastSpring = -1.0f;

        //The last value of the damping of the hitch
        private float lastDamping = -1.0f;

        //==================================================
        //FAR related things
        //==================================================

        //The last rotations, needed to update the FAR voxels
        private Quaternion[] lastRotations = new Quaternion[2];

        //Intervall at which the shape information should be updated
        private const int FARUpdate = 60;

        //counter for the update of the shape
        private int FARUpdateCnt = 0;

        //==================================================
        //State of the hitch
        //==================================================

        //Saves whether the motion of the parts is locked
        [KSPField(isPersistant = true)]
        public bool isLockEngaged = true;

        //Saves wheter the parts has a valid parent
        protected bool hasParent = false;

        //Saves whether the setup of the joints is done
        public bool jointUnlocked = false;

        // Saves wheter the Vessel is on Rails currently
        protected bool isOnRails = false;

        // Saves whetere the hitch can be locked currently
        protected bool canLock = false;

        // Saves wheter the hitch can be unlocked currently
        protected bool canUnlock = true;

        // Saves wheter there is a valid attachments
        protected bool isValidAttachment = false;

        // Saves wheter there is a valid attachments
        protected float rotation = 0.0f;

        // The index of the currently active reference transform
        private int activeReference = -1;

        // The part that is currently the parent of this part
        private Part currentParent = null;

        //==================================================
        //User Interaction 
        //==================================================

        /// <summary>
        /// Event that can toggle the lock state of the hitch from the GUI
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Unlock", active = true)]
        public void MotionLockToggle()
        {
            if (isValidAttachment)
            {
                SetJointLock(!isLockEngaged);
            }
        }

        /// <summary>
        /// Action to toggle the lock of the hitch
        /// </summary>
        /// <param name="param">The parameter of the action (not used)</param>
        [KSPAction("Toggle Lock")]
        public void MotionLockToggleAction(KSPActionParam param)
        {
            if (isValidAttachment)
            {
                SetJointLock(!isLockEngaged);
            }
        }

        /// <summary>
        /// Action to unlock the hitch
        /// </summary>
        /// <param name="param">The parameter of the action (not used)</param>
        [KSPAction("Unlock Hitch")]
        public void UnlockHitch(KSPActionParam param)
        {
            if (isValidAttachment)
            {
                SetJointLock(false);
            }
        }

        /// <summary>
        /// Action to unlock the hitch
        /// </summary>
        /// <param name="param">The parameter of the action (not used)</param>
        [KSPAction("Lock Hitch")]
        public void LockHitch(KSPActionParam param)
        {
            if (isValidAttachment)
            {
                SetJointLock(true);
            }

            
        }

        //==================================================
        //Life Cycle
        //==================================================

        //When the hitch is intantiated
        public override void OnAwake()
        {
            GameEvents.onVesselGoOnRails.Add(OnVesselGoOnRails);
            GameEvents.onVesselGoOffRails.Add(OnVesselGoOffRails);
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
        }

        //When the hitch is destroyed
        public virtual void OnDestroy()
        {
            GameEvents.onVesselGoOnRails.Remove(OnVesselGoOnRails);
            GameEvents.onVesselGoOffRails.Remove(OnVesselGoOffRails);
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);

            if (ReferenceTransforms[0] != null)
            {
                ReferenceTransforms[0].parent = part.transform;
                ReferenceTransforms[0] = null;
            }

            if (ReferenceTransforms[1] != null)
            {
                ReferenceTransforms[1].parent = part.transform;
                ReferenceTransforms[1] = null;
            }
        }

        //Load the hitch
        public override void OnLoad(ConfigNode config)
        {
            base.OnLoad(config);

            //save the rotation delta because rotation will change dynamically
            rotationDeltas[0] = rotation1;
            rotationDeltas[1] = rotation2;
        }

        //Called when the part is instantiated
        //Initializes the joints and attachments when in flight mode
        public override void OnStart(StartState state)
        {
            if (!initialized)
            {
                jointSpringValue = jointSpring;
                jointDampingValue = jointDamping;
                initialized = true;
            }

            string[] names = visibleTransformNames.Split(',');

            if (names.Length == 2)
            {
                if (names[0].Replace(" ", string.Empty) != "NULL")
                {
                    VisibleTransforms[0] = KSPUtil.FindInPartModel(transform, names[0].Replace(" ", string.Empty));
                }
                if (names[1].Replace(" ", string.Empty) != "NULL")
                {
                    VisibleTransforms[1] = KSPUtil.FindInPartModel(transform, names[1].Replace(" ", string.Empty));
                }
            }

            //Initialize the joints when in flight mode
            if (HighLogic.LoadedSceneIsFlight)
            {
                
                StartCoroutine(WaitAndInitialize());
                
                //save the initial rotation of the part
                initialPartRotation = part.orgRot;
            }

            //update the interface
            UpdateUI();
        }

        /// <summary>
        /// Waits to initialize the references and the joint
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator WaitAndInitialize()
        {
            if (part.parent)
            {
                while (!part.attachJoint || !part.attachJoint.Joint)
                    yield return null;

                InitReferences(false);
                InitJoint();
                UpdateUI();
            }
            else
            {
                hasParent = false;
            }
        }

        //==================================================
        //Game Events
        //==================================================

        //When the physics simulation ends
        public void OnVesselGoOffRails(Vessel vessel)
        {
            //only when this vessel is going off rails
            if (vessel == this.vessel)
            {
                jointUnlocked = false;

                //When there is no joint, initialize it. --TODO can this really happen?
                if (joint == null)
                {
                    InitJoint();
                }

                //unlock the state regardless if the lock is engaged. --TODO needs to be done to get into resume where we left...why?
                UnlockJoint();

                isOnRails = false;
            }
        }

        //When the physicsless simulation starts
        public void OnVesselGoOnRails(Vessel vessel)
        {
            if (vessel == this.vessel)
            {
                //Lock the joint so we do not move around too much
                LockJoint(false);
                isOnRails = true;
            }
        }

        //When the vessel was modified (mostly docking and undocking)
        public void OnVesselWasModified(Vessel vessel)
        {
            //Only do something when this vessel is affected
            if (vessel == this.vessel)
            {
                //Save the deltas
                rotationDeltas[0] = rotation1;
                rotationDeltas[1] = rotation2;

                //Refresh the information about the references
                RefreshReferences();

                jointUnlocked = false;

                //When the part has no parent, invalidate
                if (!hasParent || !isValidAttachment)
                {
                    Debug.Log("[KerbetrotterTools:Hitch] OnVesselWasModified: Invalid State, locking joint");
                    rotationDeltas[0] = Quaternion.identity;
                    rotationDeltas[1] = Quaternion.identity;
                    rotation1 = Quaternion.identity;
                    rotation2 = Quaternion.identity;

                    //Lock and destroy the joint
                    if (joint != null)
                    {
                        DestroyImmediate(joint);
                        LockJoint(true);
                    }
                }
                UpdateUI();
            }
        }

        //==================================================
        //Core Funcionality
        //==================================================

        /// <summary>
        /// Updates the rotation of the visible transforms
        /// </summary>
        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                //Update the orientation of the first visible transform
                if ((ReferenceTransforms[0] != null) && (VisibleTransforms[0] != null)) {
                    VisibleTransforms[0].rotation = ReferenceTransforms[0].rotation;
                }
                //Update the orientation of the second visible transform
                if ((ReferenceTransforms[1] != null) && (VisibleTransforms[1] != null))
                {
                    VisibleTransforms[1].rotation = ReferenceTransforms[1].rotation;
                }
            }
        }

        //Called every physics tick and updates state of the hitch
        public void FixedUpdate()
        {
            //only run when in valid state
            if ((HighLogic.LoadedSceneIsFlight) && (!isOnRails) && (part.State != PartStates.DEAD))
            {
                //when the attachment is not valid (yet), do nothing
                if ((!isValidAttachment) || (joint == null))
                {
                    return;
                }

                /*if ((!isValidAttachment) && (numTries < 10))
                {
                    Debug.LogError("[KERBETROTTER] FixedUpdate invalid Attachment " + numTries);

                    //InitReferences(false);
                    numTries++;
                }*/

                //update the joint (
                UpdateJointLockState();

                //update the spring and damping
                UpdateSpringAndDamping();

                //Update the information about the rotation of the hitch
                if (ReferenceTransforms[0] !=  null)
                {
                    rotation1 = Conjugate(ReferenceTransforms[0].rotation) * transform.rotation;
                }

                if (ReferenceTransforms[1] != null)
                {
                    rotation2 = Conjugate(ReferenceTransforms[1].rotation) * transform.rotation;
                }


                
                //update the original position and orientation of this part and all childs
                part.UpdateOrgPosAndRot(vessel.rootPart);
                foreach (Part child in part.FindChildParts<Part>(true))
                {
                    child.UpdateOrgPosAndRot(vessel.rootPart);
                }
                //UpdateOriginalPositionAndRotation(); //--Use instead of the above when not working properly

                //Updates needed for FAR
                UpdateFAR();

                //Update whether the hitch can be locked
                if (activeReference != -1)
                {
                    rotation = Math.Abs(Quaternion.Angle(ReferenceTransforms[activeReference].rotation, transform.rotation));
                    canLock = (Math.Abs(Quaternion.Angle(ReferenceTransforms[activeReference].rotation, transform.rotation)) <= maxSnapDistance);
                }
                else
                {
                    canLock = true;
                }
            }

            //Update the visibility of the actions to lock/unlock the hitch
            UpdateLockAction();
        }

        /// <summary>
        /// Update the status of the joint
        /// </summary>
        public void UpdateJointLockState()
        {
            /*if ((joint == null) && (hasParent) && (isValidAttachment))
            {
                Debug.Log("[KERBETROTTER] UpdateJoint: Creating");
                InitJoint();
                UpdateUI();
            }*/

            if (isLockEngaged && jointUnlocked)
            {
                Debug.Log("[KerbetrotterTools:Hitch] UpdateJoint: Locking!");
                LockJoint(true);
            }
            else if (!isLockEngaged && !jointUnlocked)
            {
                Debug.Log("[KerbetrotterTools:Hitch] UpdateJoint: Unlocking!");
                UnlockJoint();
            }
        }

        /// <summary>
        /// Initialize the joint
        /// </summary>
        private void InitJoint()
        {
            if ((part.attachJoint == null) || (activeReference == -1))
            {
                Debug.LogError("[KerbetrotterTools:Hitch] InitJoint: Cannot create joint: " + (part.attachJoint == null) + ", " + (activeReference == -1));
                return;
            }

            //Destroy the joint if it still exists
            if (joint != null)
            {
                Debug.LogError("[KerbetrotterTools:Hitch] InitJoint: Destroying Joint");
                DestroyImmediate(joint);
            }

            //Save the original joint for later
            if (part.attachJoint != null)
            {
                originalJoint = part.attachJoint.Joint;
            }

            //Catch reversed joint --TODO why does this have to be separated?
            if (transform.position != part.attachJoint.Joint.connectedBody.transform.position)
            {
                joint = part.attachJoint.Joint.connectedBody.gameObject.AddComponent<ConfigurableJoint>();
                joint.connectedBody = part.attachJoint.Joint.GetComponent<Rigidbody>();
            }
            else
            {
                joint = part.attachJoint.Joint.GetComponent<Rigidbody>().gameObject.AddComponent<ConfigurableJoint>();
                joint.connectedBody = part.attachJoint.Joint.connectedBody;
            }

            if (joint == null)
            {
                Debug.LogError("[KerbetrotterTools:Hitch] InitJoint: Cannot create Joint!");
                return;
            }

            //Make the new joint unbreakable
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;

            //Make the original joint unbreakable
            part.attachJoint.Joint.breakForce = float.PositiveInfinity;
            part.attachJoint.Joint.breakTorque = float.PositiveInfinity;
            part.attachJoint.SetBreakingForces(float.PositiveInfinity, float.PositiveInfinity);

            //Lock the movement of the joint
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            //Set projection distance and angle
            joint.projectionDistance = 0f;
            joint.projectionAngle = 0f;
            joint.projectionMode = JointProjectionMode.PositionAndRotation;

            //Copy drives
            joint.linearLimit = part.attachJoint.Joint.linearLimit;
            joint.angularXDrive = part.attachJoint.Joint.angularXDrive;
            joint.angularYZDrive = part.attachJoint.Joint.angularYZDrive;
            joint.xDrive = part.attachJoint.Joint.xDrive;
            joint.yDrive = part.attachJoint.Joint.yDrive;
            joint.zDrive = part.attachJoint.Joint.zDrive;

            //Get the rigid body of the joint
            Rigidbody jointRigidBody = joint.GetComponent<Rigidbody>();

            //Set anchor position
            joint.anchor = jointRigidBody.transform.InverseTransformPoint(joint.connectedBody.transform.position);
            joint.connectedAnchor = Vector3.zero;


            //Set the right rotation of the joint (we have to invert x and y...hell knows why)
            if (activeReference != -1)
            {
                joint.transform.rotation = (joint.transform.rotation * Quaternion.Euler(-rotationDeltas[activeReference].eulerAngles.x, -rotationDeltas[activeReference].eulerAngles.y, rotationDeltas[activeReference].eulerAngles.z));
                //joint.transform.rotation = joint.transform.rotation * rotationDeltas[activeReference];
            }

            //Set correct axis
            joint.axis = jointRigidBody.transform.InverseTransformDirection(joint.connectedBody.transform.right);  //x axis
            joint.secondaryAxis = jointRigidBody.transform.InverseTransformDirection(joint.connectedBody.transform.up); //y axis

            //Disable the collision
            joint.enableCollision = false;

            //Set the drive mode
            joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        }

        /// <summary>
        /// Unlocks the joint
        /// </summary>
        /// <returns><c>true</c>, if joint was successfully unlocked, <c>false</c> otherwise.</returns>
        public virtual bool UnlockJoint()
        {
            //if the joint does not exist, do nothing
            if ((part.attachJoint == null) || (joint == null))
            {
                jointUnlocked = false;
                return false;
            }

            //If the joint is already unlocked, do nothing
            if (jointUnlocked)
            {
                return false;
            }

            joint.targetRotation = Quaternion.identity;
            
            //Lock the movement of the joint
            //joint.xMotion = ConfigurableJointMotion.Locked;
            //joint.yMotion = ConfigurableJointMotion.Locked;
            //joint.zMotion = ConfigurableJointMotion.Locked;

            //Limits for the x axis
            if (jointLimits.x != 0.0f)
            {
                SoftJointLimit lowLimit = part.attachJoint.Joint.lowAngularXLimit;
                SoftJointLimit upLimit = part.attachJoint.Joint.highAngularXLimit;

                if (jointLimits.x > 0.0f)
                {
                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                    lowLimit.limit = -jointLimits.x;
                    upLimit.limit = jointLimits.x;
                }
                else
                {
                    joint.angularXMotion = ConfigurableJointMotion.Free;
                }
                joint.lowAngularXLimit = lowLimit;
                joint.highAngularXLimit = upLimit;
            }
            else
            {
                joint.angularXMotion = ConfigurableJointMotion.Locked;
                joint.lowAngularXLimit = part.attachJoint.Joint.lowAngularXLimit;
                joint.highAngularXLimit = part.attachJoint.Joint.highAngularXLimit;
            }

            //Limits for the y axis
            if (jointLimits.y != 0.0f)
            {
                SoftJointLimit limit = part.attachJoint.Joint.angularYLimit;

                if (jointLimits.y > 0.0f)
                {
                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                    limit.limit = jointLimits.y;
                }
                else
                {
                    joint.angularYMotion = ConfigurableJointMotion.Free;
                }

                joint.angularYLimit = limit;
            }
            else
            {
                joint.angularYMotion = ConfigurableJointMotion.Locked;
                joint.angularYLimit = part.attachJoint.Joint.angularYLimit;
            }

            //Limits for the z axis
            if (jointLimits.z != 0.0f)
            {
                SoftJointLimit limit = part.attachJoint.Joint.angularZLimit;
                if (jointLimits.z > 0.0f)
                {
                    joint.angularZMotion = ConfigurableJointMotion.Limited;
                    limit.limit = jointLimits.z;
                }
                else
                {
                    joint.angularZMotion = ConfigurableJointMotion.Free;
                }

                joint.angularZLimit = limit;
            }
            else
            {
                joint.angularZMotion = ConfigurableJointMotion.Locked;
                joint.angularZLimit = part.attachJoint.Joint.angularZLimit;
            }

            //Set the X Drive of the joint
            JointDrive XDrive = joint.angularXDrive;
            XDrive.positionSpring = jointSpringValue;
            XDrive.positionDamper = jointDampingValue;
            joint.angularXDrive = XDrive;

            //Set the Y and Z drive of the joint
            JointDrive YZDrive = joint.angularYZDrive;
            YZDrive.positionSpring = jointSpringValue;
            YZDrive.positionDamper = jointDampingValue;
            joint.angularYZDrive = YZDrive;

            //Reset default joint drives
            JointDrive ZeroDrive = new JointDrive();
            ZeroDrive.positionSpring = 0.0f;
            ZeroDrive.positionDamper = 0.0f;
            ZeroDrive.maximumForce = 0.0f;

            //Set the properties of the joint
            part.attachJoint.Joint.angularXDrive = ZeroDrive;
            part.attachJoint.Joint.angularYZDrive = ZeroDrive;
            part.attachJoint.Joint.xDrive = ZeroDrive;
            part.attachJoint.Joint.yDrive = ZeroDrive;
            part.attachJoint.Joint.zDrive = ZeroDrive;
            //part.attachJoint.Joint.xMotion = ConfigurableJointMotion.Locked;
            //part.attachJoint.Joint.yMotion = ConfigurableJointMotion.Locked;
            //part.attachJoint.Joint.zMotion = ConfigurableJointMotion.Locked;
            part.attachJoint.Joint.enableCollision = false;
            part.attachJoint.Joint.angularXMotion = ConfigurableJointMotion.Free;
            part.attachJoint.Joint.angularYMotion = ConfigurableJointMotion.Free;
            part.attachJoint.Joint.angularZMotion = ConfigurableJointMotion.Free;

            jointUnlocked = true;
            Debug.Log("[KerbetrotterTools:Hitch] Unlocked Hitch");
            return true;
        }

        /// <summary>
        /// Lock the joint
        /// </summary>
        public void LockJoint(bool snap)
        {
            if ((part.attachJoint != null) && (part.attachJoint.Joint != null) && (originalJoint != null))
            {
                part.attachJoint.Joint.angularXDrive = originalJoint.angularXDrive;
                part.attachJoint.Joint.angularYZDrive = originalJoint.angularYZDrive;
                part.attachJoint.Joint.xDrive = originalJoint.xDrive;
                part.attachJoint.Joint.yDrive = originalJoint.yDrive;
                part.attachJoint.Joint.zDrive = originalJoint.zDrive;
                //part.attachJoint.Joint.xMotion = ConfigurableJointMotion.Locked;
                //part.attachJoint.Joint.yMotion = ConfigurableJointMotion.Locked;
                //part.attachJoint.Joint.zMotion = ConfigurableJointMotion.Locked;
                part.attachJoint.Joint.enableCollision = false;
            }

            //lock all the movement of the joint
            if (joint != null)
            {
                joint.angularXMotion = ConfigurableJointMotion.Locked;
                joint.angularYMotion = ConfigurableJointMotion.Locked;
                joint.angularZMotion = ConfigurableJointMotion.Locked;
            }
            Debug.Log("[KerbetrotterTools] Locked Hitch");
            jointUnlocked = false;
        }

        //==================================================
        //Helper Methods
        //==================================================

        // set original rotation to new rotation --Not neded at the moment, but kept, just to be sure
        public void UpdateOriginalPositionAndRotation()
        {
            if (initialPartRotation != null)
            {
                if (ReferenceTransforms[activeReference] != null)
                {
                    Quaternion jointRotation = Conjugate(ReferenceTransforms[activeReference].rotation) * (transform.rotation);
                    Quaternion targetRotation = initialPartRotation * Conjugate(rotationDeltas[activeReference]) * jointRotation;
                    Quaternion relativeRotation = targetRotation * Quaternion.Inverse(part.orgRot);

                    part.orgRot = targetRotation;
                    foreach (Part child in part.FindChildParts<Part>(true))
                    {
                        child.orgPos = part.orgPos + relativeRotation * (child.orgPos - part.orgPos);
                        child.orgRot = relativeRotation * child.orgRot;
                    }
                }
            }
        }

        Quaternion NormalizeQuaternion(Quaternion q)
        {
            float f = 1f / Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);

            q.w *= f;
            q.x *= f;
            q.y *= f;
            q.z *= f;

            return q;
        }

        /// <summary>
        /// Update the interface of the hitch
        /// </summary>
        private void UpdateUI()
        {
            Events["MotionLockToggle"].guiName = isLockEngaged ? Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.status.unlock") : Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.status.lock");

            //Update the visible status of the hitch
            if (part.parent == null)
            {
                status = Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.status.noparent");
            }
            else if (HighLogic.LoadedSceneIsFlight && ((joint == null) || (!isValidAttachment)))
            {
                status = Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.status.inoperable");
            }
            else if (isLockEngaged)
            {
                status = Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.status.locked");
            }
            else
            {
                status = status = Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.status.released");
            }
        }

        /// <summary>
        /// Update the GUI 
        /// </summary>
        private void UpdateLockAction()
        {
            Events["MotionLockToggle"].guiActive = hasParent && isValidAttachment && (joint != null) && ((canLock && !isLockEngaged) || (isLockEngaged && canUnlock));
        }

        /// <summary>
        /// Rebuild the attachments when docking or similar has changed
        /// </summary>
        private void RefreshReferences()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                return;
            }

            int oldReference = activeReference;

            InitReferences(true);

            //update the joint
            if (joint != null)
            {
                if (oldReference != -1)
                {
                    joint.transform.rotation = joint.transform.rotation * Conjugate(Quaternion.Euler(rotationDeltas[oldReference].eulerAngles.x, rotationDeltas[oldReference].eulerAngles.y, rotationDeltas[oldReference].eulerAngles.z));
                }

                if (activeReference != -1)
                {
                    joint.transform.rotation = joint.transform.rotation * Quaternion.Euler(rotationDeltas[activeReference].eulerAngles.x, rotationDeltas[activeReference].eulerAngles.y, rotationDeltas[activeReference].eulerAngles.z);
                }
            }
        }

        /// <summary>
        /// Attach the reference trasform the the parent if the situation is valid
        /// </summary>
        private void InitReferences(bool reInitialize)
        {
            //Do not init in the editor
            if (!HighLogic.LoadedSceneIsFlight)
            {
                return;
            }

            //Get the names of the reference transforms
            string[] names = referenceTransformNames.Split(',');

            if (names.Length == 2)
            {
                //get the first transform
                if (ReferenceTransforms[0] == null)
                {
                    //ReferenceTransforms[0] = Instantiate(part.transform);
                    ReferenceTransforms[0] = Instantiate(KSPUtil.FindInPartModel(transform, names[0].Replace(" ", string.Empty)));
                }

                //get the second transform
                if (ReferenceTransforms[1] == null)
                {
                    //ReferenceTransforms[1] = Instantiate(part.transform);
                    ReferenceTransforms[1] = Instantiate(KSPUtil.FindInPartModel(transform, names[1].Replace(" ", string.Empty)));
                }
            }

            //If part has no parent, set error
            if (part.parent == null)
            {
                if (!isLockEngaged)
                {
                    SetJointLock(true, true);
                }

                if (ReferenceTransforms[0] != null)
                {
                    ReferenceTransforms[0].parent = part.transform;
                    ReferenceTransforms[0] = null;
                }

                if (ReferenceTransforms[1] != null)
                {
                    ReferenceTransforms[1].parent = part.transform;
                    ReferenceTransforms[1] = null;
                }

                Debug.LogError("[KerbetrotterTools:Hitch] InitReferences: Part has no parent");
                hasParent = false;
                isValidAttachment = false;
                currentParent = null;
            }
            //Continue wih initialization
            else
            {
                if (currentParent != part.parent)
                {
                    if (joint != null)
                    {
                        DestroyImmediate(joint);
                        LockJoint(false);
                        jointUnlocked = false;
                    }
                    currentParent = part.parent;
                }

                //When the references are valid
                if ((ReferenceTransforms[0] != null) && (ReferenceTransforms[1] != null))
                {
                    string[] nodeNames = referenceNodeNames.Split(',');

                    //when we have a valid number of nodess
                    if (nodeNames.Length == 2)
                    {
                        //set the first transform
                        if (!reInitialize)
                        {
                            ReferenceTransforms[0].position = part.transform.position;
                            ReferenceTransforms[0].rotation = part.transform.rotation * Conjugate(rotation1);
                        }

                        AttachNode node1 = GetNode(nodeNames[0].Replace(" ", string.Empty));
                        if ((node1 != null) && (node1.attachedPart != null))
                        {
                            ReferenceTransforms[0].parent = node1.attachedPart.transform;
                        }
                        else
                        {
                            ReferenceTransforms[0].parent = part.transform;
                        }

                        //set the second transform
                        if (!reInitialize)
                        {
                            ReferenceTransforms[1].position = part.transform.position;
                            ReferenceTransforms[1].rotation = part.transform.rotation * Conjugate(rotation2);
                        }

                        AttachNode node2 = GetNode(nodeNames[1].Replace(" ", string.Empty));
                        if ((node2 != null) && (node2.attachedPart != null))
                        {
                            ReferenceTransforms[1].parent = node2.attachedPart.transform;
                        }
                        else
                        {
                            ReferenceTransforms[1].parent = part.transform;
                        }

                        //find out which is the active reference
                        if (node1.attachedPart == part.parent)
                        {
                            activeReference = 0;
                        }
                        else if (node2.attachedPart == part.parent)
                        {
                            activeReference = 1;
                        }
                        else
                        {
                            activeReference = -1;
                        }
                    }
                    isValidAttachment = activeReference != -1;
                    if (!isValidAttachment)
                    {
                        Debug.LogError("[KerbetrotterTools:Hitch] InitReferences: No active reference");
                    }
                }
                //Log that there are errors for the rotations
                else
                {
                    isValidAttachment = false;
                    Debug.LogError("[KerbetrotterTools:Hitch] InitReferences: One of the references is null: " + (ReferenceTransforms[0] == null) + ", " + (ReferenceTransforms[1] == null));
                }
                hasParent = true;
            }
        }

        /// <summary>
        /// Update the values for damping and spring when they have changed
        /// </summary>
        private void UpdateSpringAndDamping()
        {
            //Update the damping when the values changed
            if ((lastDamping != jointDampingValue) || (lastSpring != jointSpringValue))
            {
                JointDrive drv = joint.angularXDrive;
                drv.positionSpring = jointSpringValue;
                drv.positionDamper = jointDampingValue;
                joint.angularXDrive = drv;

                JointDrive drv2 = joint.angularYZDrive;
                drv2.positionSpring = jointSpringValue;
                drv2.positionDamper = jointDampingValue;
                joint.angularYZDrive = drv2;

                lastDamping = jointDampingValue;
                lastSpring = jointSpringValue;
            }
        }

        /// <summary>
        /// Create the conjugate of the quaternion
        /// This is the same as the inverse for unit qaternions but is less expensive
        /// </summary>
        /// <param name="quat">The quaternion to conjugate</param>
        /// <returns>The conjugated quaternion</returns>
        private Quaternion Conjugate(Quaternion quat)
        {
            Quaternion result = quat;
            result.x = -result.x;
            result.y = -result.y;
            result.z = -result.z;
            return result;
        }

        /// <summary>
        /// Return whether the joint is currently unlocked or not
        /// </summary>
        /// <returns>True, the joint is always unlocked</returns>
        bool IJointLockState.IsJointUnlocked()
        {
            return true;
        }

        /// <summary>
        /// Messages all children with "UpdateShapeWithAnims". This is needed for FAR to update to aerodynamic model
        /// </summary>
        protected void UpdateFAR()
        {
            if (FARUpdateCnt >= FARUpdate)
            {
                //when the angle changes significantly (more than 2°)
                if ((Math.Abs(Quaternion.Angle(lastRotations[0], rotation1)) >= 2.0f) || ((Math.Abs(Quaternion.Angle(lastRotations[1], rotation2)) >= 2.0f)))
                {
                    part.SendMessage("UpdateShapeWithAnims");
                    foreach (var p in part.children)
                    {
                        p.SendMessage("UpdateShapeWithAnims");
                    }

                    lastRotations[0] = rotation1;
                    lastRotations[1] = rotation2;
                    FARUpdateCnt = 0;
                }
            }
            else
            {
                FARUpdateCnt++;
            }
        }

        /// <summary>
        /// Lock or unlock the joint
        /// </summary>
        /// <param name="lockJoint">When true, the joint will be locked, it will be unlocked otherwise</param>

        public void SetJointLock(bool lockJoint)
        {
            SetJointLock(lockJoint, false);
        }

        /// <summary>
        /// Find the attachnode with the given name.
        /// </summary>
        /// <param name="nodeName">The name of the node</param>
        /// <returns></returns>
        protected AttachNode GetNode(string nodeName)
        {
            int numNodes = part.attachNodes.Count;
            for (int i = 0; i < numNodes; i++)
            {
                if (part.attachNodes[i].id == nodeName)
                {
                    return (part.attachNodes[i]);
                }
            }
            return null;
        }


        /// <summary>
        /// Lock or unlock the joint
        /// </summary>
        /// <param name="lockJoint">When true, the joint will be locked, it will be unlocked otherwise</param>
        /// /// <param name="force">Forces the joint to take over the locked state. Use with care!</param>
        public void SetJointLock(bool lockJoint, bool force)
        {
            //when the attchment failed, try again
            if ((!lockJoint && !hasParent) && (!HighLogic.LoadedSceneIsEditor))
            {
                RefreshReferences();
                if (!hasParent)
                {
                    isLockEngaged = true;
                    return;
                }
            }

            //only lock/unlock in valid situations
            if (((lockJoint) && (canLock)) || ((!lockJoint) && (canUnlock)) || force)
            {
                isLockEngaged = lockJoint;
                UpdateUI();
            }
        }

        //Get the title of the Module
        public string GetModuleTitle()
        {
            return Localizer.GetStringByTag("#LOC_KERBETROTTER.hitch.name");
        }

        //The Callback for drawing of the module
        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        //The information about the primary field
        public string GetPrimaryField()
        {
            return null;
        }
    }
}