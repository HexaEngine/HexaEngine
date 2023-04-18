namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Scenes;

    public class Animature : IComponent
    {
#pragma warning disable CS8618 // Non-nullable field 'Node' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public Animature(string name)
#pragma warning restore CS8618 // Non-nullable field 'Node' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
            Name = name;
        }

        public string Name;
        public GameObject Node;
        public List<BoneData> Bones = new();
        public Dictionary<string, BoneData> BonesDictionary = new();
        public Dictionary<string, Relation> Relationships = new();
        public Dictionary<Relation, string> RelationshipsDictionary = new();

        public void AddBone(BoneData bone)
        {
            Bones.Add(bone);
            BonesDictionary.Add(bone.Name, bone);
        }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
        }

        public void Destory()
        {
        }
    }
}