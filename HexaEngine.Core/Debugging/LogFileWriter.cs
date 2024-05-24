namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// A log writer that appends log messages to a file.
    /// </summary>
    public class LogFileWriter : ILogWriter
    {
        private const int MaxLogFileCount = 5;
        private readonly BufferedStream stream;
        private readonly ConcurrentQueue<string> queue = new();
        private readonly Task logWriterTask;
        private bool running = true;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileWriter"/> class and creates or opens the specified log file.
        /// </summary>
        /// <param name="folder">The path to the log folder.</param>
        public LogFileWriter(string folder)
        {
            string fileName = $"app-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.log";
            string file = Path.Combine(folder, fileName);

            EnsureFolderExists(folder);
            CompressOldLogFiles(folder);
            RemoveOldLogFiles(folder);

            int i = 0;
            while (File.Exists(file))
            {
                file = Path.Combine(folder, $"fileName-{i++}");
            }

            var fileInfo = new FileInfo(file);

            fileInfo.Directory?.Create();
            stream = new(File.Create(file));
            logWriterTask = new(LogWriterTaskVoid, TaskCreationOptions.LongRunning);
            logWriterTask.Start();
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private static void CompressOldLogFiles(string folder)
        {
            foreach (var logFile in Directory.EnumerateFiles(folder, "app-*.log"))
            {
                using FileStream fs = File.Create(Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(logFile)}.zip"));
                using ZipArchive archive = new(fs, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                File.Delete(logFile);
            }
        }

        private static void RemoveOldLogFiles(string folder)
        {
            var logZipFiles = Directory.EnumerateFiles(folder, "app-*.zip").OrderByDescending(f => new FileInfo(f).CreationTime).ToArray();

            foreach (var file in logZipFiles.Skip(MaxLogFileCount - 1))
            {
                File.Delete(file);
            }
        }

        // The method that runs in the background task to write log messages
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

        /// <summary>
        /// Clears the contents of the log file.
        /// </summary>
        public void Clear()
        {
            stream.Position = 0;
            stream.SetLength(0);
        }

        /// <summary>
        /// Flushes any buffered data to the log file.
        /// </summary>
        public void Flush()
        {
            stream.Flush();
        }

        /// <summary>
        /// Writes a log message to the log file.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        public void Write(string message)
        {
            queue.Enqueue(message);
        }

        /// <summary>
        /// Writes a log message to the log file asynchronously.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task WriteAsync(string message)
        {
            queue.Enqueue(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Releases the resources used by the <see cref="LogFileWriter"/> instance.
        /// </summary>
        /// <param name="disposing">True if the method is called directly, false if called by the finalizer.</param>
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

        /// <summary>
        /// Releases the resources used by the <see cref="LogFileWriter"/> instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}