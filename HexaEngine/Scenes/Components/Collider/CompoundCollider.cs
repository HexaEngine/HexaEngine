namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Physics;
    using System.Linq;
    using System.Numerics;

    [EditorComponent(typeof(CompoundCollider), "Compound Collider")]
    public class CompoundCollider : BaseCollider, ICompoundCollider
    {
        protected List<IColliderComponent>? colliderChildren;
        protected Buffer<CompoundChild> compoundChildren;
        protected Vector3 center;
        protected Compound compound;

        [JsonIgnore]
        public Vector3 Center => center;

        [JsonIgnore]
        public IReadOnlyList<IColliderComponent>? Children => colliderChildren;

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || hasShape)
            {
                return;
            }

            colliderChildren = parent.GetComponentsFromChilds<IColliderComponent>().ToList();
            CompoundBuilder builder = new(bufferPool, simulation.Shapes, colliderChildren.Count);
            for (int i = 0; i < colliderChildren.Count; i++)
            {
                colliderChildren[i].BuildCompound(ref builder);
            }

            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);

            if (Type == ColliderType.Dynamic)
            {
                builder.BuildDynamicCompound(out compoundChildren, out inertia, out center);
            }

            if (Type == ColliderType.Kinematic)
            {
                builder.BuildKinematicCompound(out compoundChildren, out center);
            }

            if (Type == ColliderType.Static)
            {
                builder.Dispose();
                ImGuiConsole.Log(LogSeverity.Warning, "Compound cannot be a static type");
                return;
            }

            compound = new(compoundChildren);
            index = simulation.Shapes.Add(compound);

            center -= parent.Transform.GlobalPosition;
            for (int i = 0; i < compoundChildren.Length; i++)
            {
                colliderChildren[i].SetCompoundData(this, compoundChildren[i]);
            }
            // builder.Dispose();
            hasShape = true;
        }

        public override void DestroyShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || colliderChildren == null || !hasShape)
            {
                return;
            }

            compound.Dispose(bufferPool);
            simulation.Shapes.Remove(index);
            for (int i = 0; i < colliderChildren.Count; i++)
            {
                colliderChildren[i].DestroyCompound();
            }
            bufferPool.Return(ref compoundChildren);
            hasShape = false;
        }
    }
}