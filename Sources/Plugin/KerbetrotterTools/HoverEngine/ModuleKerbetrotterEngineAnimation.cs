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
    /// Class controlling the animation of the hover engine
    /// </summary>
    class ModuleKerbetrotterEngineAnimation : PartModule
    {
        #region------------------------Module Settings-----------------------

        /// <summary>
        /// The name of the engine to use
        /// </summary>
        [KSPField]
        public string engineName = "Hover Engine";

        /// <summary>
        /// The transform to animate
        /// </summary>
        [KSPField]
        public string transformName = "thrustTransform";

        /// <summary>
        /// The maximal rotation speed in RPM
        /// </summary>
        [KSPField]
        public float maxRotationSpeed = 50;

        /// <summary>
        /// The minimal rotation speed in RPM
        /// </summary>
        [KSPField]
        public float minRotationSpeed = 1;

        /// <summary>
        /// The minimal rotation speed in RPM
        /// </summary>
        [KSPField]
        public float speedChangeRate = 1;

        /// <summary>
        /// The curve describing the speed
        /// </summary>
        [KSPField]
        private FloatCurve speedCurve = null;

        /// <summary>
        /// The curve describing the speed
        /// </summary>
        [KSPField]
        private ModuleKerbetrotterEngineControl.MainAxis rotationAxis = ModuleKerbetrotterEngineControl.MainAxis.FORWARD;

        #endregion

        #region------------------------Private Members-----------------------

        //the transform to rotate;
        private Transform[] rotationTransforms;

        //the normal engine
        private ModuleEngines engine;

        //the hover engine
        private ModuleKerbetrotterEngine kEngine;

        //wheter the animation is valid
        private bool valid;

        //the real rotation rate
        private float smoothRate = 0.0f;

        //Minimal Rate in deg/sec
        private float minDegSec = 0.0f;

        //Maximal Rate in deg/sec
        private float maxDegSec = 0.0f;

        #endregion

        #region--------------------------Life Cycle--------------------------

        /// <summary>
        /// Start up the module and initialize it
        /// </summary>
        /// <param name="state">The start state of the module</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            rotationTransforms = part.FindModelTransforms(transformName);

            //find the engine for this part
            if (engineName != string.Empty)
            {
                for (int i = 0; i < part.Modules.Count; i++)
                {
                    if ((part.Modules[i] is ModuleEngines) && (((ModuleEngines)part.Modules[i]).engineID == engineName))
                    {
                        engine = (ModuleEngines)part.Modules[i];
                    }
                    if (part.Modules[i] is ModuleKerbetrotterEngine)
                    {
                        kEngine = (ModuleKerbetrotterEngine)part.Modules[i];
                        break;
                    }
                }
            }
            minDegSec = 360 * minRotationSpeed;
            maxDegSec = 360 * maxRotationSpeed;
            valid = (rotationTransforms != null) & (rotationTransforms.Length > 0) && ((engine != null) || (kEngine != null));
        }

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if ((!HighLogic.LoadedSceneIsFlight) || (!valid))
            {
                return;
            }

            float angleRate = 0;

            if (kEngine != null)
            {
                if (!kEngine.isRunning)
                {
                    angleRate = 0;
                }
                else if (speedCurve != null)
                {
                    angleRate = Mathf.Lerp(minDegSec, maxDegSec, Mathf.Clamp(speedCurve.Evaluate(kEngine.throttleSetting), 0, 1));
                }
                else
                {
                    angleRate = Mathf.Lerp(minDegSec, maxDegSec, Mathf.Clamp(kEngine.throttleSetting, 0, 1));
                }
            }
            else if (engine != null)
            {
                if (!engine.isActiveAndEnabled || !engine.EngineIgnited || engine.flameout)
                {
                    angleRate = 0;
                }
                else if (speedCurve != null)
                {
                    angleRate = Mathf.Lerp(minDegSec, maxDegSec, Mathf.Clamp(speedCurve.Evaluate(engine.throttleSetting), 0, 1));
                }
                else
                {
                    angleRate = Mathf.Lerp(minDegSec, maxDegSec, Mathf.Clamp(engine.throttleSetting, 0, 1));
                }
            }

            smoothRate = Mathf.Lerp(smoothRate, angleRate, TimeWarp.deltaTime / speedChangeRate);

            if (smoothRate > 0.1)
            {
                switch (rotationAxis)
                {
                    case ModuleKerbetrotterEngineControl.MainAxis.FORWARD:
                        for (int i = 0; i < rotationTransforms.Length; i++)
                        {
                            rotationTransforms[i].Rotate(Vector3.forward, smoothRate * TimeWarp.deltaTime);
                        }
                        break;
                    case ModuleKerbetrotterEngineControl.MainAxis.RIGHT:
                        for (int i = 0; i < rotationTransforms.Length; i++)
                        {
                            rotationTransforms[i].Rotate(Vector3.right, smoothRate * TimeWarp.deltaTime);
                        }
                        break;
                    case ModuleKerbetrotterEngineControl.MainAxis.UP:
                        for (int i = 0; i < rotationTransforms.Length; i++)
                        {
                            rotationTransforms[i].Rotate(Vector3.up, smoothRate * TimeWarp.deltaTime);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Free all resources when the part is destroyed
        /// </summary>
        public void OnDestroy()
        {
            engine = null;
            kEngine = null;
            rotationTransforms = null;
        }

        #endregion
    }
}
