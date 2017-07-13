using System.Collections.Generic;
using UnityEngine;

namespace KerbetrotterTools
{
    /// <summary>
    /// This class allows to switch some of the models from parts
    /// </summary>
    [KSPModule("Kerbetrotter Model Switch")]
    public class ModuleKerbetrotterMeshSwitch : PartModule
    {
        //The transforms to show and hide
        [KSPField(isPersistant = false)]
        public string transforms = string.Empty;

        [KSPField(isPersistant = false)]
        public string visibleNames = string.Empty;

        [KSPField(isPersistant = false)]
        public string switchName = "#LOC_KERBETROTTER.meshswitch.model";

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KERBETROTTER.meshswitch.model")]
        [UI_ChooseOption(scene = UI_Scene.Editor)]
        public int numModel = 0;
        private int oldModelNum = -1;

        BaseField modelBaseField;
        UI_ChooseOption modelUIChooser;

        //The list lights
        private List<TransformData> transformsData;

        /// <summary>
        /// Update the lights in the OnUpdate method
        /// </summary>
        public void Update()
        {
            //when the active model changes
            if (oldModelNum != numModel)
            {
                for (int i = 0; i < transformsData.Count; i++)
                {
                    if (i == numModel)
                    {
                        for (int j = 0; j < transformsData[i].transforms.Count; j++)
                        {
                            transformsData[i].transforms[j].gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < transformsData[i].transforms.Count; j++)
                        {
                            transformsData[i].transforms[j].gameObject.SetActive(false);
                        }
                    }
                }
                oldModelNum = numModel;
            }
        }

        /// <summary>
        /// Find the light transforms and lights to dim with the animation
        /// </summary>
        /// <param name="state">The startstate of the partmodule</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            modelBaseField = Fields["numModel"];
            modelUIChooser = (UI_ChooseOption)modelBaseField.uiControlEditor;
            modelBaseField.guiName = switchName;

            //find all the lights
            transformsData = new List<TransformData>();
            
            //Search in the named transforms for the lights
            if (transforms != string.Empty) 
            {
                string[] transformNames = transforms.Split(',');

                //find all the transforms
                List<Transform> transformsList = new List<Transform>();

                for (int i = 0; i < transformNames.Length; i++)
                {
                    TransformData transSetting = new TransformData();
                    //get all the transforms
                    transSetting.transforms.AddRange(part.FindModelTransforms(transformNames[i].Trim()));

                    transformsData.Add(transSetting);
                }

                string[] visible = visibleNames.Split(',');
                for (int i = 0; i < visible.Length; i++)
                {
                    visible[i] = visible[i].Trim();
                }

                if (visible.Length == transformNames.Length)
                {
                    modelUIChooser.options = visible;
                }
                else
                {
                    //set the changes for the modelchooser
                    modelUIChooser.options = transformNames;
                }

                //when there is only one model, we do not need to show the controls
                if (transformNames.Length < 2)
                {
                    modelBaseField.guiActive = false;
                    modelBaseField.guiActiveEditor = false;
                }
            }
            else
            {
                Debug.LogError("ModuleKerbetrotterMultiLight: No light transform defined!)");
            }
        }

        private class TransformData
        {
            public TransformData()
            {
                transforms = new List<Transform>();
            }
            public List<Transform> transforms;
        }
    }
}
