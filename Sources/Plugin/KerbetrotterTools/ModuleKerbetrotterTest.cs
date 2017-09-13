namespace KerbetrotterTools
{
    class ModuleKerbetrotterTest : PartModule
    {
        //-----------------------------Engine Settigns----------------------------
        [KSPField(isPersistant = true, guiActive = true)]
        bool brakesEnabled = false;

        float brakeState = 0.0f;

        /// <summary>
        /// Action to set the breakes
        /// </summary>
        [KSPAction(actionGroup = KSPActionGroup.Brakes)]
        public void ToggleBrakes(KSPActionParam param)
        {
            brakesEnabled = (param.type == KSPActionType.Activate);
        }

        public void Update()
        {
            //brakeState = PhysicsGlobals
        }

    }
}
