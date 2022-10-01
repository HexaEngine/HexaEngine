namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Logging;
    using HexaEngine.Editor;
    using System.Diagnostics;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            Trace.Listeners.Add(new DebugListener("output.log"));

            Application.Run(new EditorWindow());
        }
    }
}