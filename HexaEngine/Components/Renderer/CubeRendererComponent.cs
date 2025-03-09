namespace HexaEngine.Components.Renderer
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Editor.Attributes;
    using System.Numerics;

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(CubeRendererComponent), "Cube Renderer", Icon = "\xf5ee")]
    public class CubeRendererComponent : PrimitiveRenderComponent
    {
        private float width = 1;
        private float height = 1;
        private float depth = 1;

        [EditorProperty("Width", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Width { get => width; set => SetAndUpdateModelEquals(ref width, value); }

        [EditorProperty("Height", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Height { get => height; set => SetAndUpdateModelEquals(ref height, value); }

        [EditorProperty("Depth", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Depth { get => depth; set => SetAndUpdateModelEquals(ref depth, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new Cube(new CubeDesc(width, height, depth));
        }

        protected override BoundingBox GetBoundingBox()
        {
            Vector3 half = new Vector3(width, height, depth) * 0.5f;
            return new BoundingBox(-half, half);
        }
    }

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(SphereRendererComponent), "Sphere Renderer", Icon = "\xf5ee")]
    public class SphereRendererComponent : PrimitiveRenderComponent
    {
        private float diameter = 1;
        private uint tessellation = 16;

        [EditorProperty("Diameter", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Diameter { get => diameter; set => SetAndUpdateModelEquals(ref diameter, value); }

        [EditorProperty("Tessellation", 3u, 128u, EditorPropertyMode.Default)]
        public uint Tessellation { get => tessellation; set => SetAndUpdateModelEquals(ref tessellation, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new UVSphere(new(diameter, tessellation));
        }

        protected override BoundingBox GetBoundingBox()
        {
            float radius = diameter * 0.5f;
            return new BoundingBox(new Vector3(-radius), new Vector3(radius));
        }
    }

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(TetrahedronRendererComponent), "Tetrahedron Renderer", Icon = "\xf5ee")]
    public class TetrahedronRendererComponent : PrimitiveRenderComponent
    {
        private float size = 1;

        [EditorProperty("Size", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Size { get => size; set => SetAndUpdateModelEquals(ref size, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new Tetrahedron(new(size));
        }

        protected override BoundingBox GetBoundingBox()
        {
            float radius = size * 0.5f;
            return new BoundingBox(new Vector3(-radius), new Vector3(radius));
        }
    }

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(OctahedronRendererComponent), "Octahedron Renderer", Icon = "\xf5ee")]
    public class OctahedronRendererComponent : PrimitiveRenderComponent
    {
        private float size = 1;

        [EditorProperty("Size", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Size { get => size; set => SetAndUpdateModelEquals(ref size, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new Octahedron(new(size));
        }

        protected override BoundingBox GetBoundingBox()
        {
            float radius = size * 0.5f;
            return new BoundingBox(new Vector3(-radius), new Vector3(radius));
        }
    }

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(CylinderRendererComponent), "Cylinder Renderer", Icon = "\xf5ee")]
    public class CylinderRendererComponent : PrimitiveRenderComponent
    {
        private float height = 1;
        private float diameter = 1;
        private uint tessellation = 16;

        [EditorProperty("Height", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Height { get => height; set => SetAndUpdateModelEquals(ref height, value); }

        [EditorProperty("Diameter", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Diameter { get => diameter; set => SetAndUpdateModelEquals(ref diameter, value); }

        [EditorProperty("Tessellation", 3u, 128u, EditorPropertyMode.Default)]
        public uint Tessellation { get => tessellation; set => SetAndUpdateModelEquals(ref tessellation, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new Cylinder(new(height, diameter, tessellation));
        }

        protected override BoundingBox GetBoundingBox()
        {
            float radius = diameter * 0.5f;
            float halfHeight = height * 0.5f;
            return new BoundingBox(new Vector3(-radius, -halfHeight, -radius), new Vector3(radius, halfHeight, radius));
        }
    }

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(CapsuleRendererComponent), "Capsule Renderer", Icon = "\xf5ee")]
    public class CapsuleRendererComponent : PrimitiveRenderComponent
    {
        private float height = 1;
        private float diameter = 1;
        private uint tessellation = 16;

        [EditorProperty("Height", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Height { get => height; set => SetAndUpdateModelEquals(ref height, value); }

        [EditorProperty("Diameter", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Diameter { get => diameter; set => SetAndUpdateModelEquals(ref diameter, value); }

        [EditorProperty("Tessellation", 3u, 128u, EditorPropertyMode.Default)]
        public uint Tessellation { get => tessellation; set => SetAndUpdateModelEquals(ref tessellation, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new Capsule(new(height, diameter, Math.Max(tessellation, 3)));
        }

        protected override BoundingBox GetBoundingBox()
        {
            float radius = diameter * 0.5f;
            float halfHeight = height * 0.5f + radius;
            return new BoundingBox(new Vector3(-radius, -halfHeight, -radius), new Vector3(radius, halfHeight, radius));
        }
    }

    [EditorCategory("Primitives")]
    [EditorComponent(typeof(TorusRendererComponent), "Torus Renderer", Icon = "\xf5ee")]
    public class TorusRendererComponent : PrimitiveRenderComponent
    {
        private float diameter = 1;
        private float thickness = 1;
        private uint tessellation = 16;

        [EditorProperty("Diameter", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Diameter { get => diameter; set => SetAndUpdateModelEquals(ref diameter, value); }

        [EditorProperty("Thickness", 0f, float.MaxValue, EditorPropertyMode.Default)]
        public float Thickness { get => thickness; set => SetAndUpdateModelEquals(ref thickness, value); }

        [EditorProperty("Tessellation", 3u, 128u, EditorPropertyMode.Default)]
        public uint Tessellation { get => tessellation; set => SetAndUpdateModelEquals(ref tessellation, value); }

        protected override IPrimitive CreatePrimitive()
        {
            return new Torus(new(diameter, thickness, Math.Max(tessellation, 3)));
        }

        protected override BoundingBox GetBoundingBox()
        {
            float radius = diameter * 0.5f;
            float halfHeight = thickness * 0.5f + radius;
            return new BoundingBox(new Vector3(-radius, -halfHeight, -radius), new Vector3(radius, halfHeight, radius));
        }
    }
}