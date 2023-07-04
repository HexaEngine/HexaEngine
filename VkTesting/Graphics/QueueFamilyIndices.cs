namespace HexaEngine.Vulkan
{
    public struct QueueFamilyIndices
    {
        public uint? GraphicsFamily;
        public uint? PresentFamily;

        public readonly bool IsComplete => GraphicsFamily is not null && PresentFamily is not null;
    }
}