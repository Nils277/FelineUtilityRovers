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
using KerbetrotterTools.Switching;
using System.Collections.Generic;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterSwitchMaster : ModuleKerbetrotterSwitch
    {
        //The delegate to listen for switches
        public delegate void OnSwitch(string setup);

        //The list of listeners for switches
        private List<OnSwitch> mListener = new List<OnSwitch>();

        //The setup Group this switch belongs to
        [KSPField]
        public string setupGroup = "None";

        /// <summary>
        /// Start method of the module
        /// </summary>
        /// <param name="state">The start state</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            mListener.Clear();
            List<ISwitchListener> modules = part.FindModulesImplementing<ISwitchListener>();
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i].getSetup() == setupGroup) {
                    mListener.Add(modules[i].onSwitch);
                }
            }
        }

        /// <summary>
        /// Update the listener about the new setup
        /// </summary>
        /// <param name="newSetup">The name of the new seup</param>
        protected void updateListener(string newSetup)
        {
            for (int i = 0; i< mListener.Count; i++)
            {
                mListener[i](newSetup);
            }
        }

        /// <summary>
        /// Clear all objects when this module is destroyed
        /// </summary>
        virtual public void OnDestroy()
        {
            mListener.Clear();
        }
    }
}
