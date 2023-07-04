namespace VkTesting.Graphics
{
    using Silk.NET.Vulkan;

    public enum QueueType
    {
        Graphics,
        Compute,
        Copy,
        Bundle,
        VideoEncode,
        VideoDecode,
    }

    public class VulkanCommandQueue
    {
        private Queue queue;
        private QueueType queueType;

        public VulkanCommandQueue(Queue queue, QueueType queueType)
        {
            this.queue = queue;
            this.queueType = queueType;
        }

        internal Queue Queue => queue;

        public QueueType QueueType => queueType;

        public void ExecuteCommandLists(VulkanCommandList[] lists)
        {
        }

        public void ExecuteCommandLists(ReadOnlySpan<VulkanCommandList> lists)
        {
        }
    }
}