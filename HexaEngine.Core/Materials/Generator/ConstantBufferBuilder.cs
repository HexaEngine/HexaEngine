namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Materials.Generator.Structs;
    using System.Collections;
    using System.Collections.Generic;

    public class ConstantBufferBuilder : IList<ConstantBufferDef>
    {
        private readonly VariableTable table;
        private readonly string name;
        private readonly uint slot;
        private readonly List<ConstantBufferDef> defs = [];

        public ConstantBufferBuilder(VariableTable table, string? name = null, uint slot = uint.MaxValue)
        {
            this.table = table;
            name = table.GetUniqueName(name ?? "constants");
            this.name = name;
            this.slot = slot;
        }

        public string Name => name;

        public uint Slot => slot;

        public int Count => defs.Count;

        public bool IsReadOnly => ((ICollection<ConstantBufferDef>)defs).IsReadOnly;

        public ConstantBufferDef this[int index] { get => defs[index]; set => defs[index] = value; }

        public void Finish(bool autoSlot)
        {
            if (defs.Count == 0) return;

            ConstantBuffer buffer = new(slot, name, defs);
            table.AddConstantBuffer(buffer, autoSlot);
        }

        public void Add(string name, SType type)
        {
            Add(new ConstantBufferDef(name, type));
        }

        public void Add(ConstantBufferDef def)
        {
            SanityChecks(def);
            defs.Add(def);
        }

        private static void SanityChecks(ConstantBufferDef def)
        {
            if (!def.Type.IsAny(TypeFlags.Scalar | TypeFlags.Vector | TypeFlags.Matrix | TypeFlags.Struct))
            {
                throw new InvalidOperationException("SType must be either scalar, vector, matrix or struct type.");
            }
        }

        public bool Remove(string name)
        {
            return Remove(new ConstantBufferDef(name));
        }

        public bool Remove(ConstantBufferDef def)
        {
            return defs.Remove(def);
        }

        public void RemoveAt(int index)
        {
            defs.RemoveAt(index);
        }

        public int IndexOf(string name)
        {
            return IndexOf(new ConstantBufferDef(name));
        }

        public int IndexOf(ConstantBufferDef def)
        {
            return defs.IndexOf(def);
        }

        public void Insert(int index, string name, SType type)
        {
            Insert(index, new ConstantBufferDef(name, type));
        }

        public void Insert(int index, ConstantBufferDef def)
        {
            SanityChecks(def);
            defs.Insert(index, def);
        }

        public void Clear()
        {
            defs.Clear();
        }

        public bool Contains(ConstantBufferDef item)
        {
            return defs.Contains(item);
        }

        public void CopyTo(ConstantBufferDef[] array, int arrayIndex)
        {
            defs.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ConstantBufferDef> GetEnumerator()
        {
            return defs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}