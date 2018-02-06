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
using KSP.Localization;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterEngine : PartModule, IModuleInfo
    {
        //-----------------------------Engine Settigns----------------------------

        /// <summary>
        /// The transform of the thrust vector to control
        /// </summary>
        [KSPField]
        public string thrustVectorTransformName = "thrustTransform";

        /// <summary>
        /// The name of the transform to check the distance to the ground
        /// </summary>
        [KSPField]
        public string heightTransformName = "heigthTransform";

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
        /// The thrust limiter setting
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#autoLOC_6001363"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.5f, affectSymCounterparts = UI_Scene.All)]
        public float thrustLimiter = 100.0f;
        
        /// <summary>
        /// The height offset for the engine to balance it out
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KERBETROTTER.engine.offset"), UI_FloatRange(minValue = -2, maxValue = 2f, stepIncrement = 0.01f, affectSymCounterparts = UI_Scene.All)]
        public float heightOffset = 0.0f;

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
        /// The current hover mode
        /// </summary>
        [KSPField(guiName = "#LOC_KERBETROTTER.engine.mode.title", guiActive = true)]
        public string mode = "#LOC_KERBETROTTER.engine.mode.terrain";

        /// <summary>
        /// Whether the engine mode is enabled or not
        /// </summary>
        [KSPField(guiName = "#LOC_KERBETROTTER.engine.hovering", guiActive = true)]
        public string hovering = "";


        //-----------------------------Saved engine state--------------------

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
        /// Whether the user has a custom PID profile
        /// </summary>
        [KSPField(isPersistant = true)]
        private bool customPID = false;

        /// <summary>
        /// saves the mode of the engine
        /// </summary>
        [KSPField(isPersistant = true)]
        private bool holdAltitude = false;

        //----------------------------Private members------------------------

        //the thrust transformt to change
        private Transform thrustTransform;

        //The transform the get the height
        public Transform heightTransform;

        //The local rotation from the height transform Stored for convenience purposes
        private Quaternion heightLocalRotation;

        //saves whether the height should be changed
        private float heightChange = 0;

        // the profiles for different planets
        private List<KerbetrotterPIDProfile> profiles = new List<KerbetrotterPIDProfile>();

        //sets whether the the advanced controls are visible
        private bool showAdvancedControls = false;

        //the altitude of the engine
        private float altitude;

        //The mask for the ray cast to the terrain
        private int raycastMask = (1 << 28 | 1 << 15);

        //The correction for the altitude
        private float alitudeCorrection;

        //the current profile
        private int currentProfile = 0;

        //the engine that is controlled
        private ModuleEngines primaryEngine;

        //the engine that is controlled
        private ModuleEngines secondaryEngine;

        //the engine that is controlled
        private MultiModeEngine engineMode;

        //when the engine is on hold
        private bool onHold = false;

        private bool isIgnited = false;


        private float lastPercentage = 100.0f;

        //-----------------------------Strings--------------------------

        private static string engine_hover_enabled = Localizer.Format("#autoLOC_900889");
        private static string engine_hover_disabled = Localizer.Format("#autoLOC_900890");
        private static string engine_state_onHold = Localizer.Format("#LOC_KERBETROTTER.engine.state.onHold");
        private static string engine_mode_terrain = Localizer.Format("#LOC_KERBETROTTER.engine.mode.terrain");
        private static string engine_mode_altitude = Localizer.Format("#LOC_KERBETROTTER.engine.mode.altitude");

        //----------------------------Interaction-----------------------

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
        /// Action to toggle the hover effect
        /// </summary>
        [KSPEvent(guiName = "#LOC_KERBETROTTER.engine.mode.switch", guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ToggleHoverMode()
        {
            holdAltitude = !holdAltitude;
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
                        ((ModuleKerbetrotterEngine)module).setHoverMode(holdAltitude, true);
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
        [KSPEvent(guiName = "#LOC_KERBETROTTER.engine.advanced.show", guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void ToogleControls()
        {
            showAdvancedControls = !showAdvancedControls;
            updateAndvancedControlVisibility();
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
        /// Action to toggle the hover effect
        /// </summary>
        [KSPAction("#LOC_KERBETROTTER.engine.mode.switch")]
        public void ToggleHoverModeAction(KSPActionParam param)
        {
            holdAltitude = !holdAltitude;
            updateHoverStatus();
        }

        //----------------------------Life Cycle------------------------

        /// <summary>
        /// Get the switchable resources on load to allow the partInfo to be populated
        /// </summary>
        /// <param name="partNode"> The config node for this partmodule</param>
        public override void OnLoad(ConfigNode partNode)
        {
            base.OnLoad(partNode);
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
            return Localizer.Format("#LOC_KERBETROTTER.engine.desc");
        }

        /// <summary>
        /// Start up the module and initialize it
        /// </summary>
        /// <param name="state">The start state of the module</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //find the MultiModeEngine if available
            for (int i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i] is MultiModeEngine)
                {
                    engineMode = (MultiModeEngine)part.Modules[i];
                    break;
                }
            }

            //find all the engines
            for (int i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i] is ModuleEngines || part.Modules[i] is ModuleEnginesFX)
                {
                    if (engineMode == null)
                    {
                        primaryEngine = (ModuleEngines)part.Modules[i];
                        break;
                    }
                    else if (engineMode.primaryEngineID == ((ModuleEngines)part.Modules[i]).engineID)
                    {
                        primaryEngine = (ModuleEngines)part.Modules[i];
                    }
                    else
                    {
                        secondaryEngine = (ModuleEngines)part.Modules[i];
                    }
                }
            }

            primaryEngine.Fields["thrustPercentage"].guiActive = false;
            primaryEngine.Fields["thrustPercentage"].guiActiveEditor = false;
            if (primaryEngine is ModuleKerbetrotterEngineFX)
            {
                primaryEngine.Fields["thrustLimiter"].guiActive = false;
                primaryEngine.Fields["thrustLimiter"].guiActiveEditor = false;
                ((ModuleKerbetrotterEngineFX)primaryEngine).setControlled(true);
            }

            if (secondaryEngine != null)
            {
                secondaryEngine.Fields["thrustPercentage"].guiActive = false;
                secondaryEngine.Fields["thrustPercentage"].guiActiveEditor = false;
                if (secondaryEngine is ModuleKerbetrotterEngineFX)
                {
                    secondaryEngine.Fields["thrustLimiter"].guiActive = false;
                    secondaryEngine.Fields["thrustLimiter"].guiActiveEditor = false;
                    ((ModuleKerbetrotterEngineFX)secondaryEngine).setControlled(true);
                }
            }

            thrustTransform = part.FindModelTransform(thrustVectorTransformName.Trim());
            heightTransform = part.FindModelTransform(heightTransformName.Trim());
            heightLocalRotation = heightTransform.localRotation;

            updateHoverStatus();
            LoadPIDProfiles(part.partInfo.partConfig);
            updateAndvancedControlVisibility();

            lastPercentage = thrustLimiter;

            if ((!customPID))
            {
                ResetProfile();
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
        /// Update for every physicss tic
        /// </summary>
        public virtual void FixedUpdate()
        {
            ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;

            if (!HighLogic.LoadedSceneIsFlight)
            {
                if (HighLogic.LoadedSceneIsEditor)
                {

                    engine.thrustPercentage = thrustLimiter;
                }

                updateStatus();
                return;
            }

            //the final throttle setting for the engine
            float finalThrottle = 0.0f;

            //get the currently running engine
            //ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;

            if (engine.isActiveAndEnabled && engine.EngineIgnited && !engine.flameout)
            {
                //when hovering point the transform to the surface
                if (hoverEnabled)
                {
                    //update the set height
                    if (heightSet)
                    {
                        hoverHeight -= heightChange * TimeWarp.fixedDeltaTime * 2;
                        if (!holdAltitude)
                        {
                            if (hoverHeight > maxHoverHeight)
                            {
                                hoverHeight = maxHoverHeight;
                            }
                            else if (hoverHeight < minHoverHeight)
                            {
                                hoverHeight = minHoverHeight;
                            }
                        }
                    }

                    //when the desired height is set, hover
                    if (heightSet)
                    {
                        float height = holdAltitude? getAltitude() :getSurfaceDistance();

                        if (!float.IsNaN(height))
                        {
                            finalThrottle = PID(height);
                        }
                    }
                    else
                    {
                        finalThrottle = 0.0f;
                    }
                    if (onHold)
                    {
                        finalThrottle = 0.0f;
                    }

                    if (engine != null)
                    {
                        float throttle = vessel.ctrlState.mainThrottle;
                        float wantedThrottle = finalThrottle * thrustLimiter;

                        //make the throttle of the engine independent of the trottle setting
                        if ((throttle * 100.0f < wantedThrottle) || (throttle < 0.01))
                        {
                            engine.thrustPercentage = 100;
                            engine.throttleMin = Mathf.Clamp((throttle - (wantedThrottle / 100.0f)) / (throttle - 1), 0.0f, 1.0f);
                        }
                        else
                        {
                            engine.thrustPercentage = Mathf.Clamp(wantedThrottle / throttle, 0.0f, 100.0f);
                            engine.throttleMin = 0;
                        }
                    }
                }
                else if (lastPercentage != thrustLimiter)
                {
                    engine.thrustPercentage = thrustLimiter;
                    lastPercentage = thrustLimiter;
                }
            }
            else if (engine.flameout)
            {
                engine.thrustPercentage = 0;
                engine.minThrust = 0;
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
                if (hoverEnabled && heightSet)
                {
                    heightChange = vessel.ctrlState.Y;
                }
            }

            //get the currently running engine
            ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;

            if (isIgnited != engine.EngineIgnited)
            {
                updateHoverStatus();
                isIgnited = engine.EngineIgnited;
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
        /// Get whether the engine is running
        /// </summary>
        public bool isRunning
        {
            get
            {
                //get the currently running engine
                ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;
                return engine.isActiveAndEnabled & engine.EngineIgnited && !engine.flameout;
            }
        }

        /// <summary>
        /// Get the throttle setting
        /// </summary>
        public float throttleSetting
        {
            get
            {
                //get the currently running engine
                ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;
                return engine.currentThrottle;
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
        /// Set whether the engine should hover
        /// </summary>
        /// <param name="hover">True when hover should be enabled, else false</param>
        public void setHoverEnabled(bool hover, bool fireEvent)
        {
            hoverEnabled = hover;
            updateHoverStatus(fireEvent);
        }

        /// <summary>
        /// Set whether the engine should hold the altitude
        /// </summary>
        /// <param name="hover">True when hover should hold the altitude, else false</param>
        public void setHoverMode(bool holdAltitude, bool fireEvent)
        {
            this.holdAltitude = holdAltitude;
            updateHoverStatus(fireEvent);
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
            Debug.Log("[HOVER] Height Set: " + height + " Enabled: " + hoverEnabled);
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

        //update the hover status, sets whether an event should be fired or not
        private void updateHoverStatus(bool fireEvent)
        {
            //get the currently running engine
            ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;

            if (!hoverEnabled)
            {
                primaryEngine.throttleMin = 0.0f;
                primaryEngine.thrustPercentage = thrustLimiter;
                if (secondaryEngine != null)
                {
                    secondaryEngine.throttleMin = 0.0f;
                    secondaryEngine.thrustPercentage = thrustLimiter;
                }
            }

            if ((!hoverEnabled) || (!engine.EngineIgnited))
            {
                PID_errorSum = 0;
                PID_lastError = 0;
                alitudeCorrection = 0;
                heightSet = false;
            }
            if (fireEvent) {
                KerbetrotterEngineHoverEvent.onEngineHover.Fire(this, hoverEnabled & engine.EngineIgnited, holdAltitude);
            }
            hovering = hoverEnabled ? engine_hover_enabled : engine_hover_disabled;
            Events["ToggleHover"].guiName = hoverEnabled? Localizer.Format("#LOC_KERBETROTTER.engine.mode.switch_free") : Localizer.Format("#LOC_KERBETROTTER.engine.mode.switch_hover");
            mode = holdAltitude ? engine_mode_altitude : engine_mode_terrain;
        }

        //update the status of the hover mode
        private void updateHoverStatus()
        {
            updateHoverStatus(true);
        }

        //update the visibility of the advanced controls
        private void updateAndvancedControlVisibility()
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
        private void LoadPIDProfilesInternal(ConfigNode node)
        {
            ConfigNode[] propConfig = node.GetNodes("PID-PROFILE");
            for (int i = 0; i < propConfig.Length; i++)
            {
                KerbetrotterPIDProfile profile = new KerbetrotterPIDProfile(propConfig[i]);
                profiles.Add(profile);
            }
        }

        // PID controller to update the height of the engines
        private float PID(float height)
        {
            float error = hoverHeight - height + heightOffset;
            if (!holdAltitude)
            {
                error -= alitudeCorrection;
            }

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
        public float getAltitude()
        {
            return FlightGlobals.getAltitudeAtPos(heightTransform.position);
        }

        // Get the distance to the surface from the current part
        public float getSurfaceDistance()
        {
            altitude = FlightGlobals.getAltitudeAtPos(heightTransform.position);

            if (holdAltitude)
            {
                return altitude;
            }

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
            //get the currently running engine
            ModuleEngines engine = (engineMode == null || engineMode.runningPrimary) ? primaryEngine : secondaryEngine;
            if (engine.EngineIgnited)
            {
                if (hoverEnabled)
                {
                    //check if the engine can hover
                    Vector3 downVed = Quaternion.LookRotation(Vector3.Normalize(vessel.mainBody.transform.position - heightTransform.position)) * Vector3.forward;

                    if (Vector3.Angle(downVed, thrustTransform.forward) > 60)
                    {
                        engine.status = engine_state_onHold;
                        onHold = true;
                    }
                    else
                    {
                        onHold = false;
                    }
                }
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
    }
}