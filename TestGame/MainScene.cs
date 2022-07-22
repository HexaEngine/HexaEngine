namespace TestGame
{
    using HexaEngine.Lights;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;

    public class MainScene : Scene
    {
        public MainScene()
        {
            /*
            AddMaterial(new()
            {
                Name = "Sphere",
                Albedo = new(1, 0, 0),
                Metalness = 0,
                Roughness = 0.2f,
                Ao = 1,
            });
            AddMaterial(new()
            {
                Name = "Box1",
                Albedo = new(0, 1, 0),
                Metalness = 0,
                Roughness = 0.2f,
                Ao = 1,
            });
            AddMaterial(new()
            {
                Name = "Box2",
                Albedo = new(1, 0, 0),
                Metalness = 0,
                Roughness = 0.2f,
                Ao = 1,
            });
            Mesh sphereMesh = Mesh.LoadFromBin("output.msh");
            sphereMesh.MaterialName = "Sphere";
            AddMesh(sphereMesh);

            SceneNode node = new() { Name = "Sphere" };
            node.AddMesh(sphereMesh);
            AddChild(node);

            Mesh boxMesh = Mesh.LoadFromBin("box.msh");
            boxMesh.MaterialName = "Box1";
            AddMesh(boxMesh);

            SceneNode node1 = new() { Name = "Box 1" };
            node1.AddMesh(boxMesh);
            node1.Transform.Position = new(0, -2, 0);
            AddChild(node1);

            Mesh box1Mesh = boxMesh.Clone();
            box1Mesh.MaterialName = "Box2";
            AddMesh(box1Mesh);

            SceneNode node2 = new() { Name = "Box 2" };
            node2.AddMesh(box1Mesh);
            node2.Transform.Position = new(0, 0, 6);
            node2.Transform.Rotation = new(0, 90, 0);
            AddChild(node2);

            DirectionalLight light = new() { Name = "Sun" };
            light.Transform.Position = new(0, 5, -5);
            light.Transform.Rotation = new(0, 45, 0);
            light.Color = new(1, 1, 1, 1);
            AddChild(light);
            */
        }
    }
}