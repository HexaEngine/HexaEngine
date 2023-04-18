namespace TestApp
{
    using System.Diagnostics;

    public static class Program
    {
        public static async Task Main()
        {
            ProcessStartInfo psi = new("cmd.exe");
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            var process = Process.Start(psi);
            var so = process.StandardOutput;
            var si = process.StandardInput;
            var se = process.StandardError;

            bool running = true;
            Task outputtask = Task.Factory.StartNew(async () =>
            {
                char[] buffer = new char[1024];
                while (running)
                {
                    var read = await so.ReadAsync(buffer, 0, 1024);
                    Console.Write(new string(buffer.AsSpan(0, read)));
                }
            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            Task errortask = Task.Factory.StartNew(async () =>
            {
                char[] buffer = new char[1024];
                while (running)
                {
                    Thread.Sleep(1000);
                    //var read = await so.ReadAsync(buffer, 0, 1024);
                    //Console.Write(new string(buffer.AsSpan(0, read)));
                }
            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            while (running)
            {
                var input = Console.Read();

                await si.WriteAsync((char)input);
            }

            outputtask.Wait();
            errortask.Wait();
        }
    }
}