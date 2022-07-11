namespace TestGame
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.Trees;
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System.Diagnostics;
    using System.Numerics;

    public class MovementController : IComponent
    {
        private Camera node;
        private Scene scene;
        private const float DegToRadFactor = 0.0174532925f;
        public float Speed = 10F;
        public float AngluarSpeed = 20F;
        public const float AngluarGrain = 0.004f;
        private bool leftDown;
        private long lastTick;

        public bool IsVisible { get; set; }

        public MovementController()
        {
            lastTick = Stopwatch.GetTimestamp();
        }

        public void Update()
        {
            var ticks = Stopwatch.GetTimestamp();
            var deltaTime = (float)(ticks - lastTick);
            deltaTime /= Stopwatch.Frequency;
            lastTick = ticks;

            if (Designer.InDesignMode)
                return;

            var delta = Mouse.GetDelta();

            if (delta.X != 0 | delta.Y != 0)
            {
                var re = new Vector3(delta.X, delta.Y, 0) * deltaTime * AngluarSpeed;
                node.Transform.Rotation += re;
                if (node.Transform.Rotation.Y < 270 & node.Transform.Rotation.Y > 180)
                {
                    node.Transform.Rotation = new Vector3(node.Transform.Rotation.X, 270f, node.Transform.Rotation.Z);
                }
                if (node.Transform.Rotation.Y > 90 & node.Transform.Rotation.Y < 270)
                {
                    node.Transform.Rotation = new Vector3(node.Transform.Rotation.X, 90f, node.Transform.Rotation.Z);
                }
            }

            if (Keyboard.IsDown(Keys.W))
            {
                var rotation = Matrix4x4.CreateFromYawPitchRoll(node.Transform.Rotation.X * DegToRadFactor, node.Transform.Rotation.Y * DegToRadFactor, 0f);
                if (Keyboard.IsDown(Keys.Lshift))
                    node.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                else
                    node.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * Time.Delta;
            }
            if (Keyboard.IsDown(Keys.S))
            {
                var rotation = Matrix4x4.CreateFromYawPitchRoll(node.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                if (Keyboard.IsDown(Keys.Lshift))
                    node.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                else
                    node.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * Time.Delta;
            }
            if (Keyboard.IsDown(Keys.A))
            {
                var rotation = Matrix4x4.CreateFromYawPitchRoll(node.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                if (Keyboard.IsDown(Keys.Lshift))
                    node.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                else
                    node.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * Time.Delta;
            }
            if (Keyboard.IsDown(Keys.D))
            {
                var rotation = Matrix4x4.CreateFromYawPitchRoll(node.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                if (Keyboard.IsDown(Keys.Lshift))
                    node.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                else
                    node.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * Time.Delta;
            }
            if (Keyboard.IsDown(Keys.Space))
            {
                var rotation = Matrix4x4.CreateFromYawPitchRoll(node.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                if (Keyboard.IsDown(Keys.Lshift))
                    node.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                else
                    node.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * Time.Delta;
            }
            if (Keyboard.IsDown(Keys.C))
            {
                var rotation = Matrix4x4.CreateFromYawPitchRoll(node.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                if (Keyboard.IsDown(Keys.Lshift))
                    node.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                else
                    node.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * Time.Delta;
            }
            if (!Mouse.IsDown(MouseButton.Left) & leftDown)
            {
                leftDown = false;
            }
            if (Mouse.IsDown(MouseButton.Left))
            {
                var handler = new RayHitHandler(scene.Simulation);
                scene.Simulation.RayCast(node.Transform.Position, node.Transform.Forward, 100, ref handler);
            }
        }

        public void Initialize(IGraphicsDevice device, SceneNode node)
        {
            this.node = node as Camera;
            scene = node.GetScene();
        }

        public void Uninitialize()
        {
        }

        private class RayHitHandler : IRayHitHandler
        {
            private readonly Simulation Simulation;

            public RayHitHandler(Simulation simulation)
            {
                Simulation = simulation;
            }

            public bool AllowTest(CollidableReference collidable)
            {
                return collidable.Mobility == CollidableMobility.Dynamic;
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return collidable.Mobility == CollidableMobility.Dynamic;
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
            {
                var refer = Simulation.Bodies.GetBodyReference(collidable.BodyHandle);
                refer.ApplyLinearImpulse(ray.Direction);
                refer.Awake = true;
            }
        }
    }
}