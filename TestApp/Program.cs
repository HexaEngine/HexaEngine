namespace TestApp
{
    using HexaEngine.Editor.Packaging;
    using HexaEngine.Editor.Projects;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            HexaProject project = HexaProject.CreateNew();

            ItemGroup itemGroup = [new PackageReference("HexaEngine.Core", "1.0.0.0")];
            project.Items.Add(itemGroup);

            HexaProjectWriter writer = new("Project.hexaproj");
            writer.Write(project);
            writer.Close();
        }
    }
}