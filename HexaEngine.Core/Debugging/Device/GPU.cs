namespace HexaEngine.Core.Debugging.Device
{
    using Silk.NET.Core.Native;

    public struct GPU
    {
        public readonly string Description;
        public readonly uint VendorId;
        public readonly uint DeviceId;
        public readonly uint SubSysId;
        public readonly uint Revision;
        public readonly nuint DedicatedVideoMemory;
        public readonly nuint DedicatedSystemMemory;
        public readonly nuint SharedSystemMemory;
        public readonly Luid AdapterLUID;
        public readonly uint Flags;

        public GPU(string description, uint vendorId, uint deviceId, uint subSysId, uint revision, nuint dedicatedVideoMemory, nuint dedicatedSystemMemory, nuint sharedSystemMemory, Luid adapterLUID, uint flags)
        {
            Description = description;
            VendorId = vendorId;
            DeviceId = deviceId;
            SubSysId = subSysId;
            Revision = revision;
            DedicatedVideoMemory = dedicatedVideoMemory;
            DedicatedSystemMemory = dedicatedSystemMemory;
            SharedSystemMemory = sharedSystemMemory;
            AdapterLUID = adapterLUID;
            Flags = flags;
        }
    }
}