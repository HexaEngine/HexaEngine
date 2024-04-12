namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Mathematics;
    using System;
    using System.IO;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a plain node with basic identification and hierarchy information.
    /// </summary>
    public struct PlainNode
    {
        /// <summary>
        /// Gets or sets the unique identifier of the node.
        /// </summary>
        public int Id;

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the identifier of the parent node in the hierarchy.
        /// </summary>
        public int ParentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlainNode"/> struct with the specified values.
        /// </summary>
        /// <param name="id">The unique identifier of the node.</param>
        /// <param name="name">The name of the node.</param>
        /// <param name="parentId">The identifier of the parent node in the hierarchy.</param>
        public PlainNode(int id, string name, int parentId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
        }
    }

    /// <summary>
    /// Represents a node in a 3D scene hierarchy.
    /// </summary>
    public unsafe class Node
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the transformation matrix of the node.
        /// </summary>
        public Matrix4x4 Transform;

        /// <summary>
        /// Gets or sets the flags associated with the node.
        /// </summary>
        public NodeFlags Flags;

        /// <summary>
        /// Gets or sets the list of mesh IDs associated with the node.
        /// </summary>
        public List<uint> Meshes;

        /// <summary>
        /// Gets or sets the parent node in the hierarchy.
        /// </summary>
        public Node? Parent;

        /// <summary>
        /// Gets or sets the list of child nodes.
        /// </summary>
        public List<Node> Children;

        /// <summary>
        /// Gets or sets the metadata associated with the node.
        /// </summary>
        public Metadata Metadata;

#nullable disable

        /// <summary>
        /// Private constructor for creating an instance of the <see cref="Node"/> class.
        /// </summary>
        private Node()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class with specified values.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="transform">The transformation matrix of the node.</param>
        /// <param name="flags">The flags associated with the node.</param>
        /// <param name="meshes">The list of mesh IDs associated with the node.</param>
        /// <param name="parent">The parent node in the hierarchy.</param>
        /// <param name="children">The list of child nodes.</param>
        /// <param name="metadata">The metadata associated with the node.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class with specified values.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="transform">The transformation matrix of the node.</param>
        /// <param name="flags">The flags associated with the node.</param>
        /// <param name="parent">The parent node in the hierarchy.</param>
        /// <param name="metadata">The metadata associated with the node.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class with specified values.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="transform">The transformation matrix of the node.</param>
        /// <param name="flags">The flags associated with the node.</param>
        /// <param name="parent">The parent node in the hierarchy.</param>
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

        /// <summary>
        /// Reads a <see cref="Node"/> instance from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for strings.</param>
        /// <param name="endianness">The endianness of the binary data in the stream.</param>
        /// <returns>The read <see cref="Node"/> instance.</returns>
        public static Node ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            Node node = new();
            node.Read(stream, encoding, endianness);
            return node;
        }

        /// <summary>
        /// Reads the <see cref="Node"/> data from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for strings.</param>
        /// <param name="endianness">The endianness of the binary data in the stream.</param>
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

        /// <summary>
        /// Writes the <see cref="Node"/> data to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used for strings.</param>
        /// <param name="endianness">The endianness of the binary data in the stream.</param>
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

        /// <summary>
        /// Recursively counts the total number of nodes in the hierarchy, starting from this node.
        /// </summary>
        /// <param name="count">The current count, which will be updated by this method.</param>
        public void CountNodes(ref int count)
        {
            count++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CountNodes(ref count);
            }
        }

        /// <summary>
        /// Recursively counts the total number of bones in the hierarchy, starting from this node.
        /// </summary>
        /// <param name="count">The current count, which will be updated by this method.</param>
        public void CountBones(ref int count)
        {
            if ((Flags & NodeFlags.Bone) != 0)
            {
                count++;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CountBones(ref count);
            }
        }

        /// <summary>
        /// Fills an array with nodes in a depth-first order, starting from this node.
        /// </summary>
        /// <param name="nodes">The array to be filled with nodes.</param>
        /// <param name="index">The current index in the array, which will be updated by this method.</param>
        public void FillNodes(Node[] nodes, ref int index)
        {
            nodes[index] = this;
            index++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FillNodes(nodes, ref index);
            }
        }

        /// <summary>
        /// Fills an array with bones in a depth-first order, starting from this node.
        /// </summary>
        /// <param name="nodes">The array to be filled with bones.</param>
        /// <param name="index">The current index in the array, which will be updated by this method.</param>
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

        /// <summary>
        /// Fills an array with local transformation matrices in a depth-first order, starting from this node.
        /// </summary>
        /// <param name="locals">The array to be filled with local transformation matrices.</param>
        /// <param name="index">The current index in the array, which will be updated by this method.</param>
        public void FillTransforms(Matrix4x4[] locals, ref int index)
        {
            locals[index] = Transform;
            index++;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FillTransforms(locals, ref index);
            }
        }

        /// <summary>
        /// Fills an array with bone transformation matrices in a depth-first order, starting from this node.
        /// </summary>
        /// <param name="locals">The array to be filled with bone transformation matrices.</param>
        /// <param name="index">The current index in the array, which will be updated by this method.</param>
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

        /// <summary>
        /// Traverses the hierarchy and populates an array of <see cref="PlainNode"/> instances representing nodes in a tree structure.
        /// </summary>
        /// <param name="nodes">The array to be populated with <see cref="PlainNode"/> instances.</param>
        /// <param name="index">The current index in the array, which will be updated by this method.</param>
        /// <param name="parent">The parent ID for the current node in the hierarchy.</param>
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

        /// <summary>
        /// Traverses the hierarchy and populates an array of <see cref="PlainNode"/> instances representing bones in a tree structure.
        /// </summary>
        /// <param name="nodes">The array to be populated with <see cref="PlainNode"/> instances.</param>
        /// <param name="index">The current index in the array, which will be updated by this method.</param>
        /// <param name="parent">The parent ID for the current node in the hierarchy.</param>
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

        /// <summary>
        /// Gets the global transformation matrix of the node in the hierarchy.
        /// </summary>
        /// <returns>The global transformation matrix.</returns>
        public Matrix4x4 GetGlobalTransform()
        {
            if (Parent == null)
            {
                return Transform;
            }

            return Transform * Parent.GetGlobalTransform();
        }

        /// <summary>
        /// Gets the global transformation matrix of the node in the hierarchy, considering a root transformation.
        /// </summary>
        /// <param name="root">The root transformation matrix.</param>
        /// <returns>The global transformation matrix.</returns>
        public Matrix4x4 GetGlobalTransform(Matrix4x4 root)
        {
            if (Parent == null)
            {
                return Transform * root;
            }

            return Transform * Parent.GetGlobalTransform(root);
        }

        /// <summary>
        /// Returns a string representation of the node, which is its name.
        /// </summary>
        /// <returns>The name of the node.</returns>
        public override string ToString()
        {
            return Name;
        }

        public Node? FindNode(string name)
        {
            if (Name == name)
            {
                return this;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var node = Children[i].FindNode(name);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }
    }
}