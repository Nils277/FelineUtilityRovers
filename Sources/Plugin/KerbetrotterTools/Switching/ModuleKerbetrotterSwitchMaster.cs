using System.Collections.Generic;

namespace KerbetrotterTools
{
    class ModuleKerbetrotterSwitchMaster : PartModule
    {
        //The delegate to listen for switches
        public delegate void OnSwitch(string setup);

        //The list of listeners for switches
        private List<OnSwitch> mListener = new List<OnSwitch>();

        //The setup Group this switch belongs to
        [KSPField]
        public string setupGroup = "None";

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
