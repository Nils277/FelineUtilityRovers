using UnityEngine;

namespace KerbetrotterTools
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class KerbetrotterEngineHoverEvent : MonoBehaviour
    {
        public static EventData<ModuleKerbetrotterEngine, bool> onEngineHover;


        /// <summary>
        /// When the class awakes it inits all the filters it found for the KerbatrotterTools
        /// </summary>
        private void Awake()
        {
            onEngineHover = new EventData<ModuleKerbetrotterEngine, bool>("onEngineHover");
        }
    }
}
