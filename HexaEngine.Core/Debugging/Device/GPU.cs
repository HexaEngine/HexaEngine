namespace HexaEngine.Core.Debugging.Device
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Graphics Processing Unit (GPU) with its associated properties.
    /// </summary>
    public readonly struct GPU : IEquatable<GPU>
    {
        /// <summary>
        /// Gets the description of the GPU.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Gets the vendor ID of the GPU.
        /// </summary>
        public readonly uint VendorId;

        /// <summary>
        /// Gets the device ID of the GPU.
        /// </summary>
        public readonly uint DeviceId;

        /// <summary>
        /// Gets the subsystem ID of the GPU.
        /// </summary>
        public readonly uint SubSysId;

        /// <summary>
        /// Gets the revision of the GPU.
        /// </summary>
        public readonly uint Revision;

        /// <summary>
        /// Gets the amount of dedicated video memory in bytes.
        /// </summary>
        public readonly nuint DedicatedVideoMemory;

        /// <summary>
        /// Gets the amount of dedicated system memory in bytes.
        /// </summary>
        public readonly nuint DedicatedSystemMemory;

        /// <summary>
        /// Gets the amount of shared system memory in bytes.
        /// </summary>
        public readonly nuint SharedSystemMemory;

        /// <summary>
        /// Gets the locally unique identifier (LUID) associated with the GPU adapter.
        /// </summary>
        public readonly LUID AdapterLUID;

        /// <summary>
        /// Gets the flags or attributes of the GPU.
        /// </summary>
        public readonly uint Flags;

        /// <summary>
        /// Initializes a new instance of the GPU struct with the specified properties.
        /// </summary>
        /// <param name="description">The description of the GPU.</param>
        /// <param name="vendorId">The vendor ID of the GPU.</param>
        /// <param name="deviceId">The device ID of the GPU.</param>
        /// <param name="subSysId">The subsystem ID of the GPU.</param>
        /// <param name="revision">The revision of the GPU.</param>
        /// <param name="dedicatedVideoMemory">The amount of dedicated video memory in bytes.</param>
        /// <param name="dedicatedSystemMemory">The amount of dedicated system memory in bytes.</param>
        /// <param name="sharedSystemMemory">The amount of shared system memory in bytes.</param>
        /// <param name="adapterLUID">The LUID associated with the GPU adapter.</param>
        /// <param name="flags">The flags or attributes of the GPU.</param>
        public GPU(string description, uint vendorId, uint deviceId, uint subSysId, uint revision, nuint dedicatedVideoMemory, nuint dedicatedSystemMemory, nuint sharedSystemMemory, LUID adapterLUID, uint flags)
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

        public override bool Equals(object? obj)
        {
            return obj is GPU gPU && Equals(gPU);
        }

        public bool Equals(GPU other)
        {
            return Description == other.Description &&
                   VendorId == other.VendorId &&
                   DeviceId == other.DeviceId &&
                   SubSysId == other.SubSysId &&
                   Revision == other.Revision &&
                   DedicatedVideoMemory.Equals(other.DedicatedVideoMemory) &&
                   DedicatedSystemMemory.Equals(other.DedicatedSystemMemory) &&
                   SharedSystemMemory.Equals(other.SharedSystemMemory) &&
                   EqualityComparer<LUID>.Default.Equals(AdapterLUID, other.AdapterLUID) &&
                   Flags == other.Flags;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Description);
            hash.Add(VendorId);
            hash.Add(DeviceId);
            hash.Add(SubSysId);
            hash.Add(Revision);
            hash.Add(DedicatedVideoMemory);
            hash.Add(DedicatedSystemMemory);
            hash.Add(SharedSystemMemory);
            hash.Add(AdapterLUID);
            hash.Add(Flags);
            return hash.ToHashCode();
        }

        public static bool operator ==(GPU left, GPU right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GPU left, GPU right)
        {
            return !(left == right);
        }
    }
}