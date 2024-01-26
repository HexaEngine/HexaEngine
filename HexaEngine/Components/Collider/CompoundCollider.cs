namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using System.Linq;
    using System.Numerics;

    [EditorCategory("Collider")]
    [EditorComponent(typeof(CompoundCollider), "Compound Collider")]
    public class CompoundCollider : BepuBaseCollider, ICompoundCollider
    {
        protected List<IBepuColliderComponent>? colliderChildren;
        protected Buffer<CompoundChild> compoundChildren;
        protected Vector3 center;
        protected Compound compound;

        [JsonIgnore]
        public Vector3 Center => center;

        [JsonIgnore]
        public IReadOnlyList<IBepuColliderComponent>? Children => colliderChildren;

        public override void CreateShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || bufferPool == null || hasShape)
            {
                return;
            }
            lock (bufferPool)
            {
                lock (simulation)
                {
                    colliderChildren = GameObject.GetComponentsFromChilds<IBepuColliderComponent>().ToList();
                    CompoundBuilder builder = new(bufferPool, simulation.Shapes, colliderChildren.Count);
                    for (int i = 0; i < colliderChildren.Count; i++)
                    {
                        colliderChildren[i].BuildCompound(ref builder);
                    }

                    pose = new(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);

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
                        Logger.Warn("Compound cannot be a static type");
                        return;
                    }

                    compound = new(compoundChildren);
                    index = simulation.Shapes.Add(compound);

                    center -= GameObject.Transform.GlobalPosition;
                    for (int i = 0; i < compoundChildren.Length; i++)
                    {
                        colliderChildren[i].SetCompoundData(this, compoundChildren[i]);
                    }
                    builder.Dispose();
                }
            }
            hasShape = true;
        }

        public override void DestroyShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || bufferPool == null || colliderChildren == null || !hasShape)
            {
                return;
            }
            lock (simulation)
            {
                simulation.Shapes.Remove(index);
            }
            lock (bufferPool)
            {
                compound.Dispose(bufferPool);
            }

            for (int i = 0; i < colliderChildren.Count; i++)
            {
                colliderChildren[i].DestroyCompound();
            }
            lock (bufferPool)
            {
                bufferPool.Return(ref compoundChildren);
            }
            hasShape = false;
        }
    }
}