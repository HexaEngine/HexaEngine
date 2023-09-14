namespace D3D12Testing
{
    using D3D12Testing.Windows;

    public class Program
    {
        public static void Main(string[] args)
        {
            SdlWindow window = new();
            Application.Run(window);
        }
    }
}