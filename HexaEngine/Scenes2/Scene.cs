namespace HexaEngine.Scenes2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public unsafe struct Vector<T> where T : unmanaged
    {
        private T** arrays;
        private int length;
        private int capacity;

        public void Resize(int length)
        {
        }

        public void Trim(int length)
        {
        }

        public void Clear()
        {
        }
    }

    public struct Scene
    {
    }
}