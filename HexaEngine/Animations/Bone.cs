namespace HexaEngine.Animations
{
    using System.Collections.Generic;
    using System.Numerics;

    // TODO: Bone not yet implemented
    public class Bone
    {
        public string Name { get; set; }

        // Bind space transform
        public Matrix4x4 Offset { get; set; }

        // local matrix transform
        public Matrix4x4 LocalTransform { get; set; }

        // To-root transform
        public Matrix4x4 GlobalTransform { get; set; }

        // copy of the original local transform
        public Matrix4x4 OriginalLocalTransform { get; set; }

        // parent bone reference
        public Bone Parent { get; set; }

        // child bone references
        public List<Bone> Children { get; private set; }

        public IEnumerable<Bone> GetBones()
        {
            yield return this;
            foreach (var bone in Children)
            {
                foreach (var child in bone.GetBones())
                {
                    yield return child;
                }
            }
        }
    }
}