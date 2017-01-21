using UnityEngine;
using System.Collections.Generic;

namespace KerbetrotterTools
{
    /// <summary>
    /// This Class allows to toggle the visibilty of a transform (incluive subtransforms) This also affects the colliders
    /// </summary>
    class ModuleKerbetrotterMeshToggle : PartModule
    {
        
        [KSPField]//the names of the transforms
        public string transformNames = string.Empty;

        [KSPField]//Text to show to hide a mesh
        public string showMeshString = "Show Mesh";

        [KSPField]//Text to show to show a mesh
        public string hideMeshString = "Hide Mesh";

        [KSPField]//Whether the toggle is available in flight
        public bool availableInFlight = true; 

        [KSPField]//Whether the toggle is available in editor
        public bool availableInEditor = true; 

        //--------------persistent states----------------
        [KSPField(isPersistant = true)]
        public bool transformsVisible = true;

        //the list of models
        List<Transform> transforms;

        //saves whether the visibility has been updated yet or not
        public bool initialized = false;

        /// <summary>
        /// Find the transforms that can be toggled
        /// </summary>
        /// <param name="state">the state of the part</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            string[] transformGroupNames = transformNames.Split(',');
            transforms = new List<Transform>();

            //----------------------------------------------------------
            //create the list of transforms to be made toggleble
            //----------------------------------------------------------
            for (int k = 0; k < transformGroupNames.Length; k++)
            {
                transforms.AddRange(part.FindModelTransforms(transformGroupNames[k].Trim()));
            }

            updateMeshes();
        }

        /// <summary>
        /// Update the visibility of the GUI
        /// </summary>
        private void updateGUI()
        {
            //when there is only one model, we do not need to show the controls
            if (transforms.Count < 1)
            {
                Events["toggleMesh"].active = false;
            }
            //when there are two models make the controls appear as a switch between two parts
            else
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    Events["toggleMesh"].active = availableInEditor;
                }
                else if (HighLogic.LoadedSceneIsFlight)
                {
                    Events["toggleMesh"].active = availableInFlight;
                }
                else
                {
                    Events["toggleMesh"].active = false;
                    return;
                }
                
                if (transformsVisible)
                {
                    Events["toggleMesh"].guiName = hideMeshString;
                }
                else
                {
                    Events["toggleMesh"].guiName = showMeshString;
                }

            }
        }

        /// <summary>
        /// The update method of the partmodule
        /// </summary>
        /*public void Update()
        {
            if (!initialized)
            {
                updateMeshes();
                initialized = true;
            }
        }*/

        /// <summary>
        /// Event that toggles the visibility of the mesh
        /// </summary>
        [KSPEvent(name = "toggleMesh", guiName = "Toggle Mesh", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = true)]
        public void toggleMesh()
        {
            transformsVisible = !transformsVisible;
            updateMeshes();
        }

        /// <summary>
        /// Update the meshes of the part
        /// </summary>
        private void updateMeshes()
        {
            int numTransforms = transforms.Count;
            for (int i = 0; i < numTransforms; i++)
            {
                transforms[i].gameObject.SetActive(transformsVisible);
            }
            updateGUI();
        }
    }
}
