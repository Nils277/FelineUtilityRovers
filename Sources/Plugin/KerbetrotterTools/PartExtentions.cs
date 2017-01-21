using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KerbetrotterTools
{
    public static class PartExtensions
    {
        /// <summary>
        /// Field holding the list of window fields. To not have to init at every request
        /// </summary>
        private static FieldInfo windowListField;

        /// <summary>
        /// Find the UIPartActionWindow for a part. Usually this is useful just to mark it as dirty.
        /// </summary>
        public static UIPartActionWindow FindActionWindow(this Part part)
        {
            if (part == null)
            { 
                return null;
            }

            // We need to do quite a bit of reflection to dig the thing out. 
            // We could just use Object.Find, but that requires hitting a heap more objects.
            UIPartActionController controller = UIPartActionController.Instance;
            if (controller == null)
            {
                return null;
            }
                
            //initialize the window list
            if (windowListField == null)
            {
                Type controllerType = typeof(UIPartActionController);

                foreach (FieldInfo info in controllerType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (info.FieldType == typeof(List<UIPartActionWindow>))
                    {
                        windowListField = info;
                        break;
                    }
                }

                if (windowListField == null)
                {
                    Debug.LogWarning("*PartExtentions* Unable to find UIPartActionWindow list");
                    return null;
                }
            }

            //get the list if UIPartActionWindows
            List<UIPartActionWindow> uiPartActionWindows = (List<UIPartActionWindow>)windowListField.GetValue(controller);
            if (uiPartActionWindows == null)
            { 
                return null;
            }

            //find and return the right partactionwindow
            for (int i = uiPartActionWindows.Count-1; i >= 0 ; i--)
            {
                if ((uiPartActionWindows[i] != null) && (uiPartActionWindows[i].part == part))
                {
                    return uiPartActionWindows[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the index of a partmodule in the part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <param name="module">The partmodule to check</param>
        /// <returns>The index of the partmodule</returns>
        public static int getModuleIndex(this Part part, PartModule module)
        {
            int numModules = part.Modules.Count;
            for (int i = 0; i < numModules; i++)
            {
                if ((module.GetInstanceID() == part.Modules[i].GetInstanceID()) && (module.moduleName == part.Modules[i].moduleName))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
