/*
 * Copyright (C) 2017 Nils277 (https://github.com/Nils277)
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
using KSP.Localization;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// This module controls the the hovermotors, to be able to accelerate, break, strife and steer with the vessel when the hover engines are used
    /// </summary>
    class ModuleKerbetrotterEngineControl : PartModule
    {
        //------------------------Module Parameter-----------------

        /// <summary>
        /// The transform of the thrust vector to control
        /// </summary>
        [KSPField]
        public string thrustTransformName = "thrustTransform";

        /// <summary>
        /// A transform to use as reference for the maximal rotation
        /// </summary>
        [KSPField]
        public string referenceTransformName = "referenceTransform";

        /// <summary>
        /// A transform to use as reference for the maximal rotation
        /// </summary>
        [KSPField]
        public string engineName = "Hover Engine";

        /// <summary>
        /// A transform to use as reference for the maximal rotation
        /// </summary>
        [KSPField]
        public string animationID = string.Empty;

        /// <summary>
        /// The maximal angle for the control of the engines
        /// </summary>
        [KSPField]
        public float controlAngle = 20.0f;

        /// <summary>
        /// The maximal angle the engines can rotate in each direction
        /// </summary>
        [KSPField]
        public float maxAngle = 45.0f;

        /// <summary>
        /// The rotation speed in degree per second
        /// </summary>
        [KSPField]
        public float controlAngleRate = 7.0f;

        /// <summary>
        /// The rotation speed in degree per second
        /// </summary>
        [KSPField]
        public float maxSpeed = 60.0f;

        /// The rotation speed in degree per second
        /// </summary>
        [KSPField]
        public float maxAngleRate = 90.0f;

        //------------------------Interface-------------------

        [KSPField(guiName = "#autoLOC_6001467", guiActive = false, guiActiveEditor = false), UI_Toggle(affectSymCounterparts = UI_Scene.All)]
        public bool allowSteer = true;

        [KSPField(guiName = "#autoLOC_6001468", guiActive = false, guiActiveEditor = false), UI_Toggle(affectSymCounterparts = UI_Scene.All, enabledText = "Inverted", disabledText = "Normal")]
        public bool invertSteering = false;

        [KSPField(guiName = "#LOC_KERBETROTTER.engine.control.accelerate", guiActive = false, guiActiveEditor = false), UI_Toggle(affectSymCounterparts = UI_Scene.All)]
        public bool allowAccelerate = true;

        [KSPField(guiName = "#LOC_KERBETROTTER.engine.control.accelerate.direction", guiActive = false, guiActiveEditor = false), UI_Toggle(affectSymCounterparts = UI_Scene.All, enabledText = "Inverted", disabledText = "Normal")]
        public bool invertAccelerate = false;

        [KSPField(guiName = "#LOC_KERBETROTTER.engine.control.cancelrotation", guiActive = true), UI_Toggle(affectSymCounterparts = UI_Scene.All)]
        public bool cancelRotation = true;

        [KSPField(guiName = "#LOC_KERBETROTTER.engine.control.canceldrift", guiActive = true), UI_Toggle(affectSymCounterparts = UI_Scene.All)]
        public bool cancelDrift = true;

        [KSPField(guiName = "#LOC_KERBETROTTER.engine.control.ground", guiActive = true), UI_Toggle(affectSymCounterparts = UI_Scene.All)]
        public bool pointToGround = true;

        /// <summary>
        /// The proportional part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.control.rotation.p"), UI_FloatRange(affectSymCounterparts = UI_Scene.All, minValue = 0, maxValue = 1f, stepIncrement = 0.01f)]
        public float Kp_r = 0.5f;

        /// <summary>
        /// The integral part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.control.rotation.i"), UI_FloatRange(affectSymCounterparts = UI_Scene.All, minValue = 0, maxValue = 1f, stepIncrement = 0.01f)]
        public float Ki_r = 0.7f;

        /// <summary>
        /// The differential part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.control.rotation.d"), UI_FloatRange(affectSymCounterparts = UI_Scene.All, minValue = 0, maxValue = 1f, stepIncrement = 0.01f)]
        public float Kd_r = 0.15f;

        /// <summary>
        /// The proportional part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.control.speed.p"), UI_FloatRange(affectSymCounterparts = UI_Scene.All, minValue = 0, maxValue = 1f, stepIncrement = 0.01f)]
        public float Kp_s = 0.4f;

        /// <summary>
        /// The integral part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.control.speed.i"), UI_FloatRange(affectSymCounterparts = UI_Scene.All, minValue = 0, maxValue = 1f, stepIncrement = 0.01f)]
        public float Ki_s = 0.2f;

        /// <summary>
        /// The differential part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.control.speed.d"), UI_FloatRange(affectSymCounterparts = UI_Scene.All, minValue = 0, maxValue = 1f, stepIncrement = 0.01f)]
        public float Kd_s = 0.1f;

        //----------------------------Interaction-------------------------

        /// <summary>
        /// Toggle the Visibility of the advanced controls of the engine controller
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.control.advanced.show", guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ToogleControls()
        {
            showAdvancedControls = !showAdvancedControls;
            updateAndvanceControlVisibility();
        }

        /// <summary>
        /// Action to toggle the engine mode
        /// </summary>
        [KSPAction(guiName = "#LOC_KERBETROTTER.control.brake", actionGroup = KSPActionGroup.Brakes)]
        public void ToggleBreak(KSPActionParam param)
        {
            brakeEnabled = param.type == KSPActionType.Activate;
        }

        //----------------------------Private Members------------------------

        private bool showAdvancedControls = false;

        //the thrust transformt to change
        private Transform thrustTransform;

        //the reference transform
        private Transform referenceTransform;

        //the engine that is controlled
        private ModuleKerbetrotterEngine engine;

        //the engine that is controlled
        private ModuleAnimateGeneric anim;

        //the ID of the reference transform of the vessel
        private uint referenceTransformID;

        //the steering contol input
        private float steer;

        //the acceleration contol input
        private float accelerate;

        //the drift control input
        private float drift;

        //parameters for the rotation control
        private float[] rotationControl = new float[] { 0, 0, 0 };

        //parameters for drift control
        private float[] driftControl = new float[] { 0, 0, 0 };

        //parameters for drift control
        private float[] speedControl = new float[] { 0, 0, 0 };

        [KSPField(isPersistant = true)]
        private bool brakeEnabled = false;

        [KSPField(isPersistant = true)]
        private MainAxis mainAxis = MainAxis.FORWARD;

        [KSPField(isPersistant = true)]
        private bool invertedAxis = false;

        //the targeted speed of the vessel
        private float targetSpeed = 0;

        //the targeted drift of the vessel
        private float targetDrift = 0;

        //the targeted rotation of the vessel
        private float targetAngleRate = 0;

        //gets whether the animation was valid
        private bool animOk = false;

        //----------------------------Life Cycle------------------------

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight)
            {
                steer = allowSteer? invertSteering? -vessel.ctrlState.wheelSteer * 0.3f : vessel.ctrlState.wheelSteer * 0.3f : 0;
                accelerate = allowAccelerate? invertAccelerate? -vessel.ctrlState.wheelThrottle : vessel.ctrlState.wheelThrottle : 0;
                drift = allowAccelerate ? invertAccelerate? -vessel.ctrlState.X : vessel.ctrlState.X : 0;

                float speedFactor = Mathf.Min(1.0f, 1.0f / Mathf.Sqrt(accelerate * accelerate + drift * drift));

                targetSpeed = maxSpeed * accelerate * speedFactor;
                targetDrift = maxSpeed * drift * speedFactor; 
                targetAngleRate = maxAngleRate * steer;

                //when the reference transform changed
                if (referenceTransformID != vessel.referenceTransformId)
                {
                    updateMajorAxis();
                    referenceTransformID = vessel.referenceTransformId;
                }
            }
        }

        /// <summary>
        /// Start up the module and initialize it
        /// </summary>
        /// <param name="state">The start state of the module</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //referenceTransformID = vessel.referenceTransformId;

            thrustTransform = part.FindModelTransform(thrustTransformName.Trim());
            referenceTransform = part.FindModelTransform(referenceTransformName.Trim());

            //updateMajorAxis();
            updateAndvanceControlVisibility();

            //find the engine for this part
            if (engineName != string.Empty)
            {
                for (int i = 0; i < part.Modules.Count; i++)
                {
                    if ((part.Modules[i] is ModuleKerbetrotterEngine) && (((ModuleKerbetrotterEngine)part.Modules[i]).EngineName == engineName))
                    {
                        engine = (ModuleKerbetrotterEngine)part.Modules[i];
                        break;
                    }
                }
            }

            //find a corresponding animation if specified
            if (animationID != string.Empty)
            {
                for (int i = 0; i < part.Modules.Count; i++)
                {
                    if ((part.Modules[i] is ModuleAnimateGeneric) && (((ModuleAnimateGeneric)part.Modules[i]).moduleID == animationID))
                    {
                        anim = (ModuleAnimateGeneric)part.Modules[i];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Free all resources when the part is destroyed
        /// </summary>
        public void OnDestroy()
        {
            anim = null;
            engine = null;
        }

        /// <summary>
        /// Update for every physicss tic
        /// </summary>
        public virtual void FixedUpdate()
        {

            if (!HighLogic.LoadedSceneIsFlight)
            {
                return;
            }

            //start with the zero rotation
            Quaternion engineRotation = referenceTransform.rotation;

            bool validSituation = true;

            if (anim != null)
            {
                validSituation &= (anim.aniState == ModuleAnimateGeneric.animationStates.LOCKED) & (anim.animTime == 1.0f);
                if (!animOk && validSituation)
                {
                    updateMajorAxis();
                    animOk = true;
                }
                animOk = validSituation;
            }
            
            //check if the engine is can hover
            Vector3 downVed = Quaternion.LookRotation(Vector3.Normalize(vessel.mainBody.transform.position - referenceTransform.position)) * Vector3.forward;
            validSituation &= Vector3.Angle(downVed, referenceTransform.forward) < (60 + maxAngle);

            bool engineRunning = (engine == null) || (engine.HoverEnabled && (engine.EngineStatus == ModuleKerbetrotterEngine.EngineState.Running));

            //when the engine is in a valid situation
            if (engineRunning && validSituation)
            {
                //point to the ground when the rotation is not set
                if (pointToGround)
                {
                    engineRotation = Quaternion.LookRotation(vessel.mainBody.transform.position - thrustTransform.position);
                }

                //get the velocity of the vessel
                Vector3 vel = Quaternion.Inverse(vessel.ReferenceTransform.rotation) * vessel.GetSrfVelocity();

                //get the anglelar velocity of the vessel
                Vector3 rotateAxis = Quaternion.Inverse(vessel.ReferenceTransform.rotation) * (thrustTransform.position - vessel.CoM);
                if (mainAxis == MainAxis.FORWARD)
                {
                    rotateAxis.z = 0;
                }
                else if (mainAxis == MainAxis.UP)
                {
                    rotateAxis.y = 0;
                }
                else
                {
                    rotateAxis.x = 0;
                }
                rotateAxis = vessel.ReferenceTransform.rotation * rotateAxis;
                rotateAxis.Normalize();

                //set the control value to the input
                float setSteer = steer;
                float setDrift = drift;
                float setAccelerate = accelerate;


                //cancel steering when braking or cancel steering is enabled
                if (((cancelRotation) && (steer == 0)) || (brakeEnabled))
                {
                    if (mainAxis == MainAxis.FORWARD)
                    {
                        rotationControl = PID(-vessel.angularVelocity[2] * 180 / Mathf.PI, Kp_r, Ki_r, Kd_r, rotationControl[1], rotationControl[2], 1, 1, 20.0f, 2);
                    }
                    else if (mainAxis == MainAxis.UP){
                        rotationControl = PID(vessel.angularVelocity[1] * 180 / Mathf.PI, Kp_r, Ki_r, Kd_r, rotationControl[1], rotationControl[2], 1, 1, 20.0f, 2);
                    }
                    else
                    {
                        rotationControl = PID(-vessel.angularVelocity[0] * 180 / Mathf.PI, Kp_r, Ki_r, Kd_r, rotationControl[1], rotationControl[2], 1, 1, 20.0f, 2);
                    }
                    setSteer = rotationControl[0] * 0.5f;
                }
                //rotate the vessel with the desired angle rate
                else if ((steer != 0.0f) && (maxAngleRate != 0.0f))
                {
                    if (mainAxis == MainAxis.FORWARD)
                    {
                        rotationControl = PID(targetAngleRate - vessel.angularVelocity[2], Kp_r, Ki_r, Kd_r, rotationControl[1], rotationControl[2], 1, 1, 20.0f, 2);
                    }
                    else if (mainAxis == MainAxis.UP)
                    {
                        rotationControl = PID(targetAngleRate + vessel.angularVelocity[1], Kp_r, Ki_r, Kd_r, rotationControl[1], rotationControl[2], 1, 1, 20.0f, 2);
                    }
                    else
                    {
                        rotationControl = PID(targetAngleRate - vessel.angularVelocity[0], Kp_r, Ki_r, Kd_r, rotationControl[1], rotationControl[2], 1, 1, 20.0f, 2);
                    }
                    setSteer = rotationControl[0] * 0.2f;
                }
                //simply use input
                else
                {
                    rotationControl[0] = 0;
                    rotationControl[1] = 0;
                    rotationControl[2] = 0;
                }

                //cancel the drift when braking or cancel drift is enabled
                if (((cancelDrift) && (drift == 0)) || (brakeEnabled))
                {
                    if (mainAxis == MainAxis.FORWARD) {
                        driftControl = PID(vel[0] * 3, Kp_s, Ki_s, Kd_s, driftControl[1], driftControl[2], 3, 4, 6.5f, 1);
                    }
                    else if (mainAxis == MainAxis.UP)
                    {
                        driftControl = PID(vel[0] * 3, Kp_s, Ki_s, Kd_s, driftControl[1], driftControl[2], 3, 4, 6.5f, 1);
                    }
                    else 
                    {
                        driftControl = PID(vel[1] * 3, Kp_s, Ki_s, Kd_s, driftControl[1], driftControl[2], 3, 4, 6.5f, 1);
                    }
                    setDrift = driftControl[0];
                }
                //drift the vessel with the desired speed
                else if ((drift != 0.0f) && (maxSpeed != 0.0f))
                {
                    if (mainAxis == MainAxis.FORWARD)
                    {
                        driftControl = PID(targetDrift + vel[0], Kp_s, Ki_s, Kd_s, driftControl[1], driftControl[2], 3, 4, 6.5f, 1);
                    }
                    else if (mainAxis == MainAxis.UP)
                    {
                        driftControl = PID(targetDrift + vel[0], Kp_s, Ki_s, Kd_s, driftControl[1], driftControl[2], 3, 4, 6.5f, 1);
                    }
                    else
                    {
                        driftControl = PID(targetDrift + vel[1], Kp_s, Ki_s, Kd_s, driftControl[1], driftControl[2], 3, 4, 6.5f, 1);
                    }
                    setDrift = driftControl[0];
                }
                //simply use input
                else
                {
                    driftControl[0] = 0;
                    driftControl[1] = 0;
                    driftControl[2] = 0;
                }

                //cancel the speed when the brakes are enabled
                if (brakeEnabled)
                {
                    if ((mainAxis == MainAxis.FORWARD))
                    {
                        speedControl = PID(-vel[1] * 3, Kp_s, Ki_s, Kd_s, speedControl[1], speedControl[2], 3, 4, 6.5f, 1);
                    }
                    else if (mainAxis == MainAxis.UP)
                    {
                        speedControl = PID(-vel[2] * 3, Kp_s, Ki_s, Kd_s, speedControl[1], speedControl[2], 3, 4, 6.5f, 1);
                    }
                    else
                    {
                        speedControl = PID(-vel[2] * 3, Kp_s, Ki_s, Kd_s, speedControl[1], speedControl[2], 3, 4, 6.5f, 1);
                    }
                    setAccelerate = speedControl[0];
                }
                //accelerate the vessel to the desired speed
                else if ((accelerate != 0.0f) && (maxSpeed != 0.0f))
                {
                    if ((mainAxis == MainAxis.FORWARD))
                    {
                        speedControl = PID(targetSpeed - vel[1], Kp_s, Ki_s, Kd_s, speedControl[1], speedControl[2], 3, 4, 6.5f, 1);
                    }
                    else if (mainAxis == MainAxis.UP)
                    {
                        speedControl = PID(targetSpeed - vel[2], Kp_s, Ki_s, Kd_s, speedControl[1], speedControl[2], 3, 4, 6.5f, 1);
                    }
                    else
                    {
                        speedControl = PID(targetSpeed - vel[2], Kp_s, Ki_s, Kd_s, speedControl[1], speedControl[2], 3, 4, 6.5f, 1);
                    }
                    setAccelerate = speedControl[0];
                }
                //simply use the input
                else
                {
                    speedControl[0] = 0;
                    speedControl[1] = 0;
                    speedControl[2] = 0;
                }

                //rotate the engine to accelerate and decelerate
                if ((Mathf.Abs(setAccelerate) > 0.0f) || (Mathf.Abs(setDrift) > 0.0f))
                {
                    Vector3 v;
                    if (mainAxis == MainAxis.FORWARD)
                    {
                        v = (vessel.ReferenceTransform.right * setAccelerate) + (vessel.ReferenceTransform.up * setDrift);
                    }
                    else if (mainAxis == MainAxis.UP)
                    {
                        v = (vessel.ReferenceTransform.right * setAccelerate) + (vessel.ReferenceTransform.forward * setDrift);
                    }
                    else
                    {
                        v = (vessel.ReferenceTransform.up * setAccelerate) + (vessel.ReferenceTransform.forward * setDrift);
                    }

                    if (invertedAxis)
                    {
                        v = -v;
                    }
                   
                    engineRotation = Quaternion.AngleAxis(controlAngle * Mathf.Clamp(Mathf.Abs(setAccelerate) + Mathf.Abs(setDrift), 0.0f, 1.0f), v) * engineRotation;
                }

                //rotate the engine to steer
                if (Mathf.Abs(setSteer) > 0.0f)
                {
                    if (invertedAxis)
                    {
                        rotateAxis = -rotateAxis;
                    }
                    engineRotation = Quaternion.AngleAxis(controlAngle * setSteer, rotateAxis) * engineRotation;
                }
            }

            if (controlAngleRate != 0)
            {
                //smoothly update the rotation of the engine
                Vector3 targetForward = engineRotation * Vector3.forward;
                float angle = Vector3.Angle(thrustTransform.forward, targetForward);
                Vector3 target = Vector3.Slerp(thrustTransform.forward, targetForward, Mathf.Clamp((controlAngleRate * TimeWarp.deltaTime * controlAngleRate) / angle, 0.0f, 1.0f));
                thrustTransform.LookAt(thrustTransform.position + target);
            }
            else
            {
                //set the rotation
                thrustTransform.rotation = engineRotation;
            }

            //limit the maximal rotation of the engine
            float difference = Vector3.Angle(referenceTransform.forward, thrustTransform.forward);
            if (difference > maxAngle)
            {
                thrustTransform.rotation = Quaternion.Slerp(referenceTransform.rotation, thrustTransform.rotation, (maxAngle / difference));
            }
        }


        //--------------------------Control-------------------

        // PID controller to update the height of the engines
        private float[] PID(float error, float Kp, float Ki, float Kd, float error_last, float error_sum, float max_sum, float max_diff, float divider, float maxVal)
        {
            //proportional part
            float p_out = Kp * error / divider;

            //integral part
            error_sum += error * TimeWarp.deltaTime;
            error_sum = Mathf.Clamp(error_sum, -max_sum, max_sum);

            float i_out = Ki * error_sum / divider;

            //differential part
            float derivative = (error - error_last) / TimeWarp.deltaTime;
            derivative = Mathf.Clamp(derivative, -max_diff, max_diff);
            float d_out = Kd * derivative / divider;
            error_last = error;

            //get the control output
            float output = p_out + i_out + d_out;

            return new float[] {Mathf.Clamp(output, -maxVal, maxVal), error_last, error_sum};
        }

        //-----------------------------------Helper-------------------------------------

        //update the visibility of the advanced controls
        private void updateAndvanceControlVisibility()
        {
            // controls in flight
            Fields["Kp_r"].guiActive = showAdvancedControls;
            Fields["Ki_r"].guiActive = showAdvancedControls;
            Fields["Kd_r"].guiActive = showAdvancedControls;
            Fields["Kp_s"].guiActive = showAdvancedControls;
            Fields["Ki_s"].guiActive = showAdvancedControls;
            Fields["Kd_s"].guiActive = showAdvancedControls;
            Fields["allowSteer"].guiActive = showAdvancedControls;
            Fields["invertSteering"].guiActive = showAdvancedControls;
            Fields["allowAccelerate"].guiActive = showAdvancedControls;
            Fields["invertAccelerate"].guiActive = showAdvancedControls;

            //controls in editor
            Fields["Kp_r"].guiActiveEditor = showAdvancedControls;
            Fields["Ki_r"].guiActiveEditor = showAdvancedControls;
            Fields["Kd_r"].guiActiveEditor = showAdvancedControls;
            Fields["Kp_s"].guiActiveEditor = showAdvancedControls;
            Fields["Ki_s"].guiActiveEditor = showAdvancedControls;
            Fields["Kd_s"].guiActiveEditor = showAdvancedControls;
            Fields["allowSteer"].guiActiveEditor = showAdvancedControls;
            Fields["invertSteering"].guiActiveEditor = showAdvancedControls;
            Fields["allowAccelerate"].guiActiveEditor = showAdvancedControls;
            Fields["invertAccelerate"].guiActiveEditor = showAdvancedControls;

            Events["ToogleControls"].guiName = showAdvancedControls ? Localizer.Format("#LOC_KERBETROTTER.control.advanced.hide") : Localizer.Format("#LOC_KERBETROTTER.control.advanced.show");
        }

        private void updateMajorAxis()
        {
            float[] angles = new float[6];

            //forward
            angles[0] = Vector3.Angle(referenceTransform.forward, vessel.ReferenceTransform.forward);
            //backwards
            angles[1] = Vector3.Angle(referenceTransform.forward, -vessel.ReferenceTransform.forward);
            //right
            angles[2] = Vector3.Angle(referenceTransform.forward, vessel.ReferenceTransform.right);
            //left
            angles[3] = Vector3.Angle(referenceTransform.forward, -vessel.ReferenceTransform.right);
            //up
            angles[4] = Vector3.Angle(referenceTransform.forward, vessel.ReferenceTransform.up);
            //down
            angles[5] = Vector3.Angle(referenceTransform.forward, -vessel.ReferenceTransform.up);

            //find the axis with the lowes angle difference
            int min = 0;
            for (int i = 1; i < 6; i++)
            {
                if (angles[i] < angles[min])
                {
                    min = i;
                }
            }

            switch (min)
            {
                case 0:
                    mainAxis = MainAxis.FORWARD;
                    invertedAxis = false;
                    break;
                case 1:
                    mainAxis = MainAxis.FORWARD;
                    invertedAxis = true;
                    break;
                case 2:
                    mainAxis = MainAxis.RIGHT;
                    invertedAxis = false;
                    break;
                case 3:
                    mainAxis = MainAxis.RIGHT;
                    invertedAxis = true;
                    break;
                case 4:
                    mainAxis = MainAxis.UP;
                    invertedAxis = true;
                    break;
                case 5:
                    mainAxis = MainAxis.UP;
                    invertedAxis = false;
                    break;
                default:
                    mainAxis = MainAxis.FORWARD;
                    invertedAxis = false;
                    break;
            }
        }

        public enum MainAxis
        {
            FORWARD,
            RIGHT,
            UP,
        }
    }
}
