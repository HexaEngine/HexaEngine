namespace HexaEngine.Physics
{
    using HexaEngine.Core;
    using MagicPhysX;
    using System.Runtime.InteropServices;

    public unsafe class ControllerFilterCallbacks : DisposableBase
    {
        internal PxControllerFilterCallback* controllerFilterCallback;

        public ControllerFilterCallbacks(Func<PxController, PxController, bool> func)
        {
            controllerFilterCallback = AllocT<PxControllerFilterCallback>();
            Memset(controllerFilterCallback, 0, sizeof(PxControllerFilterCallback));
            void** vTable = AllocArray(1);
            vTable[0] = (void*)Marshal.GetFunctionPointerForDelegate(func);
            controllerFilterCallback->vtable_ = vTable;
        }

        protected override void DisposeCore()
        {
            if (controllerFilterCallback != null)
            {
                if (controllerFilterCallback->vtable_ != null)
                {
                    Free(controllerFilterCallback->vtable_);
                    controllerFilterCallback->vtable_ = null;
                }

                Free(controllerFilterCallback);
                controllerFilterCallback = null;
            }
        }
    }
}