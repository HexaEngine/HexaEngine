namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;

    public class FileLogWriter : ILogWriter
    {
        private readonly BufferedStream stream;
        private readonly ConcurrentQueue<string> queue = new();
        private readonly Task logWriterTask;
        private bool running = true;
        private bool disposedValue;

        public FileLogWriter(string file)
        {
            var fileInfo = new FileInfo(file);
            fileInfo.Directory?.Create();
            stream = new(File.Create(file));
            logWriterTask = new(LogWriterTaskVoid, TaskCreationOptions.LongRunning);
            logWriterTask.Start();
        }

        private void LogWriterTaskVoid()
        {
            while (running)
            {
                while (queue.TryDequeue(out var message))
                {
                    stream.Write(Encoding.UTF8.GetBytes(message));
                }

                Thread.Sleep(1);
            }
        }

        public void Clear()
        {
            stream.Position = 0;
            stream.SetLength(0);
        }

        public void Flush()
        {
            stream.Flush();
        }

        public void Write(string message)
        {
            queue.Enqueue(message);
        }

        public Task WriteAsync(string message)
        {
            queue.Enqueue(message);
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                running = false;
                logWriterTask.Wait();
                logWriterTask.Dispose();
                stream.Dispose();

                disposedValue = true;
            }
        }

        ~FileLogWriter()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}