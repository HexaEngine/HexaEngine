namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Diagnostics;

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

        public event EventHandler<string>? ErrorTextReceived;

        public event EventHandler? ProcessExited;

        public event EventHandler<string>? StandardTextReceived;

        public int ExitCode
        {
            get { return process.ExitCode; }
        }

        public bool Running
        {
            get; private set;
        }

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
        }

        public void Write(char data)
        {
            lock (lockObject)
            {
                pendingWriteData = new(data, 1);
                inputSignal.Set();
            }
        }

        public void WriteLine(string data)
        {
            Write(data + Environment.NewLine);
        }

        protected virtual void OnErrorTextReceived(string e)
        {
            ErrorTextReceived?.Invoke(this, e);
        }

        protected virtual void OnProcessExited()
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }

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