namespace HexaEngine.Objects
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class Skeleton
    {
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

        public Matrix4x4 GetGlobalTransform(string name)
        {
            Stack<Relation> relations = new();
            Relation? relation = Relationships[name];
            while (relation != null)
            {
                relations.Push(relation);
                relation = relation.Parent;
            }
            relations.Pop();
            Matrix4x4 result = Matrix4x4.Identity;
            while (relations.TryPop(out relation))
            {
                result *= BonesDictionary[RelationshipsDictionary[relation]].Offset;
            }
            return result;
        }
    }

    public class Relation
    {
        public string? ParentName;
        public Relation? Parent;
    }

    [EditorNode("Mesh")]
    public unsafe class Mesh
    {
        public MeshVertex[]? Vertices;
        public int[]? Indices;
        public MeshBone[]? Bones;
        public Skeleton? Skeleton;
        public int MaterialIndex = -1;
        private string materialName = string.Empty;
        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }

        [EditorProperty("Material name")]
        public string MaterialName
        {
            get => materialName;
            set
            {
                materialName = value;
            }
        }
    }
}