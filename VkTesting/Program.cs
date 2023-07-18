namespace VkTesting
{
    using HexaEngine.Core;
    using VkTesting.Windows;

    public class Program
    {
        public static void Main(string[] args)
        {
            SdlWindow window = new();
            Application.Run(window);
        }
    }
}