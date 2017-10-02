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
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterDockingAdjustment : PartModule
    {
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "#autoLOC_443418")]
        [UI_FloatEdit(scene = UI_Scene.All, minValue = 0.0f, maxValue = 1.0f)]
        public float dockPosition = -1.0f;
        private float oldPosition = -1.0f;

        //the targe position
        private float targetPosition = 0;

        [KSPField(isPersistant = false)]
        public float defaultposition = 0.2f;

        //the name of the animation
        [KSPField(isPersistant = false)]
        public string animationName = string.Empty;

        //the layer of the animation
        [KSPField(isPersistant = false)]
        public int layer = 2;

        [KSPField(isPersistant = false)]
        public float animSpeed = 0.5f;

        //the stored animation
        private Animation anim;

        //the module the animation is dependent on
        private ModuleAnimateGeneric extendAnimation = null;

        //the docking port 
        private ModuleDockingNode dockingNode = null;

        //The gui element
        BaseField positionBaseField;

        //flag that this is the first update
        private bool start = false;

        //----------------methods-----------------

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            start = true;

            //init
            if (dockPosition < 0.0)
            {
                dockPosition = defaultposition;
            }

            //find the animation
            anim = part.FindModelAnimators(animationName)[0];
            anim[animationName].layer = layer;

            //find the dependent modules
            extendAnimation = (ModuleAnimateGeneric)part.GetComponent("ModuleAnimateGeneric");
            dockingNode = (ModuleDockingNode)part.GetComponent("ModuleDockingNode");

            positionBaseField = Fields["dockPosition"];

            if (anim != null)
            {
                if (extendAnimation.animTime != 1.0f)
                {
                    targetPosition = defaultposition;
                }
                else
                {
                    targetPosition = dockPosition;
                }

                anim[animationName].speed = 0.0f;
                anim[animationName].normalizedTime = targetPosition;
                anim.Play(animationName);
            }
        }

        public void OnDestroy()
        {
            //free referenced modules
            extendAnimation = null;
            dockingNode = null;
            anim = null;
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
            {
                return;
            }

            if (anim == null)
            {
                return;
            }

            float currentAnimationTime = anim[animationName].normalizedTime;

            //when the extend animation is animating
            if ((extendAnimation != null) && (extendAnimation.aniState == ModuleAnimateGeneric.animationStates.MOVING))
            {
                float time = (extendAnimation.animTime - 0.5f) * 2;
                time = time < 0.0f ? 0.0f : time;
                time = time > 1.0f ? 1.0f : time;

                anim[animationName].normalizedTime = interpolate(targetPosition, defaultposition, time);
                anim[animationName].speed = 0.0f;
                if (!anim.IsPlaying(animationName))
                {
                    anim.Play(animationName);
                }
            }
            //the default state
            else if ((extendAnimation == null) || (extendAnimation.animTime == 1.0f))
            {
                if ((currentAnimationTime == 0.0f) && (anim[animationName].speed > 0.0f)) {
                    currentAnimationTime = 1.0f;
                }

                //stop the animation when the target position is reached
                if (anim[animationName].speed > 0)
                {
                    if ((currentAnimationTime >= targetPosition) && (!start))
                    {
                        anim.Stop(animationName);
                        anim[animationName].normalizedTime = targetPosition;
                    }
                }
                else
                {
                    if ((currentAnimationTime <= targetPosition) && (!start))
                    {
                        anim.Stop(animationName);
                        anim[animationName].normalizedTime = targetPosition;
                    }
                }

                //update the targeted position
                if (oldPosition != dockPosition)
                {

                    targetPosition = dockPosition;
                    oldPosition = dockPosition;

                    //start the animation when the destination is not yet reached
                    if (currentAnimationTime != targetPosition)
                    {
                        if (targetPosition > currentAnimationTime)
                        {
                            anim[animationName].speed = animSpeed;
                        }
                        else
                        {
                            anim[animationName].speed = -animSpeed;
                        }
                        //when the animation is not yet playing, play it
                        if (!anim.IsPlaying(animationName))
                        {
                            anim.Play(animationName);
                        }
                    }
                }
            }
            else if ((extendAnimation != null) && (extendAnimation.animTime == 0.0f))
            {
                if ((anim.IsPlaying(animationName)) && (!start))
                {
                    anim[animationName].normalizedTime = targetPosition;
                    anim.Stop(animationName); 
                }
            }
            start = false;
            updateVisibility();
        }

        //interpolate between two position
        private float interpolate(float a, float b, float value)
        {
            return (a * value) + (b * (1 - value));
        }

        //change the visibilit of the 
        private void updateVisibility()
        {
            if ((extendAnimation != null) && (dockingNode != null)) {
                if (dockingNode.fsm.CurrentState != null)
                {
                    if ((extendAnimation.animTime != 1.0) || (dockingNode.fsm.CurrentState == dockingNode.st_docked_dockee) || (dockingNode.fsm.CurrentState == dockingNode.st_docked_docker))
                    {
                        positionBaseField.guiActive = false;
                    }
                    else
                    {
                        positionBaseField.guiActive = true;
                    }
                }
            }
            else
            {
                positionBaseField.guiActive = true;
            }    
        }
    }
}
