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
namespace KerbetrotterTools {

    /// <summary>
    /// Extension class for a part
    /// </summary>
    public static class PartExtensions {

        /// <summary>
        /// Trigger an update of the action window of this part
        /// </summary>
        public static void updateActionWindow(this Part part) {
            if (part == null) { 
                return;
            }

            UIPartActionWindow[] windows = UnityEngine.Object.FindObjectsOfType<UIPartActionWindow>();
            for (int i = 0; i < windows.Length; i++) {
                if (windows[i].part == part) {
                    windows[i].ClearList();
                    windows[i].displayDirty = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Get the index of a partmodule in the part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <param name="module">The partmodule to check</param>
        /// <returns>The index of the partmodule</returns>
        public static int getModuleIndex(this Part part, PartModule module) {
            int numModules = part.Modules.Count;
            for (int i = 0; i < numModules; i++) {
                if ((module.GetInstanceID() == part.Modules[i].GetInstanceID()) && (module.moduleName == part.Modules[i].moduleName)) {
                    return i;
                }
            }
            return -1;
        }
    }
}
