namespace HexaEngine.Objects
{
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Scenes;
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
    }
}