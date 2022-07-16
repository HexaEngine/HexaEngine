namespace HexaEngine.Core.Logging
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class DebugListener : TraceListener
    {
        private readonly BufferedStream stream;

        public DebugListener(string file)
        {
#if !RELEASE
            var fileInfo = new FileInfo(file);
            fileInfo.Directory?.Create();
            stream = new(File.Create(file));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
#endif
        }

#if !RELEASE

        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            stream.Flush();
            stream.Close();
        }

        private void Application_ApplicationClosing(object sender, EventArgs e)
        {
            stream.Flush();
            stream.Close();
        }

#endif

        public override void Write(string? message)
        {
            if (message == null)
                return;
#if !RELEASE
            stream.Write(Encoding.UTF8.GetBytes(message));
#endif
        }

        public override void WriteLine(string? message)
        {
            if (message == null)
                return;
#if !RELEASE
            stream.Write(Encoding.UTF8.GetBytes(message + "\n"));
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if !RELEASE
            WriteLine(e.ExceptionObject);
            if (e.IsTerminating)
            {
                stream.Flush();
                stream.Close();
            }
#endif
        }
    }
}