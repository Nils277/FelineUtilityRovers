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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    class KerbetrotterEngineVesselControl : VesselModule
    {
        //List if engines that hold terrain distance
        private List<ModuleKerbetrotterEngine> terrainEngines = new List<ModuleKerbetrotterEngine>();

        //List if engines that hold altitude
        private List<ModuleKerbetrotterEngine> altitudeEngines = new List<ModuleKerbetrotterEngine>();

        //List if engines switch from altitude mode into terrain mode
        private List<ModuleKerbetrotterEngine> switchToTerrainEngines = new List<ModuleKerbetrotterEngine>();

        //Event for an engine changing its state
        private EventData<ModuleKerbetrotterEngine, bool, bool> onEngineHoverChangeEvent;

        //Wheter the count of new engines has changed
        private bool terrainEnginesChanged = false;

        private bool altitudeEnginesChanged = false;

        private bool altitudeEnginesSwitched = false;



        protected override void OnAwake()
        {
            onEngineHoverChangeEvent = GameEvents.FindEvent<EventData<ModuleKerbetrotterEngine, bool, bool>>("onEngineHover");
            if (onEngineHoverChangeEvent != null)
            {
                onEngineHoverChangeEvent.Add(onEngineHoverChange);
            }
        }

        //the end of the module, remove from list of engines
        protected void OnDestroy()
        {
            if (onEngineHoverChangeEvent != null)
            {
                onEngineHoverChangeEvent.Remove(onEngineHoverChange);
            }
        }

        //event when an engine is added to the hover list
        private void onEngineHoverChange(ModuleKerbetrotterEngine engine, bool hoverActive, bool holdAltitude)
        {
            if (engine.vessel == vessel)
            {
                if (hoverActive)
                {
                    if (holdAltitude)
                    {
                        //see if the terrain engines contain this engine
                        if (terrainEngines.Contains(engine))
                        {
                            terrainEngines.Remove(engine);
                            terrainEnginesChanged = true;
                        }
                        //see if the engine needs to be added to the altitude engines
                        if (!altitudeEngines.Contains(engine))
                        {
                            altitudeEngines.Add(engine);
                            altitudeEnginesChanged = true;
                        }
                    }
                    else
                    {
                        bool fromAltitude = false;
                        //see if the engine needs to be added to the altitude engines
                        if (altitudeEngines.Contains(engine))
                        {
                            altitudeEngines.Remove(engine);
                            altitudeEnginesChanged = true;
                            fromAltitude = true;
                        }
                        //see if the terrain engines contain this engine
                        if (!terrainEngines.Contains(engine))
                        {
                            if (fromAltitude)
                            {
                                switchToTerrainEngines.Add(engine);
                                altitudeEnginesSwitched = true;
                            }
                            else
                            {
                                terrainEngines.Add(engine);
                                terrainEnginesChanged = true;
                            }
                        }
                    }
                }
                else
                {
                    //remove the engine from the terrain engines
                    if (terrainEngines.Contains(engine))
                    {
                        terrainEngines.Remove(engine);
                        terrainEnginesChanged = true;
                    }
                    //remove the engine from the altitude engines
                    if (altitudeEngines.Contains(engine))
                    {
                        altitudeEngines.Remove(engine);
                        altitudeEnginesChanged = true;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            //when engines for altitude mode were added or removed
            if (altitudeEnginesChanged)
            {
                updateAltitudeEngines();
                altitudeEnginesChanged = false;
            }
            //when engines switch to terrain mode
            if (altitudeEnginesSwitched)
            {
                updateSwitchToTerrainEngines();
                altitudeEnginesSwitched = false;
            }
            //when engines for terrain mode were added or removed
            if (terrainEnginesChanged)
            {
                updateTerrainEngines();
                terrainEnginesChanged = false;
            }

            //slightly fit the terrain engines to the distance of the terrain
            float avgAltitude = 0.0f;
            for (int i = 0; i < terrainEngines.Count; i++)
            {
                avgAltitude += terrainEngines[i].Altitude - terrainEngines[i].HeightOffset;
            }

            avgAltitude /= terrainEngines.Count;
            for (int i = 0; i < terrainEngines.Count; i++)
            {
                float correction = (terrainEngines[i].Altitude - avgAltitude - terrainEngines[i].HeightOffset) * 0.6666666f;
                terrainEngines[i].setAltitudeCorrection(correction);
            }
        }

        //Update the status of the engines hovering over the terrain when an engine was added or removed from the list of active engines
        private void updateSwitchToTerrainEngines()
        {
            //check whether there are engines that changed from altitude to terrain mode
            if (switchToTerrainEngines.Count > 0)
            {
                float maxHoverHeight = float.PositiveInfinity;
                float minHoverHeight = float.NegativeInfinity;

                for (int i = 0; i < switchToTerrainEngines.Count; i++)
                {
                    maxHoverHeight = Math.Min(maxHoverHeight, switchToTerrainEngines[i].maxHoverHeight);
                    minHoverHeight = Math.Max(minHoverHeight, switchToTerrainEngines[i].minHoverHeight);
                }
                double terrainHeight = FlightGlobals.ActiveVessel.pqsAltitude;
                bool valid = ((terrainHeight < 0) && ((vessel.mainBody.ocean))) ? FlightGlobals.ActiveVessel.altitude < (maxHoverHeight + 1)
                    : ((FlightGlobals.ActiveVessel.altitude - terrainHeight) < (maxHoverHeight + 1));

                if (valid)
                {
                    terrainEngines.AddRange(switchToTerrainEngines);
                    switchToTerrainEngines.Clear();
                    terrainEnginesChanged = true;
                }
                else
                {
                    double height = vessel.mainBody.ocean ? FlightGlobals.ActiveVessel.altitude : (FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.pqsAltitude);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.engine.hoverfail_2", maxHoverHeight), 2f, ScreenMessageStyle.UPPER_CENTER));

                    for (int i = 0; i < switchToTerrainEngines.Count; i++)
                    {
                        switchToTerrainEngines[i].setHoverMode(true, false);
                    }
                    altitudeEngines.AddRange(switchToTerrainEngines);
                    switchToTerrainEngines.Clear();
                }
            }
        }

        //Update the status of the engines hovering over the terrain when an engine was added or removed from the list of active engines
        private void updateTerrainEngines()
        {
            if (terrainEngines.Count > 0)
            {
                
                //get the center of the engines
                Vector3 center = new Vector3();
                float maxHoverHeight = float.PositiveInfinity;
                float minHoverHeight = float.NegativeInfinity;

                for (int i = 0; i < terrainEngines.Count; i++)
                {
                    center += terrainEngines[i].heightTransform.position;
                    maxHoverHeight = Math.Min(maxHoverHeight, terrainEngines[i].maxHoverHeight);
                    minHoverHeight = Math.Max(minHoverHeight, terrainEngines[i].minHoverHeight);
                }
                center /= terrainEngines.Count;

                double terrainHeight = FlightGlobals.ActiveVessel.pqsAltitude;
                bool valid = ((terrainHeight < 0) && ((vessel.mainBody.ocean)))? FlightGlobals.ActiveVessel.altitude < (maxHoverHeight + 1) 
                    : ((FlightGlobals.ActiveVessel.altitude - terrainHeight) < (maxHoverHeight + 1));

                //when we just switched to hovering, set the height
                if (!valid)
                {
                    double height = vessel.mainBody.ocean ? FlightGlobals.ActiveVessel.altitude : (FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.pqsAltitude);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.engine.hoverfail", maxHoverHeight), 2f, ScreenMessageStyle.UPPER_CENTER));
                    for (int i = 0; i < terrainEngines.Count; i++)
                    {
                        terrainEngines[i].setHoverEnabled(false, false);
                    }
                    terrainEngines.Clear();
                }
                else
                {
                    float heightSum = 0;
                    int heightNum = 0;

                    for (int i = 0; i < terrainEngines.Count; i++)
                    {
                        ModuleKerbetrotterEngine engine = terrainEngines[i];
                        float distance = engine.getSurfaceDistance();
                        if (!float.IsNaN(distance))
                        {
                            heightSum += distance;
                            heightNum++;
                        }
                    }
                    float height = Mathf.Clamp(heightSum / heightNum, minHoverHeight, maxHoverHeight);
                    for (int i = 0; i < terrainEngines.Count; i++)
                    {
                        ModuleKerbetrotterEngine engine = terrainEngines[i];
                        engine.setHoverHeight(height);
                    }
                }
            }
        }

        private void updateAltitudeEngines()
        {
            if (altitudeEngines.Count > 0)
            {
                //get the center of the engines
                Vector3 center = new Vector3();

                for (int i = 0; i < altitudeEngines.Count; i++)
                {
                    center += altitudeEngines[i].heightTransform.position;
                }
                center /= altitudeEngines.Count;

                float heightSum = 0;
                int heightNum = 0;

                for (int i = 0; i < altitudeEngines.Count; i++)
                {
                    ModuleKerbetrotterEngine engine = altitudeEngines[i];
                    float distance = engine.getAltitude();
                    if (!float.IsNaN(distance))
                    {
                        heightSum += distance;
                        heightNum++;
                    }
                }
                float height = heightSum / heightNum;
                for (int i = 0; i < altitudeEngines.Count; i++)
                {
                    altitudeEngines[i].setHoverHeight(height);
                }
                
            }
        }
    }
}
