namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class CrashLogger
    {
        private static StreamWriter? writer;

        public static void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            writer = new(File.Create("app.log"));
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            writer?.Close();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            writer?.WriteLine(e.ExceptionObject.ToString());
            writer?.Close();
        }
    }
}