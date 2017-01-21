using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterTransparendPodHelper : PartModule
    {
        //the name of the additional internal
        [KSPField]
        public string hiddenOverlayTransformNames = string.Empty;

        //The names of the transforms
        private string[] trasformNames;

        //The list of found transforms to hide/show
        private List<Transform> transforms;


        /// <summary>
        /// The update method of this module. It checks the status of the IVA camera and disables or enables certain transforms
        /// </summary>
        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
            {
                return;
            }

            if ((transforms == null) && (hiddenOverlayTransformNames != null) && (hiddenOverlayTransformNames != string.Empty))
            {
                transforms = new List<Transform>();
                trasformNames = hiddenOverlayTransformNames.Split('|');
                int numNames = trasformNames.Length;
                for (int i = 0; i < numNames; i++)
                {
                    if (trasformNames[i] != string.Empty)
                    {
                        Transform newTransform = part.internalModel.FindModelTransform(trasformNames[i]);
                        if (newTransform != null)
                        {
                            transforms.Add(newTransform);
                        }
                    }
                }
            }
            


            if ((transforms != null) && (transforms.Count > 0)) {
                int numTransforms = transforms.Count;
                for (int i = 0; i < numTransforms; i++)
                {
                    if (transforms[i] == null)
                    {
                        transforms = null;
                        return;
                    }

                    if (HighLogic.LoadedSceneIsEditor)
                    {
                        if ((transforms[i].gameObject != null) && (transforms[i].gameObject.activeSelf))
                        {
                            transforms[i].gameObject.SetActive(false);
                        }
                    }
                    else if ((isStockOverlayActive()) && (transforms[i].gameObject != null) && (transforms[i].gameObject.activeSelf))
                    {
                        transforms[i].gameObject.SetActive(false);
                    }
                    else if ((!isStockOverlayActive()) && (transforms[i].gameObject != null) && (!transforms[i].gameObject.activeSelf))
                    {
                        transforms[i].gameObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all objects when this module is destroyed
        /// </summary>
        public void OnDestroy()
        {
            if ((transforms != null) && (transforms.Count > 0))
            {
                int numTransforms = transforms.Count;
                for (int i = 0; i < numTransforms; i++)
                {
                    transforms[i].gameObject.SetActive(true);
                }
                transforms = null;
            }
        }

        /// <summary>
        /// Chech if the stock overlay is visible
        /// </summary>
        /// <returns>True when stock overlay is active, else false</returns>
        private bool isStockOverlayActive()
        {
            if (Camera.allCameras == null)
            {
                return false;
            }

            int count = Camera.allCamerasCount;
            for (int i = 0; i < count; ++i)
            {
                //when the camera for the overly exists return true
                if (Camera.allCameras[i].name.Equals("InternalSpaceOverlay Host"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
