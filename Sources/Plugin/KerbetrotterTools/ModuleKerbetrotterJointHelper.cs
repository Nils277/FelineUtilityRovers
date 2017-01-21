namespace KerbetrotterTools
{
    class ModuleKerbetrotterJointHelper : PartModule, IJointLockState
    {
        public bool IsJointUnlocked()
        {
            return true;
        }
    }
}
