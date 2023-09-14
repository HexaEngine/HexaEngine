namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO.Metadata;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public struct PlainNode
    {
        public int Id;
        public string Name;
        public int ParentId;

        public PlainNode(int id, string name, int parentId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
        }
    }

    public unsafe class Node
    {
        public string Name;
        public Matrix4x4 Transform;
        public NodeFlags Flags;
        public List<uint> Meshes;
        public Node? Parent;
        public List<Node> Children;
        public Metadata Metadata;

        private Node()
        {
        }

        public Node(string name, Matrix4x4 transform, NodeFlags flags, List<uint> meshes, Node? parent, List<Node> children, Metadata metadata)
        {
            Name = name;
            Transform = transform;
            Flags = flags;
            Meshes = meshes;
            Parent = parent;
            Children = children;
            Metadata = metadata;
        }

        public Node(string name, Matrix4x4 transform, NodeFlags flags, Node? parent, Metadata metadata)
        {
            Name = name;
            Transform = transform;
            Flags = flags;
            Meshes = new();
            Parent = parent;
            Children = new();
            Metadata = metadata;
        }

        public Node(string name, Matrix4x4 transform, NodeFlags flags, Node? parent)
        {
            Name = name;
            Transform = transform;
            Flags = flags;
            Meshes = new();
            Parent = parent;
            Children = new();
            Metadata = new();
        }

        public static Node ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            Node node = new();
            node.Read(stream, encoding, endianness);
            return node;
        }

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            Name = stream.ReadString(encoding, endianness) ?? string.Empty;
            Transform = stream.ReadMatrix4x4(endianness);
            Flags = (NodeFlags)stream.ReadInt32(endianness);
            var meshesCount = stream.ReadInt32(endianness);
            Meshes = new(meshesCount);
            for (int i = 0; i < meshesCount; i++)
            {
                Meshes.Add(stream.ReadUInt32(endianness));
            }
            Metadata = Metadata.ReadFrom(stream, encoding, endianness);
            var childrenCount = stream.ReadInt32(endianness);
            Children = new(childrenCount);
            for (var i = 0; i < childrenCount; i++)
            {
                Node node = new();
                node.Read(stream, encoding, endianness);
                node.Parent = this;
                Children.Add(node);
            }
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(Name, encoding, endianness);
            stream.WriteMatrix4x4(Transform, endianness);
            stream.WriteInt32((int)Flags, endianness);
            stream.WriteInt32(Meshes.Count, endianness);
            for (int i = 0; i < Meshes.Count; i++)
            {
                stream.WriteUInt32(Meshes[i], endianness);
            }
            Metadata.Write(stream, encoding, endianness);
            stream.WriteInt32(Children.Count, endianness);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Write(stream, encoding, endianness);
            }
        }

        public void CountNodes(ref int count)
        {
            count++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CountNodes(ref count);
            }
        }

        public void CountBones(ref int count)
        {
            if ((Flags & NodeFlags.Bone) != 0)
                count++;

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CountBones(ref count);
            }
        }

        public void FillNodes(Node[] nodes, ref int index)
        {
            nodes[index] = this;
            index++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FillNodes(nodes, ref index);
            }
        }

        public void FillBones(Node[] nodes, ref int index)
        {
            if ((Flags & NodeFlags.Bone) != 0)
            {
                nodes[index] = this;
                index++;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FillBones(nodes, ref index);
            }
        }

        public void FillTransforms(Matrix4x4[] locals, ref int index)
        {
            locals[index] = Transform;
            index++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FillTransforms(locals, ref index);
            }
        }

        public void FillBoneTransforms(Matrix4x4[] locals, ref int index)
        {
            if ((Flags & NodeFlags.Bone) != 0)
            {
                locals[index] = Transform;
                index++;
            }
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FillBoneTransforms(locals, ref index);
            }
        }

        public void TraverseTree(PlainNode[] nodes, ref int index, int parent = -1)
        {
            var id = index;
            nodes[id] = new(id, Name, parent);
            index++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].TraverseTree(nodes, ref index, id);
            }
        }

        public void TraverseTreeBones(PlainNode[] nodes, ref int index, int parent = -1)
        {
            var id = index;
            if ((Flags & NodeFlags.Bone) != 0)
            {
                nodes[id] = new(id, Name, parent);
                index++;
            }
            else
            {
                id = parent;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].TraverseTreeBones(nodes, ref index, id);
            }
        }

        public Matrix4x4 GetGlobalTransform()
        {
            if (Parent == null)
                return Transform;
            return Transform * Parent.GetGlobalTransform();
        }

        public Matrix4x4 GetGlobalTransform(Matrix4x4 root)
        {
            if (Parent == null)
                return Transform * root;
            return Transform * Parent.GetGlobalTransform(root);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}