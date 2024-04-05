namespace HexaEngine.Vulkan
{
    public struct QueueFamilyIndices
    {
        public uint? GraphicsFamily;
        public uint? PresentFamily;

        public bool IsComplete => GraphicsFamily is not null && PresentFamily is not null;
    }
}