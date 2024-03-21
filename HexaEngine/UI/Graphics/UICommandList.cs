namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public static class SortingAlgorithms
    {
        public static void MergeSort<T>(IList<T> array) where T : IComparable<T>
        {
            if (array == null || array.Count <= 1)
                return;

            T[] tempArray = new T[array.Count];
            MergeSort(array, tempArray, 0, array.Count - 1);
        }

        private static void MergeSort<T>(IList<T> array, IList<T> tempArray, int left, int right) where T : IComparable<T>
        {
            if (left < right)
            {
                int mid = (left + right) / 2;
                MergeSort(array, tempArray, left, mid);
                MergeSort(array, tempArray, mid + 1, right);
                Merge(array, tempArray, left, mid, right);
            }
        }

        private static void Merge<T>(IList<T> array, IList<T> tempArray, int left, int mid, int right) where T : IComparable<T>
        {
            int i = left, j = mid + 1, k = left;

            // Merge two sorted arrays into tempArray
            while (i <= mid && j <= right)
            {
                if (array[i].CompareTo(array[j]) <= 0)
                {
                    tempArray[k] = array[i];
                    i++;
                }
                else
                {
                    tempArray[k] = array[j];
                    j++;
                }
                k++;
            }

            // Copy remaining elements of left array if any
            while (i <= mid)
            {
                tempArray[k] = array[i];
                i++;
                k++;
            }

            // Copy remaining elements of right array if any
            while (j <= right)
            {
                tempArray[k] = array[j];
                j++;
                k++;
            }

            // Copy merged elements back to original array
            for (int m = left; m <= right; m++)
            {
                array[m] = tempArray[m];
            }
        }

        public static void BlockSort<T>(IList<T> array) where T : IComparable<T>
        {
            if (array == null || array.Count <= 1)
                return;

            int blockSize = (int)Math.Sqrt(array.Count);

            for (int i = 0; i < array.Count; i += blockSize)
            {
                int end = Math.Min(i + blockSize - 1, array.Count - 1);
                PartialSort(array, i, end);
            }

            // Merge sorted blocks in place
            int blocks = (int)Math.Ceiling((double)array.Count / blockSize);
            for (int i = 1; i < blocks; i++)
            {
                Merge(array, blockSize * (i - 1), blockSize * i - 1, Math.Min(blockSize * (i + 1) - 1, array.Count - 1));
            }
        }

        private static void PartialSort<T>(IList<T> array, int start, int end) where T : IComparable<T>
        {
            for (int i = start; i <= end; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j <= end; j++)
                {
                    if (array[j].CompareTo(array[minIndex]) < 0)
                    {
                        minIndex = j;
                    }
                }
                if (minIndex != i)
                {
                    (array[minIndex], array[i]) = (array[i], array[minIndex]);
                }
            }
        }

        private static void Merge<T>(IList<T> array, int left, int mid, int right) where T : IComparable<T>
        {
            int i = left, j = mid + 1;

            while (i <= mid && j <= right)
            {
                if (array[i].CompareTo(array[j]) <= 0)
                {
                    i++;
                }
                else
                {
                    T temp = array[j];
                    // Shift elements from j to i
                    for (int k = j; k > i; k--)
                    {
                        array[k] = array[k - 1];
                    }
                    array[i] = temp;
                    i++;
                    mid++;
                    j++;
                }
            }
        }

        public static void BlockSort<T>(IList<T> array, IComparer<T> comparer)
        {
            if (array == null || array.Count <= 1)
                return;

            int blockSize = (int)Math.Sqrt(array.Count);

            for (int i = 0; i < array.Count; i += blockSize)
            {
                int end = Math.Min(i + blockSize - 1, array.Count - 1);
                PartialSort(array, i, end, comparer);
            }

            // Merge sorted blocks in place
            int blocks = (int)Math.Ceiling((double)array.Count / blockSize);
            for (int i = 1; i < blocks; i++)
            {
                Merge(array, blockSize * (i - 1), blockSize * i - 1, Math.Min(blockSize * (i + 1) - 1, array.Count - 1), comparer);
            }
        }

        private static void PartialSort<T>(IList<T> array, int start, int end, IComparer<T> comparer)
        {
            for (int i = start; i <= end; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j <= end; j++)
                {
                    if (comparer.Compare(array[j], array[minIndex]) < 0)
                    {
                        minIndex = j;
                    }
                }
                if (minIndex != i)
                {
                    (array[minIndex], array[i]) = (array[i], array[minIndex]);
                }
            }
        }

        private static void Merge<T>(IList<T> array, int left, int mid, int right, IComparer<T> comparer)
        {
            int i = left, j = mid + 1;

            while (i <= mid && j <= right)
            {
                if (comparer.Compare(array[i], array[j]) <= 0)
                {
                    i++;
                }
                else
                {
                    T temp = array[j];
                    // Shift elements from j to i
                    for (int k = j; k > i; k--)
                    {
                        array[k] = array[k - 1];
                    }
                    array[i] = temp;
                    i++;
                    mid++;
                    j++;
                }
            }
        }
    }

    public class ZIndexComparer : IComparer<UIDrawCommand>
    {
        public static readonly ZIndexComparer Instance = new();

        int IComparer<UIDrawCommand>.Compare(UIDrawCommand x, UIDrawCommand y)
        {
            if (x.ZIndex == y.ZIndex)
            {
                return 0;
            }
            if (x.ZIndex < y.ZIndex)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    public unsafe class UICommandList
    {
        private UnsafeList<UIVertex> vertices = new();
        private UnsafeList<uint> indices = new();
        private readonly List<UIDrawCommand> commands = new();
        private readonly Stack<Matrix3x2> transformStack = new();
        private readonly Stack<ClipRectangle> clipRectStack = new();

        private Matrix3x2 transformSum = Matrix3x2.Identity;
        private Matrix3x2 transformTop = Matrix3x2.Identity;
        private Matrix3x2 transformLocal = Matrix3x2.Identity;
        private Matrix3x2 transformGlobal = Matrix3x2.Identity;

        private ClipRectangle clipRect;
        private uint vertexCountSinceLast;
        private uint indexCountSinceLast;
        private uint vertexCountOffset;
        private uint indexCountOffset;

        private int zIndex;

        public UICommandList()
        {
        }

        public int TotalVtxCount => (int)vertices.Size;

        public int TotalIdxCount => (int)indices.Size;

        public UnsafeList<UIVertex> VtxBuffer => vertices;

        public UnsafeList<uint> IdxBuffer => indices;

        public List<UIDrawCommand> CmdBuffer => commands;

        public UIVertex* VtxWritePtr => vertices.Back;

        public uint* IdxWritePtr => indices.Back;

        public Matrix3x2 Transform
        {
            get => transformLocal;
            set
            {
                transformLocal = value;
                transformGlobal = value * transformSum;
            }
        }

        public ClipRectangle ClipRect => clipRect;

        public int ZIndex { get => zIndex; set => zIndex = value; }

        public void PushClipRect(ClipRectangle clipRect)
        {
            clipRectStack.Push(this.clipRect);
            this.clipRect = clipRect;
        }

        public void PopClipRect()
        {
            clipRect = clipRectStack.Pop();
        }

        public void PushTransform(Matrix3x2 transform)
        {
            transformStack.Push(transformTop);
            transformTop = transform;
            transformSum = transform * transformSum;
        }

        public void PopTransform()
        {
            Matrix3x2.Invert(transformTop, out var transformInv);
            transformSum = transformInv * transformSum;
            transformTop = transformStack.Pop();
        }

        public uint AddVertex(UIVertex vertex)
        {
            uint idx = vertexCountSinceLast;
            vertices.Add(vertex);
            vertexCountSinceLast++;
            return idx;
        }

        public void AddIndex(uint index)
        {
            indices.Add(index);
            indexCountSinceLast++;
        }

        public void AddFace(uint i0, uint i1, uint i2)
        {
            indices.Add(i0);
            indices.Add(i1);
            indices.Add(i2);
            indexCountSinceLast += 3;
        }

        public void AddFace(int i0, int i1, int i2)
        {
            indices.Add((uint)i0);
            indices.Add((uint)i1);
            indices.Add((uint)i2);
            indexCountSinceLast += 3;
        }

        public UIVertex* ReserveVertices(uint count)
        {
            var s = vertices.Size;
            vertices.Resize(vertices.Size + count);
            vertexCountSinceLast += count;
            return vertices.Data + s;
        }

        public uint* ReserveIndices(uint count)
        {
            var s = indices.Size;
            indices.Resize(indices.Size + count);
            indexCountSinceLast += count;
            return indices.Data + s;
        }

        public void Reserve(uint idxCount, uint vtxCount)
        {
            indices.Resize(indices.Size + idxCount);
            indexCountSinceLast += idxCount;
            vertices.Resize(vertices.Size + vtxCount);
            vertexCountSinceLast += vtxCount;
        }

        public void PrimReserve(int idxCount, int vtxCount)
        {
            indices.Reserve(indices.Size + (uint)idxCount);
            vertices.Reserve(vertices.Size + (uint)vtxCount);
        }

        public void RecordDraw(UICommandType commandType = UICommandType.DrawPrimitive, Brush? brush = null, nint textureId0 = 0, nint textureId1 = 0)
        {
            Vector2 min = new(float.MaxValue);
            Vector2 max = new(float.MinValue);
            for (uint i = 0; i < vertexCountSinceLast; i++)
            {
                Vector2* v = &vertices.Data[vertexCountOffset + i].Position;
                Vector2 ve = Vector2.Transform(*v, transformGlobal);
                min = Vector2.Min(min, ve);
                max = Vector2.Max(max, ve);
                *v = ve;
            }

            RectangleF bounds = new(min.X, min.Y, max.X, max.Y);

            UIDrawCommand cmd = new(vertices.Data, indices.Data, vertexCountSinceLast, indexCountSinceLast, vertexCountOffset, indexCountOffset, zIndex: zIndex, clipRect, bounds, commandType, brush, textureId0, textureId1);
            commands.Add(cmd);

            vertexCountOffset = vertices.Size;
            indexCountOffset = indices.Size;
            vertexCountSinceLast = 0;
            indexCountSinceLast = 0;
        }

        public void ExecuteCommandList(UICommandList commandList)
        {
            for (int i = 0; i < commandList.commands.Count; i++)
            {
                var cmd = commandList.commands[i];
                cmd.VertexOffset += vertexCountOffset;
                cmd.IndexOffset += indexCountOffset;
                cmd.ClipRect = clipRect;
                commands.Add(cmd);
            }

            PrimReserve(commandList.TotalIdxCount, commandList.TotalVtxCount);

            vertices.AppendRange(commandList.vertices.Data, commandList.vertices.Size);
            indices.AppendRange(commandList.indices.Data, commandList.indices.Size);

            if (transformGlobal != Matrix3x2.Identity)
            {
                for (uint i = 0; i < commandList.vertices.Size; i++)
                {
                    vertices.Data[vertexCountOffset + i].Position = Vector2.Transform(vertices[vertexCountOffset + i].Position, transformGlobal);
                }
            }

            vertexCountOffset = vertices.Size;
            indexCountOffset = indices.Size;
        }

        public void BeginDraw()
        {
            UIDrawCommand.id = 0;
            vertices.Clear();
            indices.Clear();
            commands.Clear();
            clipRectStack.Clear();

            indexCountOffset = 0;
            vertexCountOffset = 0;
        }

        public void EndDraw()
        {
            if (commands.Count < 2)
            {
                SortingAlgorithms.BlockSort(commands, ZIndexComparer.Instance);
                return;
            }

            var lastCmd = commands[0];
            var lastCmdIdx = 0;
            for (int i = 1; i < commands.Count; i++)
            {
                var cmd = commands[i];
                if (cmd.Type == lastCmd.Type && cmd.TextureId0 == lastCmd.TextureId0 && cmd.TextureId1 == lastCmd.TextureId1 && cmd.Brush == lastCmd.Brush && cmd.ZIndex == lastCmd.ZIndex)
                {
                    if (lastCmd.VertexOffset + lastCmd.VertexCount == cmd.VertexOffset)
                    {
                        for (uint j = 0; j < cmd.IndexCount; j++)
                        {
                            indices[j + cmd.IndexOffset] += lastCmd.VertexCount;
                        }

                        lastCmd.VertexCount += cmd.VertexCount;
                        lastCmd.IndexCount += cmd.IndexCount;

                        commands[lastCmdIdx] = lastCmd;
                        commands.RemoveAt(i);
                        i--;

                        continue;
                    }
                }

                lastCmd = cmd;
                lastCmdIdx = i;
            }

            SortingAlgorithms.BlockSort(commands, ZIndexComparer.Instance);
        }

        public void Dispose()
        {
            vertices.Release();
            indices.Release();
        }
    }
}