namespace HexaEngine.Plugins2
{
    using HexaEngine.Cameras;
    using HexaEngine.Lights;
    using HexaEngine.Plugins2.Records;
    using HexaEngine.Plugins2.Resources;
    using HexaEngine.Scenes;

    public class PluginFactory
    {
        public static Plugin Convert(string name, string description, Scene scene)
        {
            Plugin plugin = new(name, description);

            // Resources;
            for (int i = 0; i < scene.Materials.Count; i++)
            {
                Resource resource = new MaterialResource(scene.Materials[i]);
            }

            // Nodes;
            for (int i = 0; i < scene.Nodes.Count; i++)
            {
                var node = scene.Nodes[i];
                if (node is Light light)
                {
                    continue;
                }
                if (node is Camera camera)
                {
                    continue;
                }

                plugin.Records.Add(new NodeRecord(node));
            }

            return plugin;
        }
    }
}