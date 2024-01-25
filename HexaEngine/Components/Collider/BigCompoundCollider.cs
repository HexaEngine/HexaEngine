namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using System.Linq;

    [EditorComponent(typeof(BigCompoundCollider), "Big Compound Collider")]
    public class BigCompoundCollider : CompoundCollider
    {
        protected new BigCompound compound;

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
                    colliderChildren = GameObject.GetComponentsFromChilds<IColliderComponent>().ToList();
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

                    compound = new(compoundChildren, simulation.Shapes, bufferPool);
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