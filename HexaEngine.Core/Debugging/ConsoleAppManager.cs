namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Manages an external console application and provides communication capabilities.
    /// </summary>
    public class ConsoleAppManager
    {
        private readonly string appName;
        private readonly Process process = new();
        private readonly object lockObject = new();
        private string? pendingWriteData;

        private Task outputTask;
        private Task errorTask;
        private Task inputTask;
        private AutoResetEvent inputSignal = new(false);
#nullable disable
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleAppManager"/> class with the specified application name.
        /// </summary>
        /// <param name="appName">The path or name of the external console application to manage.</param>
        public ConsoleAppManager(string appName)
        {

            this.appName = appName;

            process.StartInfo = new(this.appName);
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.EnableRaisingEvents = true;
            process.Exited += ProcessOnExited;
        }
#nullable restore

        /// <summary>
        /// Occurs when an error message is received from the managed console application.
        /// </summary>
        public event EventHandler<string>? ErrorTextReceived;

        /// <summary>
        /// Occurs when the managed console application exits.
        /// </summary>
        public event EventHandler? ProcessExited;

        /// <summary>
        /// Occurs when standard output text is received from the managed console application.
        /// </summary>
        public event EventHandler<string>? StandardTextReceived;

        /// <summary>
        /// Gets the exit code of the managed console application.
        /// </summary>
        public int ExitCode
        {
            get { return process.ExitCode; }
        }

        /// <summary>
        /// Gets a value indicating whether the managed console application is currently running.
        /// </summary>
        public bool Running
        {
            get; private set;
        }

        /// <summary>
        /// Asynchronously executes the managed console application with the specified command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to pass to the console application.</param>
        /// <exception cref="InvalidOperationException">Thrown if the process is still running.</exception>
        public void ExecuteAsync(params string[] args)
        {
            if (Running)
            {
                throw new InvalidOperationException(
                    "Process is still Running. Please wait for the process to complete.");
            }

            string arguments = string.Join(" ", args);

            process.StartInfo.Arguments = arguments;

            process.Start();
            process.StandardInput.AutoFlush = true;
            Running = true;

            outputTask = Task.Factory.StartNew(ReadOutputAsync, default, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            inputTask = Task.Factory.StartNew(WriteInputTask, default, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            errorTask = Task.Factory.StartNew(ReadOutputErrorAsync, default, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Writes a string to the standard input of the managed console application.
        /// </summary>
        /// <param name="data">The string to write to the standard input.</param>
        public void Write(string data)
        {
            if (data == null)
            {
                return;
            }

            lock (lockObject)
            {
                pendingWriteData = data;
            }
            inputSignal.Set();
        }

        /// <summary>
        /// Writes a character to the standard input of the managed console application.
        /// </summary>
        /// <param name="data">The character to write to the standard input.</param>
        public void Write(char data)
        {
            lock (lockObject)
            {
                pendingWriteData = new(data, 1);
            }
            inputSignal.Set();
        }

        /// <summary>
        /// Writes a line of text to the standard input of the managed console application.
        /// </summary>
        /// <param name="data">The line of text to write, followed by a newline character.</param>
        public void WriteLine(string data)
        {
            Write(data + Environment.NewLine);
        }

        /// <summary>
        /// Called when an error message is received from the managed console application.
        /// </summary>
        /// <param name="e">The error message received.</param>
        protected virtual void OnErrorTextReceived(string e)
        {
            ErrorTextReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the managed console application exits.
        /// </summary>
        protected virtual void OnProcessExited()
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when standard output text is received from the managed console application.
        /// </summary>
        /// <param name="e">The standard output text received.</param>
        protected virtual void OnStandardTextReceived(string e)
        {
            StandardTextReceived?.Invoke(this, e);
        }

        private void ProcessOnExited(object? sender, EventArgs eventArgs)
        {
            OnProcessExited();
        }

        private async void ReadOutputAsync()
        {
            var buffer = new char[1024];

            while (process.HasExited == false)
            {
                int length = await process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                var str = new string(buffer.AsSpan(0, length));
                OnStandardTextReceived(str);
                Thread.Sleep(1);
            }

            Running = false;
        }

        private async void ReadOutputErrorAsync()
        {
            var buffer = new char[1024];

            do
            {
                int length = await process.StandardError.ReadAsync(buffer, 0, buffer.Length);
                var str = new string(buffer.AsSpan(0, length));

                OnErrorTextReceived(str);
                Thread.Sleep(1);
            }
            while (process.HasExited == false);
        }

        private async void WriteInputTask()
        {
            while (process.HasExited == false)
            {
                inputSignal.WaitOne();
                if (pendingWriteData != null)
                {
                    await process.StandardInput.WriteAsync(pendingWriteData);

                    lock (lockObject)
                    {
                        pendingWriteData = null;
                    }
                }

                Thread.Sleep(1);
            }
        }
    }
}