namespace TestApp
{
    using HexaEngine.Editor.Packaging;
    using HexaEngine.Editor.Projects;
    using System.Numerics;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            Vector3 scale = new(2, 1, 1);
            Vector3 scaleSrc = new(1);
            scaleSrc = new(scale.X != scaleSrc.X ? scale.X : scale.Y != scaleSrc.Y ? scale.Y : scale.Z);
        }
    }
}