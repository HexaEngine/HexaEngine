namespace HexaEngine.Scenes.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using System.Linq;

    [EditorComponent(typeof(BigCompoundCollider), "Big Compound Collider")]
    public class BigCompoundCollider : CompoundCollider
    {
        protected new BigCompound compound;

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || hasShape) return;
            colliderChildren = parent.GetComponentsFromChilds<BaseCollider>().ToList();
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

            compound = new(compoundChildren, simulation.Shapes, bufferPool);
            index = simulation.Shapes.Add(compound);

            center -= parent.Transform.GlobalPosition;
            for (int i = 0; i < compoundChildren.Length; i++)
            {
                colliderChildren[i].SetCompoundData(this, compoundChildren[i]);
            }
            builder.Dispose();
            hasShape = true;
        }

        public override void DestroyShape()
        {
            if (Application.InDesignMode || parent == null || scene == null || colliderChildren == null || !hasShape) return;
            compound.Dispose(bufferPool);
            scene.Simulation.Shapes.Remove(index);
            for (int i = 0; i < colliderChildren.Count; i++)
            {
                colliderChildren[i].DestroyCompound();
            }
            scene.BufferPool.Return(ref compoundChildren);
            hasShape = false;
        }
    }
}