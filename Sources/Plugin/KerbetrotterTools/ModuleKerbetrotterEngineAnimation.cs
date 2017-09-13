using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterEngineAnimation : PartModule
    {
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


        //-----------------------private members--------------------

        //the transform to rotate;
        private Transform[] rotationTransforms;

        //the engine
        private ModuleKerbetrotterEngine engine;

        //wheter the animation is valid
        private bool valid;

        //the real rotation rate
        private float smoothRate = 0.0f;

        //Minimal Rate in deg/sec
        private float minDegSec = 0.0f;

        //Maximal Rate in deg/sec
        private float maxDegSec = 0.0f;

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
                    if ((part.Modules[i] is ModuleKerbetrotterEngine) && (((ModuleKerbetrotterEngine)part.Modules[i]).EngineName == engineName))
                    {
                        engine = (ModuleKerbetrotterEngine)part.Modules[i];
                        break;
                    }
                }
            }
            minDegSec = 360 * minRotationSpeed;
            maxDegSec = 360 * maxRotationSpeed;

            valid = (rotationTransforms != null) & (rotationTransforms.Length > 0) & (engine != null);
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

            if (!engine.isOperational)
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
            rotationTransforms = null;
        }
    }
}
