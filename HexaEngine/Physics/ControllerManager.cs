namespace HexaEngine.Physics
{
    using MagicPhysX;

    public unsafe class ControllerManager
    {
        private readonly object _lock = new();
        private PxControllerManager* manager;

        public ControllerManager(PhysicsSystem system)
        {
            manager = NativeMethods.phys_PxCreateControllerManager(system.PxScene, false);
        }

        public PxController* CreateController(PxCapsuleControllerDesc desc)
        {
            lock (_lock)
            {
                return manager->CreateControllerMut((PxControllerDesc*)&desc);
            }
        }

        public PxController* CreateController(PxBoxControllerDesc desc)
        {
            lock (_lock)
            {
                return manager->CreateControllerMut((PxControllerDesc*)&desc);
            }
        }

        internal void Dispose()
        {
            lock (_lock)
            {
                if (manager != null)
                {
                    manager->ReleaseMut();
                    manager = null;
                }
            }
        }
    }
}