namespace TestApp
{
    using HexaEngine.Lights;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            SpatialAllocator allocator = new(new(8192), 8);
            var node4 = allocator.Alloc(new(512));
            var node5 = allocator.Alloc(new(512));
            var node6 = allocator.Alloc(new(512));
            var node7 = allocator.Alloc(new(512));
            var node8 = allocator.Alloc(new(512));
            var node9 = allocator.Alloc(new(512));
            /*
            var node0 = allocator.Alloc(new(1024));
            var node1 = allocator.Alloc(new(1024));
            var node2 = allocator.Alloc(new(1024));
            var node3 = allocator.Alloc(new(1024));

            var node4 = allocator.Alloc(new(512));
            var node5 = allocator.Alloc(new(512));
            var node6 = allocator.Alloc(new(512));
            var node7 = allocator.Alloc(new(512));
            var node8 = allocator.Alloc(new(512));
            var node9 = allocator.Alloc(new(512));
            var node10 = allocator.Alloc(new(512));
            var node11 = allocator.Alloc(new(512));

            var node12 = allocator.Alloc(new(128));
            var node13 = allocator.Alloc(new(128));
            var node14 = allocator.Alloc(new(128));
            var node15 = allocator.Alloc(new(128));
            var node16 = allocator.Alloc(new(128));
            var node17 = allocator.Alloc(new(128));
            var node18 = allocator.Alloc(new(128));
            var node19 = allocator.Alloc(new(128));

            var node20 = allocator.Alloc(new(128));
            var node21 = allocator.Alloc(new(128));
            var node22 = allocator.Alloc(new(128));
            var node23 = allocator.Alloc(new(128));
            var node24 = allocator.Alloc(new(128));
            var node25 = allocator.Alloc(new(128));
            var node26 = allocator.Alloc(new(128));
            var node27 = allocator.Alloc(new(128));
            */
        }
    }
}