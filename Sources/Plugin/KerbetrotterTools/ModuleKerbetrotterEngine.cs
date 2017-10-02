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
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterEngine : PartModule, IThrustProvider, IModuleInfo, IResourceConsumer, IEngineStatus
    {
        //-----------------------------Engine Settigns----------------------------

        /// <summary>
        /// The name of the transforms to apply the thrust to
        /// </summary>
        [KSPField]
        public string thrustVectorTransformName = "thrustTransform";

        /// <summary>
        /// The name of the transform to check the distance to the ground
        /// </summary>
        [KSPField]
        public string heightTransformName = "heigthTransform";

        /// <summary>
        /// The minimal thrust of the engine
        /// </summary>
        //[KSPField]
        public float minThrust = 0.0f;

        /// <summary>
        /// The maximal thrust of the enginet
        /// </summary>
        //[KSPField]
        public float maxThrust = 100.0f;

        /// <summary>
        /// The speed the thrust of the engine updates
        /// </summary>
        [KSPField]
        public float thrustSpeed = 0.1f;

        /// <summary>
        /// The minimal height for hovering
        /// </summary>
        [KSPField]
        public float minHoverHeight = 1.0f;

        /// <summary>
        /// The maximal height for hovering
        /// </summary>
        [KSPField]
        public float maxHoverHeight = 100.0f;

        /// <summary>
        /// Sets whether the engine can hover or not
        /// </summary>
        [KSPField]
        public bool allowHover = false;

        /// <summary>
        /// The thrust limiter setting
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#autoLOC_6001363"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.5f,affectSymCounterparts = UI_Scene.All)]
        public float thrustLimiter = 100.0f;

        /// <summary>
        /// The height offset for the engine to balance it out
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.engine.offset"), UI_FloatRange(minValue = -2, maxValue = 2f, stepIncrement = 0.01f, affectSymCounterparts = UI_Scene.All)]
        public float heightOffset = 0.0f;

        /// <summary>
        /// The type of the engine
        /// </summary>
        [KSPField]
        public EngineType engineType = EngineType.Generic;

        /// <summary>
        /// The status of the engine
        /// </summary>
        [KSPField(guiName = "#LOC_KERBETROTTER.engine.profile", guiActive = true, guiActiveEditor = false)]
        public string pidProfile = string.Empty;

        /// <summary>
        /// The proportional part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.engine.p"), UI_FloatRange(minValue = 0, maxValue = 5f, stepIncrement = 0.1f, affectSymCounterparts = UI_Scene.All)]
        public float Kp = 1f;

        /// <summary>
        /// The integral part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.engine.i"), UI_FloatRange(minValue = 0, maxValue = 5f, stepIncrement = 0.1f, affectSymCounterparts = UI_Scene.All)]
        public float Ki = 1.5f;

        /// <summary>
        /// The differential part of the controller
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KERBETROTTER.engine.d"), UI_FloatRange(minValue = 0, maxValue = 5f, stepIncrement = 0.1f, affectSymCounterparts = UI_Scene.All)]
        public float Kd = 5f;

        /// <summary>
        /// Name to identify the engine
        /// </summary>
        [KSPField]
        public string EngineName = "Hover Engine";

        //----------------------------Visual feedback of the engine-------------------------

        /// <summary>
        /// The status of the engine
        /// </summary>
        [KSPField(guiName = "#autoLOC_475347", guiActive = true)]
        public string status = "#autoLOC_6001078";

        /// <summary>
        /// The engine mode active to the player
        /// </summary>
        [KSPField(guiName = "#LOC_KERBETROTTER.engine.flightMode", guiActive = true)]
        public string engineMode = "";

        /// <summary>
        /// Whether the engine mode is enabled or not
        /// </summary>
        [KSPField(guiName = "#LOC_KERBETROTTER.engine.hovering", guiActive = true)]
        public string hovering = "";


        //-----------------------------Saved engine state--------------------

        /// <summary>
        /// Saves whether the engine is turned on
        /// </summary>
        [KSPField(isPersistant = true)]
        private bool ignited = false;

        /// <summary>
        /// Saves if the engines is staged
        /// </summary>
        private bool staged = false;

        /// <summary>
        /// The height to hover at for this engine
        /// </summary>
        [KSPField(isPersistant = true)]
        private float hoverHeight = 0.0f;

        /// <summary>
        /// Saves whether the height of the hover engine is set
        /// </summary>
        [KSPField(isPersistant = true)]
        private bool heightSet = false;

        /// <summary>
        /// The last height error
        /// </summary>
        [KSPField(isPersistant = true)]
        private float PID_lastError = 0.0f;

        /// <summary>
        /// The sum of the height errors
        /// </summary>
        [KSPField(isPersistant = true)]
        private float PID_errorSum = 0.0f;

        /// <summary>
        /// The set mode of the engine
        /// </summary>
        [KSPField(isPersistant = true)]
        private bool hoverEnabled = false;

        /// <summary>
        /// Value holding the smoothed throttle value for the engine
        /// </summary>
        [KSPField(isPersistant = true)]
        private float engineThrottle = 0.0f;

        /// <summary>
        /// The current mode of the engine
        /// </summary>
        [KSPField(isPersistant = true)]
        private int currentEngineMode = 0;

        /// <summary>
        /// Whether the user has a custom PID profile
        /// </summary>
        [KSPField(isPersistant = true)]
        private bool customPID = false;

        //----------------------------Private members------------------------

        //The transforms the apply the thrust to
        private Transform[] thrustTransforms;

        //The transform the get the height
        public Transform heightTransform;

        //Value holding the throttle value set for the vessel
        private float vesselThrottle = 0.0f;

        //The local rotation from the height transform Stored for convenience purposes
        private Quaternion heightLocalRotation;

        //the current thrust of the engine
        private float engineThrust = 0.0f;

        //saves whether the height should be changed
        private float heightChange = 0;

        // the propellants used by the engine
        private List<KerbetrotterEngineMode> engineModes = new List<KerbetrotterEngineMode>();

        // the profiles for different planets
        private List<KerbetrotterPIDProfile> profiles = new List<KerbetrotterPIDProfile>();

        // the smoothed throttle for the effects
        private float effectvalue = 0.0f;

        // The status of the engine
        public EngineState engineState;

        //sets whether the the advanced controls are visible
        private bool showAdvancedControls = false;

        //the altitude of the engine
        private float altitude;

        //The mask for the ray cast to the terrain
        private int raycastMask = (1 << 28 | 1 << 15);

        //The correction for the altitude
        private float alitudeCorrection;

        //the divider when multiple thrusts are present
        private float thrustDivider = 1.0f;

        //the current profile
        private int currentProfile = 0; 

        //-----------------------------Strings--------------------------

        string engine_hover_enabled = string.Empty;
        string engine_hover_disabled = string.Empty;
        string engine_state_inative = string.Empty;
        string engine_state_running = string.Empty;
        string engine_state_flameout = string.Empty;
        string engine_state_noAtmo = string.Empty;
        string engine_state_noOxy = string.Empty;
        string engine_state_noWater = string.Empty;
        string engine_state_onHold = string.Empty;

        //----------------------------Interaction-----------------------

        /// <summary>
        /// Event to toggle the engine mode
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.engine.mode.change", guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ToggleMode()
        {
            currentEngineMode++;
            if (currentEngineMode >= engineModes.Count)
            {
                currentEngineMode = 0;
            }
            updateEngineMode();

            //update all symmetrical modules
            int index = part.getModuleIndex(this);
            for (int i = 0; i < part.symmetryCounterparts.Count; i++)
            {
                if (part.symmetryCounterparts[i].Modules.Count > index)
                {
                    PartModule module = part.symmetryCounterparts[i].Modules[index];
                    if (module is ModuleKerbetrotterEngine)
                    {
                        ((ModuleKerbetrotterEngine)module).setMode(currentEngineMode);
                    }
                }
            }
        }

        /// <summary>
        /// Event to toggle the hover
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.engine.togglehover", guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ToggleHover()
        {
            hoverEnabled = !hoverEnabled;
            updateHoverStatus();

            //update all symmetrical modules
            int index = part.getModuleIndex(this);
            for (int i = 0; i < part.symmetryCounterparts.Count; i++)
            {
                if (part.symmetryCounterparts[i].Modules.Count > index)
                {
                    PartModule module = part.symmetryCounterparts[i].Modules[index];
                    if (module is ModuleKerbetrotterEngine)
                    {
                        ((ModuleKerbetrotterEngine)module).setHoverEnabled(hoverEnabled, true);
                    }
                }
            }
        }

        /// <summary>
        /// Event to reset the profile
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.engine.profile.reset", guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ResetProfile()
        {
            if (profiles.Count == 0)
            {
                Events["ResetProfile"].guiActive = false;
                Events["ResetProfile"].guiActiveEditor = false;
                return;
            }

            setDefaultProfile();

            //update all symmetrical modules
            int index = part.getModuleIndex(this);
            for (int i = 0; i < part.symmetryCounterparts.Count; i++)
            {
                if (part.symmetryCounterparts[i].Modules.Count > index)
                {
                    PartModule module = part.symmetryCounterparts[i].Modules[index];
                    if (module is ModuleKerbetrotterEngine)
                    {
                        ((ModuleKerbetrotterEngine)module).setDefaultProfile();
                    }
                }
            }
        }

        /// <summary>
        /// Toggle the Visibility of the advanced controls of the engine controller
        /// </summary>
        [KSPEvent(guiName = "Show Engine Settings", guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ToogleControls()
        {
            showAdvancedControls = !showAdvancedControls;
            updateAndvanceControlVisibility();
        }

        /// <summary>
        /// Event to activate the engine
        /// </summary>
        [KSPEvent(guiName = "#autoLOC_6001382", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void Activate()
        {
            ignited = true;
            staged = true;
            updateHoverStatus();
            updateEngineIgnitionStatus();
            updateActivationCounterparts();
        }

        /// <summary>
        /// Event to shut the engine down
        /// </summary>
        [KSPEvent(guiName = "#autoLOC_6001381", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void Deactivate()
        {
            ignited = false;
            updateHoverStatus();
            updateEngineIgnitionStatus();
            updateActivationCounterparts();
        }

        /// <summary>
        /// Action to activate the engine
        /// </summary>
        /// <param name="param"></param>
        [KSPAction("#autoLOC_6001382")]
        public void ActivateAction(KSPActionParam param)
        {
            ignited = true;
            staged = true;
            updateHoverStatus();
            updateEngineIgnitionStatus();
        }

        /// <summary>
        /// Action to shut the engine down
        /// </summary>
        [KSPAction("#autoLOC_6001381")]
        public void DeactivateAction(KSPActionParam param)
        {
            ignited = false;
            updateHoverStatus();
            updateEngineIgnitionStatus();
        }

        /// <summary>
        /// Action to toggle the engine
        /// </summary>
        [KSPAction("#autoLOC_6001380")]
        public void ToggleAction(KSPActionParam param)
        {
            ignited = !ignited;
            if (ignited)
            {
                staged = true;
            }
            updateHoverStatus();
            updateEngineIgnitionStatus();
        }

        /// <summary>
        /// Action to toggle the engine to hovering mode
        /// </summary>
        [KSPAction("#LOC_KERBETROTTER.engine.mode.switch_hover")]
        public void EnableHoverAction(KSPActionParam param)
        {
            hoverEnabled = true;
            updateHoverStatus();
        }

        /// <summary>
        /// Action to toggle the engine to free run mode
        /// </summary>
        [KSPAction("#LOC_KERBETROTTER.engine.mode.switch_free")]
        public void DisableHoverAction(KSPActionParam param)
        {
            hoverEnabled = false;
            updateHoverStatus();
            
        }

        /// <summary>
        /// Action to toggle the hover effect
        /// </summary>
        [KSPAction("#LOC_KERBETROTTER.engine.togglehover")]
        public void ToggleHoverAction(KSPActionParam param)
        {
            hoverEnabled = !hoverEnabled;
            updateHoverStatus();
        }

        /// <summary>
        /// Action to toggle the engine mode
        /// </summary>
        [KSPAction("#autoLOC_6001393")]
        public void ToggleModeAction(KSPActionParam param)
        {
            currentEngineMode++;
            if (currentEngineMode >= engineModes.Count)
            {
                currentEngineMode = 0;
            }
            updateEngineMode();
        }

        //----------------------------Life Cycle------------------------

        /// <summary>
        /// Get the switchable resources on load to allow the partInfo to be populated
        /// </summary>
        /// <param name="partNode"> The config node for this partmodule</param>
        public override void OnLoad(ConfigNode partNode)
        {
            base.OnLoad(partNode);

            loadModesInternal(partNode);
        }

        /// <summary>
        /// Register for events when the main body changed
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();
            GameEvents.onVesselSOIChanged.Add(onMainBodyChanged);
        }

        public override string GetInfo()
        {
            StringBuilder info = new StringBuilder();
            bool showMode = (engineModes.Count > 1);

            for (int i = 0; i < engineModes.Count; i++)
            {
                info.Append(engineModes[i].getDescription(showMode));
                if (i < engineModes.Count-1)
                {
                    info.AppendLine();
                }
            }
            return info.ToString();
        }

        /// <summary>
        /// Start up the module and initialize it
        /// </summary>
        /// <param name="state">The start state of the module</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            part.stagingIcon = "LIQUID_ENGINE";

            loadTexts();

            thrustTransforms = part.FindModelTransforms(thrustVectorTransformName.Trim());
            heightTransform = part.FindModelTransform(heightTransformName);
            heightLocalRotation = heightTransform.localRotation;

            if ((thrustTransforms != null) && (thrustTransforms.Length > 0)) {
                thrustDivider = 1.0f / thrustTransforms.Length;
            }
            updateEngineIgnitionStatus();
            updateHoverStatus();
            loadModes(part.partInfo.partConfig);
            LoadPIDProfiles(part.partInfo.partConfig);
            updateEngineMode();
            updateAndvanceControlVisibility();

            if ((!customPID))
            {
                ResetProfile();
            }

            if (engineModes.Count < 2)
            {
                Events["ToggleMode"].guiActive = false;
                Events["ToggleMode"].guiActiveEditor = false;
                Actions["ToggleAction"].active = false;
                Fields["engineMode"].guiActive = false;
            }

            if (!allowHover)
            {
                Events["ToggleHover"].guiActive = false;
                Events["ToggleHover"].guiActiveEditor = false;
                Events["ToogleControls"].guiActive = false;
                Events["ToogleControls"].guiActiveEditor = false;
                Actions["ToggleHoverAction"].active = false;
                Actions["EnableHoverAction"].active = false;
                Actions["DisableHoverAction"].active = false;
                Fields["hovering"].guiActive = false;
                Fields["heightOffset"].guiActive = false;
                Fields["heightOffset"].guiActiveEditor = false;
            }
        }

        /// <summary>
        /// Free all resources when the part is destroyed
        /// </summary>
        public void OnDestroy()
        {
            GameEvents.onVesselSOIChanged.Remove(onMainBodyChanged);//engineModules = null;
        }

        /// <summary>
        /// Activate the engine
        /// </summary>
        public override void OnActive()
        {
            ignited = true;
            staged = true;
            updateHoverStatus();
            updateEngineIgnitionStatus();
        }

        /// <summary>
        /// Update for every physicss tic
        /// </summary>
        public virtual void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                updateStatus();
                return;
            }

            //the final throttle setting for the engine
            float finalThrottle = 0.0f;

            updateStatus();

            if (engineState == EngineState.Running)
            {
                //when hovering point the transform to the surface
                if (hoverEnabled)
                {
                    //when the control height is not set, set it
                    if (heightSet)
                    {
                        hoverHeight -= heightChange * TimeWarp.fixedDeltaTime * 2;
                        if (hoverHeight > maxHoverHeight)
                        {
                            hoverHeight = maxHoverHeight;
                        }
                        else if (hoverHeight < minHoverHeight)
                        {
                            hoverHeight = minHoverHeight;
                        }
                    }

                    //when the desired height is set, hover
                    if (heightSet)
                    {
                        //get the actual height from the surface
                        float height = getSurfaceDistance();

                        if (!float.IsNaN(height))
                        {
                            finalThrottle = PID(height);
                        }
                    }
                }
                //else the engine works in normal conditions
                else
                {
                    heightTransform.localRotation = heightLocalRotation;
                    finalThrottle = (thrustLimiter / 100.0f) * vesselThrottle;
                }                
            }

            //smoothly update the engine throttle
            if (finalThrottle > engineThrottle)
            {
                engineThrottle = engineThrottle + TimeWarp.deltaTime / thrustSpeed;
                if (engineThrottle > finalThrottle)
                {
                    engineThrottle = finalThrottle;
                }
            }
            else if (finalThrottle < engineThrottle)
            {
                engineThrottle = engineThrottle - TimeWarp.deltaTime / thrustSpeed;
                if (engineThrottle < finalThrottle)
                {
                    engineThrottle = finalThrottle;
                }
            }
            
            //apply the thrust to the engines when the are active
            if (engineThrottle > 0)
            {
                applyThrust();
            }

            updateStatus();
        }

        /// <summary>
        /// Update for every other tic
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();

            checkPIDChanged();

            if (HighLogic.LoadedSceneIsFlight)
            {
                //get the contral data from the vessel
                vesselThrottle = vessel.ctrlState.mainThrottle;

                if (hoverEnabled && heightSet)
                {
                    heightChange = vessel.ctrlState.Y;
                }

                //update the effects of the engine
                updateEffects();
            }
        }

        //--------------------------Public interface---------------

        /// <summary>
        /// Get the height offset
        /// </summary>
        public float HeightOffset
        {
            get
            {
                return heightOffset;
            }
        }

        /// <summary>
        /// Get whether the hover of the engine is enbled
        /// </summary>
        public bool HoverEnabled
        {
            get
            {
                return hoverEnabled;
            }
        }

        /// <summary>
        /// Get the status of the engine
        /// </summary>
        public EngineState EngineStatus
        {
            get
            {
                return engineState;
            }
        }

        /// <summary>
        /// The public field for the throttle of the engine
        /// </summary>
        public float Throttle
        {
            get
            {
                return engineThrottle;
            }
        }

        /// <summary>
        /// Set wheter the engines ia activated or not
        /// </summary>
        /// <param name="activated">True when the engine should be activated, else false</param>
        public void setActivated(bool activated)
        {
            ignited = activated;
            staged = activated;
            updateHoverStatus();
            updateEngineIgnitionStatus();
        }

        /// <summary>
        /// Set the mode if the engine
        /// </summary>
        /// <param name="mode">The new mode of the engine</param>
        public void setMode(int mode)
        {
            currentEngineMode = mode;
            updateEngineMode();
        }

        /// <summary>
        /// Set whether the engine should hover
        /// </summary>
        /// <param name="hover">True when hover should be enabled, else flase</param>
        public void setHoverEnabled(bool hover, bool fireEvent)
        {
            if (allowHover)
            {
                hoverEnabled = hover;
                updateHoverStatus(fireEvent);
            }
        }

        /// <summary>
        /// Reset the PID profile to the detault one
        /// </summary>
        public void setDefaultProfile()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                //reset to the default profile
                for (int i = 0; i < profiles.Count; i++)
                {
                    if (profiles[i].IsDefault)
                    {
                        Kp = profiles[i].P;
                        Ki = profiles[i].I;
                        Kd = profiles[i].D;
                        currentProfile = i;
                        pidProfile = Localizer.Format("#LOC_KERBETROTTER.engine.profile.custom");
                        customPID = false;
                        break;
                    }
                }
                //when there is no default, use the first
                if (customPID)
                {
                    Kp = profiles[0].P;
                    Ki = profiles[0].I;
                    Kd = profiles[0].D;
                    currentProfile = 0;
                    pidProfile = Localizer.Format("#LOC_KERBETROTTER.engine.profile.custom");
                    customPID = false;
                }

            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                UpdateMainBody(vessel.mainBody);
            }
            Events["ResetProfile"].guiActive = false;
            Events["ResetProfile"].guiActiveEditor = false;
        }

        /// <summary>
        /// Set the height to hover at
        /// </summary>
        /// <param name="height">The hover height</param>
        public void setHoverHeight(float height)
        {
            if (hoverEnabled)
            {
                hoverHeight = height;
                heightSet = true;
            }
        }

        /// <summary>
        /// Set the height to hover at
        /// </summary>
        /// <param name="height">The hover height</param>
        public void setAltitudeCorrection(float correction)
        {
            if (hoverEnabled)
            {
                alitudeCorrection = correction;
            }
        }

        //----------------------------Helper------------------------

        private void checkPIDChanged()
        {
            if ((!customPID) && (profiles.Count > 0))
            {
                if ((profiles[currentProfile].P != Kp) || (profiles[currentProfile].I != Ki) || (profiles[currentProfile].D != Kd))
                {
                    customPID = true;
                    currentProfile = -1;
                    pidProfile = Localizer.Format("#LOC_KERBETROTTER.engine.profile.custom");
                    Events["ResetProfile"].guiActive = true;
                    Events["ResetProfile"].guiActiveEditor = true;
                }
            }
        }

        private void onMainBodyChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> data)
        {
            if (data.host == vessel)
            {
                UpdateMainBody(data.to);
            }
        }

        private void UpdateMainBody(CelestialBody mainBody)
        {
            if (profiles.Count == 0)
            {
                return;
            }

            //find the current planet
            string planet = mainBody.name;
            //reset the planet default
            for (int i = 0; i < profiles.Count; i++)
            {
                if (planet == profiles[i].Profile)
                {
                    Kp = profiles[i].P;
                    Ki = profiles[i].I;
                    Kd = profiles[i].D;
                    currentProfile = i;
                    pidProfile = Localizer.Format("<<1>>",mainBody.displayName);
                    customPID = false;
                    break;
                }
            }
            //when the planet is not found, reset to default
            if (customPID)
            {
                //reset to the default profile
                for (int i = 0; i < profiles.Count; i++)
                {
                    if (profiles[i].IsDefault)
                    {
                        Kp = profiles[i].P;
                        Ki = profiles[i].I;
                        Kd = profiles[i].D;
                        currentProfile = i;
                        pidProfile = Localizer.Format("#LOC_KERBETROTTER.engine.profile.default");
                        customPID = false;
                        break;
                    }
                }
            }
            //when there is no default, use the first
            if (customPID)
            {
                Kp = profiles[0].P;
                Ki = profiles[0].I;
                Kd = profiles[0].D;
                currentProfile = 0;
                pidProfile = Localizer.Format("#LOC_KERBETROTTER.engine.profile.default");
                customPID = false;
            }
        }

        //Update the visible mode of the engine
        private void updateEngineMode()
        {
            if (currentEngineMode < engineModes.Count)
            {
                engineMode = engineModes[currentEngineMode].Name;
            }
            else
            {
                engineMode = engine_hover_disabled;
            }

        }

        //update the hover status, sets whether an event should be fired or not
        private void updateHoverStatus(bool fireEvent)
        {
            if ((!hoverEnabled) || (!ignited))
            {
                PID_errorSum = 0;
                PID_lastError = 0;
                alitudeCorrection = 0;
                heightSet = false;
            }
            if (fireEvent) {
                KerbetrotterEngineHoverEvent.onEngineHover.Fire(this, hoverEnabled & ignited);
            }
            hovering = hoverEnabled ? engine_hover_enabled : engine_hover_disabled;
            Events["ToggleHover"].guiName = hoverEnabled? Localizer.Format("#LOC_KERBETROTTER.engine.mode.switch_free") : Localizer.Format("#LOC_KERBETROTTER.engine.mode.switch_hover");
        }

        //update the status of the hover mode
        private void updateHoverStatus()
        {
            updateHoverStatus(true);
        }


        //update the visibility of the advanced controls
        private void updateAndvanceControlVisibility()
        {
            // controls in flight
            Fields["pidProfile"].guiActive = showAdvancedControls;
            Fields["Kp"].guiActive = showAdvancedControls;
            Fields["Ki"].guiActive = showAdvancedControls;
            Fields["Kd"].guiActive = showAdvancedControls;

            //controls in editor
            Fields["pidProfile"].guiActiveEditor = false;
            Fields["Kp"].guiActiveEditor = showAdvancedControls;
            Fields["Ki"].guiActiveEditor = showAdvancedControls;
            Fields["Kd"].guiActiveEditor = showAdvancedControls;

            //change the text of the control
            Events["ToogleControls"].guiName = showAdvancedControls ? Localizer.Format("#LOC_KERBETROTTER.engine.advanced.hide") : Localizer.Format("#LOC_KERBETROTTER.engine.advanced.show");
            Events["ResetProfile"].guiActive = showAdvancedControls & customPID;
            Events["ResetProfile"].guiActiveEditor = showAdvancedControls & customPID;
        }

        //Load all the texts that are used
        private void loadTexts()
        {
            engine_hover_disabled = Localizer.Format("#autoLOC_900890");
            engine_hover_enabled = Localizer.Format("#autoLOC_900889");
            engine_state_inative = Localizer.Format("#autoLOC_6001078");
            engine_state_running = Localizer.Format("#autoLOC_7001223");
            engine_state_flameout = Localizer.Format("#autoLOC_219016");
            engine_state_noAtmo = Localizer.Format("#LOC_KERBETROTTER.engine.state.noAtmosphere");
            engine_state_noOxy = Localizer.Format("#LOC_KERBETROTTER.engine.state.noOxygen");
            engine_state_noWater = Localizer.Format("#LOC_KERBETROTTER.engine.state.notInWater");
            engine_state_onHold = Localizer.Format("#LOC_KERBETROTTER.engine.state.onHold");
        }

        //Update the activation of all symmetry counterparts
        private void updateActivationCounterparts()
        {
            //update all symmetrical modules
            int index = part.getModuleIndex(this);
            for (int i = 0; i < part.symmetryCounterparts.Count; i++)
            {
                if (part.symmetryCounterparts[i].Modules.Count > index)
                {
                    PartModule module = part.symmetryCounterparts[i].Modules[index];
                    if (module is ModuleKerbetrotterEngine)
                    {
                        ((ModuleKerbetrotterEngine)module).setActivated(ignited);
                    }
                }
            }
        }

        // Load the needed propellants
        private void loadModes(ConfigNode node)
        {
            engineModes = new List<KerbetrotterEngineMode>();

            ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");
            int index = part.Modules.IndexOf(this);
            if (index != -1 && index < modules.Length && modules[index].GetValue("name") == moduleName)
            {
                loadModesInternal(modules[index]);
            }
            else
            {
                Debug.Log("[LYNX] Engine Config NOT found");
            }
        }

        //Load the profiles for the PID controller
        private void LoadPIDProfiles(ConfigNode node)
        {
            profiles = new List<KerbetrotterPIDProfile>();
            ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");
            int index = part.Modules.IndexOf(this);
            if (index != -1 && index < modules.Length && modules[index].GetValue("name") == moduleName)
            {
                LoadPIDProfilesInternal(modules[index]);
            }
            else
            {
                Debug.Log("[LYNX] Engine Config NOT found");
            }
        }

        // Load the needed propellants
        private void loadModesInternal(ConfigNode node)
        {
            ConfigNode[] propConfig = node.GetNodes("MODE");
            for (int i = 0; i < propConfig.Length; i++)
            {
                KerbetrotterEngineMode mode = new KerbetrotterEngineMode(propConfig[i], maxThrust);
                engineModes.Add(mode);
            }
        }

        // Load the needed propellants
        private void LoadPIDProfilesInternal(ConfigNode node)
        {
            ConfigNode[] propConfig = node.GetNodes("PID-PROFILE");
            for (int i = 0; i < propConfig.Length; i++)
            {
                KerbetrotterPIDProfile profile = new KerbetrotterPIDProfile(propConfig[i]);
                profiles.Add(profile);
            }
        }

        // Apply the thrust to the engine, depending on the parameters
        private void applyThrust()
        {
            if ((thrustTransforms == null) || (thrustTransforms.Length == 0) || (GetComponent<Rigidbody>() == null))
            {
                return;
            }

            float thrust = 0;

            //get the maximal thrust depending on atmosphere
            if (engineModes.Count > currentEngineMode)
            {
                thrust = engineModes[currentEngineMode].getThrust(vessel.speed, (float)vessel.atmDensity, engineThrottle, part) * thrustDivider;
            }

            //apply thrust to all engines
            for (int i = 0; i < thrustTransforms.Length; i++)
            {
                GetComponent<Rigidbody>().AddForceAtPosition(-thrustTransforms[i].forward * thrust, thrustTransforms[i].position);
            }
        }

        // Update the visibility of the events to activate and deactivate the engine
        private void updateEngineIgnitionStatus()
        {
            Events["Activate"].guiActive = !ignited;
            Events["Deactivate"].guiActive = ignited;
        }

        // Update of the particle and sound effects
        private void updateEffects()
        {
            if (ignited && (engineState == EngineState.Running))
            {
                float r = 0;
                //add some nouse to prevent interferences from the controller
                if (engineThrottle > 0)
                {
                    r = (UnityEngine.Random.value - 0.5f) * 0.015f;
                }
                
                effectvalue = Mathf.Lerp(effectvalue + r, engineThrottle, 0.1f);
                if (effectvalue < 0.01f)
                {
                    effectvalue = 0.0f;
                }

                part.Effect("running", Mathf.Clamp(effectvalue, 0.01f, 1f));
            }
            else
            {
                part.Effect("running", 0f);
            }
        }

        // PID controller to update the height of the engines
        private float PID(float height)
        {
            float error = hoverHeight - height - alitudeCorrection + heightOffset;

            //proportional part
            float p_out = Kp * error / 10;

            //integral part
            PID_errorSum += error * TimeWarp.deltaTime;
            PID_errorSum = Mathf.Clamp(PID_errorSum, -3f, 10f);

            float i_out = Ki * PID_errorSum / 10;

            //differential part
            float derivative = (error -PID_lastError) / TimeWarp.deltaTime;
            derivative = Mathf.Clamp(derivative, -3, 3);
            float d_out = Kd * derivative / 10;
            PID_lastError = error;

            //get the control output
            float outputThrottle = p_out + i_out + d_out;

            return Mathf.Clamp(outputThrottle, 0, 1);
        }

        // Get the distance to the surface from the current part
        public float getSurfaceDistance()
        {
            altitude = FlightGlobals.getAltitudeAtPos(heightTransform.position);
            float distance = 0;

            RaycastHit hit;
            Ray cast = new Ray(heightTransform.position, Vector3.Normalize(vessel.mainBody.transform.position - heightTransform.position));

            if (Physics.Raycast(cast, out hit, maxHoverHeight+50, raycastMask))
            {

                float distanceToSurface = hit.distance;
                distance = distanceToSurface;

                //check if transform is under water
                if (vessel.mainBody.ocean)
                {
                    if (altitude < distanceToSurface)
                    {
                        distance = altitude;
                    }
                }
            }
            else
            {
                //when the main body has an ocean, make a sanity check
                if (vessel.mainBody.ocean)
                {
                    
                    //float transformheight = FlightGlobals.getAltitudeAtPos(heightTransform.position);
                    if (altitude <= (maxHoverHeight + 50))
                    {
                        distance = altitude;
                    }
                }
            }
            //visibleHeight = distance;
            return distance;
        }

        // Update the status if the engine
        private void updateStatus()
        {
            if (ignited)
            {
                engineState = EngineState.Running;
                status = engine_state_running;

                //check if in atmosphere
                bool inAtmosphere = ((vessel.mainBody.atmosphere) && (vessel.atmDensity > 0.0f));

                //check for water
                bool inWater = ((vessel.mainBody.ocean) && (FlightGlobals.getAltitudeAtPos(heightTransform.position) < 0.0f));

                if (engineModes[currentEngineMode].FlameOut)
                {
                    engineState = EngineState.Flameout;
                    status = engine_state_flameout;
                }
                else if (engineModes[currentEngineMode].NeedsAtmosphere && !inAtmosphere)
                {
                    engineState = EngineState.MissingAtmosphere;
                    status = engine_state_noAtmo;
                }
                else if (engineModes[currentEngineMode].NeedsOxygen && (!(vessel.mainBody.atmosphereContainsOxygen) || !inAtmosphere))
                {
                    engineState = EngineState.MissingOxygen;
                    status = engine_state_noOxy;
                }
                else if (engineModes[currentEngineMode].NeedsWater && !inWater)
                {
                    engineState = EngineState.MissingWater;
                    status = engine_state_noWater;
                }
                else if (hoverEnabled)
                {
                    //check if the engine is can hover
                    Vector3 downVed = Quaternion.LookRotation(Vector3.Normalize(vessel.mainBody.transform.position - heightTransform.position)) * Vector3.forward;

                    for (int i = 0; i < thrustTransforms.Length; i++)
                    {
                        if (Vector3.Angle(downVed, thrustTransforms[i].forward) > 60)
                        {
                            engineState = EngineState.OnHold;
                            status = engine_state_onHold;
                            break;
                        }
                    }

                }
            }
            else
            {
                engineState = EngineState.Inactive;
                status = engine_state_inative;
            }
            //isRunning = ignited && !flameout && inAtmosphere;
        }

        //----------------------------Interfaces------------------------

        //------IThrustProvider------

        /// <summary>
        /// Get the current thrust of the engine
        /// </summary>
        /// <returns>The current thrust</returns>
        public float GetCurrentThrust()
        {
            return engineThrust;
        }

        /// <summary>
        /// Get the type of the engine
        /// </summary>
        /// <returns>The type of the engine</returns>
        public EngineType GetEngineType()
        {
            return engineType;
        }

        /// <summary>
        /// Get the maximal thrust from this part
        /// </summary>
        /// <returns></returns>
        public float GetMaxThrust()
        {
            return maxThrust * (thrustLimiter / 100.0f);
        }

        /// <summary>
        /// Get The center of thrust of this part and its direction
        /// </summary>
        /// <param name="qry">The query for the center of thrust</param>
        public void OnCenterOfThrustQuery(CenterOfThrustQuery qry)
        {
            Vector3 position = Vector3.zero;
            Vector3 direction = Vector3.zero;

            //sum up all directions and positions
            for (int i = 0; i < thrustTransforms.Length; i++)
            {
                position += thrustTransforms[i].position - part.transform.position;
                direction += thrustTransforms[i].forward;
            }

            qry.pos = part.transform.position + (position / thrustTransforms.Length);
            qry.dir = direction.normalized;
            qry.thrust = maxThrust * (thrustLimiter / 100.0f);

        }

        //------IEngineStatus------

        /// <summary>
        /// Get whether the engine is operational
        /// </summary>
        public bool isOperational
        {
            get
            {
                return (engineState == EngineState.Running);
            }
        }

        /// <summary>
        /// Get the normalized output of the engine 
        /// </summary>
        public float normalizedOutput
        {
            get
            {
                if (engineModes.Count > currentEngineMode)
                {
                    return engineThrottle * engineModes[currentEngineMode].getThrustModifier();
                }
                return engineThrottle;
            }
        }

        /// <summary>
        /// Get the throttle setting of this engine
        /// </summary>
        public float throttleSetting
        {
            get
            {
                return engineThrottle;
            }
        }

        /// <summary>
        /// Get the name of this engine
        /// </summary>
        public string engineName
        {
            get
            {
                return EngineName;
            }
        }

        /// <summary>
        /// Get the altitude of this engine
        /// </summary>
        public float Altitude
        {
            get
            {
                return altitude;
            }
        }

        //------IModuleInfo------

        /// <summary>
        /// Get the title of this module
        /// </summary>
        /// <returns>The name of this module</returns>
        public string GetModuleTitle()
        {
            return Localizer.Format("#LOC_KERBETROTTER.engine.name");
        }

        /// <summary>
        /// Get the callback for the draw panel
        /// </summary>
        /// <returns></returns>
        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        /// <summary>
        /// Get the primary field string
        /// </summary>
        /// <returns>The primary field</returns>
        public string GetPrimaryField()
        {
            return null;
        }

        //------IResourceConsumer------

        /// <summary>
        /// Get the consumed resources by the engine
        /// </summary>
        /// <returns>List of consumed resources</returns>
        public List<PartResourceDefinition> GetConsumedResources()
        {
            List<PartResourceDefinition> definitions = new List<PartResourceDefinition>();
            if (engineModes.Count > currentEngineMode)
            {
                for (int i = 0; i < engineModes[currentEngineMode].propellants.Count; i++)
                {
                    definitions.Add(PartResourceLibrary.Instance.GetDefinition(engineModes[currentEngineMode].propellants[i].name));
                }
            }
            return definitions;
        }

        //----------------------------------------Enums--------------------------------
        /// <summary>
        /// The state of the engine. 
        /// </summary>
        public enum EngineState
        {
            Inactive,
            Running,
            Flameout,
            MissingAtmosphere,
            MissingOxygen,
            MissingWater,
            InWater,
            OnHold
        };

        /// <summary>
        /// Class holding the information about the needed propellants
        /// </summary>
        private class EnginePropellant
        {
            public EnginePropellant(string name, float amount)
            {
                this.name = name;
                this.amount = amount;
                ID = name.GetHashCode();
            }

            public string name;
            public int ID;
            public float amount;
        }
    }
}