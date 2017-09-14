using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    class KerbetrotterEngineVesselControl : VesselModule
    {
        //List if engines in the module
        private List<ModuleKerbetrotterEngine> engines = new List<ModuleKerbetrotterEngine>();

        //Event for an engine changing its state
        private EventData<ModuleKerbetrotterEngine, bool> onEngineHoverChangeEvent;

        //center of all the engines
        //private Vector3 center;

        //Wheter the count of new engines has changed
        private bool changed = false;

        protected override void OnAwake()
        {
            onEngineHoverChangeEvent = GameEvents.FindEvent<EventData<ModuleKerbetrotterEngine, bool>>("onEngineHover");
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
        private void onEngineHoverChange(ModuleKerbetrotterEngine engine, bool hoverActive)
        {
            if (engine.vessel == vessel)
            {

                if ((hoverActive) && !engines.Contains(engine))
                {
                    engines.Add(engine);
                    changed = true;
                    return;
                }

                if ((!hoverActive) && engines.Contains(engine))
                {
                    engines.Remove(engine);
                    changed = true;
                    return;
                }
            }
        }

        void FixedUpdate()
        {
            //when engines were added or removed
            if (changed)
            {
                updateEngines();
                changed = false;
            }

            float avgAltitude = 0.0f;
            for (int i = 0; i < engines.Count; i++)
            {
                avgAltitude += engines[i].Altitude - engines[i].HeightOffset;
            }
            avgAltitude /= engines.Count;
            for (int i = 0; i < engines.Count; i++)
            {
                float correction = (engines[i].Altitude - avgAltitude - engines[i].HeightOffset) * 0.6666666f;
                engines[i].setAltitudeCorrection(correction);
            }
        }

        //Update the status of the engines when an engine was added or removed from the list of active hovering engines
        private void updateEngines()
        {
            //distances.Clear();
            if (engines.Count > 0)
            {
                //get the center of the engines
                Vector3 center = new Vector3();
                float maxHoverHeight = float.PositiveInfinity;
                float minHoverHeight = float.NegativeInfinity;

                for (int i = 0; i < engines.Count; i++)
                {
                    center += engines[i].heightTransform.position;
                    maxHoverHeight = Math.Min(maxHoverHeight, engines[i].maxHoverHeight);
                    minHoverHeight = Math.Max(minHoverHeight, engines[i].minHoverHeight);
                }
                center /= engines.Count;

                double terrainHeight = FlightGlobals.ActiveVessel.pqsAltitude;
                bool valid = ((terrainHeight < 0) && ((vessel.mainBody.ocean)))? FlightGlobals.ActiveVessel.altitude < (maxHoverHeight + 1) 
                    : ((FlightGlobals.ActiveVessel.altitude - terrainHeight) < (maxHoverHeight + 1));

                //when we just switched to hovering, set the height
                if (!valid)
                {
                    double height = vessel.mainBody.ocean ? FlightGlobals.ActiveVessel.altitude : (FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.pqsAltitude);
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_KERBETROTTER.engine.hoverfail", maxHoverHeight), 2f, ScreenMessageStyle.UPPER_CENTER));
                    for (int i = 0; i < engines.Count; i++)
                    {
                        engines[i].setHoverEnabled(false, false);
                    }
                    engines.Clear();
                }
                else
                {
                    float heightSum = 0;
                    int heightNum = 0;

                    for (int i = 0; i < engines.Count; i++)
                    {
                        ModuleKerbetrotterEngine engine = engines[i];
                        float distance = engine.getSurfaceDistance();
                        if (!float.IsNaN(distance))
                        {
                            heightSum += distance;
                            heightNum++;
                        }
                    }
                    float height = Mathf.Clamp(heightSum / heightNum, minHoverHeight, maxHoverHeight);
                    for (int i = 0; i < engines.Count; i++)
                    {
                        ModuleKerbetrotterEngine engine = engines[i];
                        engine.setHoverHeight(height);
                    }
                }
            }
        }
    }
}
