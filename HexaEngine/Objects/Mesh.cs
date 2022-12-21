namespace HexaEngine.Objects
{
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class Animature
    {
        public Animature(string name)
        {
            Name = name;
        }

        public string Name;
        public GameObject Node;
        public List<MeshBone> Bones = new();
        public Dictionary<string, MeshBone> BonesDictionary = new();
        public Dictionary<string, Relation> Relationships = new();
        public Dictionary<Relation, string> RelationshipsDictionary = new();

        public void AddBone(MeshBone bone)
        {
            Bones.Add(bone);
            BonesDictionary.Add(bone.Name, bone);
        }

        public void AddRelation(string name, string parent)
        {
            if (Relationships.TryGetValue(parent, out Relation? parentRelation) && !Relationships.ContainsKey(name))
            {
                var childRelation = new Relation() { Parent = parentRelation, ParentName = parent };
                Relationships.Add(name, childRelation);
                RelationshipsDictionary.Add(childRelation, name);
            }
            else if (Relationships.TryGetValue(name, out Relation? childRelation) && !Relationships.ContainsKey(parent))
            {
                parentRelation = new Relation() { Parent = null, ParentName = null };
                childRelation.Parent = parentRelation;
                childRelation.ParentName = parent;
                Relationships.Add(parent, parentRelation);
                RelationshipsDictionary.Add(parentRelation, parent);
            }
            else if (Relationships.TryGetValue(parent, out parentRelation) && Relationships.TryGetValue(name, out childRelation))
            {
                childRelation.Parent = parentRelation;
                childRelation.ParentName = parent;
            }
            else
            {
                parentRelation = new();
                Relationships.Add(parent, parentRelation);
                RelationshipsDictionary.Add(parentRelation, parent);
                childRelation = new() { Parent = parentRelation, ParentName = parent };
                Relationships.Add(name, childRelation);
                RelationshipsDictionary.Add(childRelation, name);
            }
        }

        public Matrix4x4 GetTransform(string? name)
        {
            if (name == null) return Matrix4x4.Identity;
            return BonesDictionary[RelationshipsDictionary[Relationships[name]]].Offset;
        }

        public Matrix4x4 GetGlobalTransform(string? name)
        {
            if (name == null) return Matrix4x4.Identity;
            Stack<Relation> relations = new();
            Relation? relation = Relationships[name];
            while (relation != null)
            {
                relations.Push(relation);
                relation = relation.Parent;
            }
            relations.Pop();
            relations.Reverse();
            Matrix4x4 result = Matrix4x4.Identity;
            while (relations.TryPop(out relation))
            {
                result *= BonesDictionary[RelationshipsDictionary[relation]].Offset;
            }
            return result;
        }

        public string FindRoot()
        {
            Relation relation = Relationships[Bones[0].Name];
            string name = Bones[0].Name;
            while (true)
            {
                if (relation.Parent == null)
                    return name;
                name = relation.ParentName ?? throw new();
                relation = relation.Parent;
            }
        }
    }

    public class Relation
    {
        public string? ParentName;
        public Relation? Parent;
    }

    public unsafe class Mesh
    {
        public MeshVertex[]? Vertices;
        public int[]? Indices;
        public MeshBone[]? Bones;
        public Animature? Animature;
        public BoundingBox AABB;
        private string name = string.Empty;

        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }
    }
}