namespace TestGame
{
    using HexaEngine.Lights;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;

    public class MainScene : Scene
    {
        public MainScene()
        {
            AddMaterial(new()
            {
                Name = "Sphere",
                Color = new(1, 0, 0),
                Metalness = 0,
                Roughness = 0.2f,
                Ao = 1,
            });
            AddMaterial(new()
            {
                Name = "Box1",
                Color = new(0, 1, 0),
                Metalness = 0,
                Roughness = 0.2f,
                Ao = 1,
            });
            AddMaterial(new()
            {
                Name = "Box2",
                Color = new(1, 0, 0),
                Metalness = 0,
                Roughness = 0.2f,
                Ao = 1,
            });
            Mesh sphere = new()
            {
                MeshPath = "output.msh",
                Name = "Sphere",
                MaterialName = "Sphere"
            };

            AddChild(sphere);

            Mesh element2 = new() { Name = "Box 1", MaterialName = "Box1", MeshPath = "box.msh" };
            element2.Transform.Position = new(0, -2, 0);

            AddChild(element2);

            Mesh element3 = new() { Name = "Box 2", MaterialName = "Box2", MeshPath = "box.msh" };
            element3.Transform.Position = new(0, 0, 6);
            element3.Transform.Rotation = new(0, 90, 0);

            AddChild(element3);

            DirectionalLight light = new() { Name = "Sun" };
            light.Transform.Position = new(0, 5, -5);
            light.Transform.Rotation = new(0, 45, 0);
            light.Color = new(1, 1, 1, 1);
            light.CastShadows = true;
            AddChild(light);
        }
    }
}