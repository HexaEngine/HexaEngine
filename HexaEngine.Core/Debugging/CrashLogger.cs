﻿namespace HexaEngine.Core.Debugging
{
    using Hardware.Info;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class CrashLogger
    {
        public static readonly FileLogWriter FileLogWriter = new($"logs/app-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.log");
        private static HardwareInfo info;
        private static Task task;

        public static void Initialize()
        {
            info = new HardwareInfo();
            task = new(info.RefreshAll);
            task.Start();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logger.Writers.Add(FileLogWriter);
        }

        private static string Humanize(ulong s)
        {
            if (s > 1099511627776)
                return $"{s / 1099511627776f}TiB";
            if (s > 1073741824)
                return $"{s / 1073741824f}GiB";
            if (s > 1048576)
                return $"{s / 1048576f}MiB";
            if (s > 1024)
                return $"{s / 1024f}KiB";
            return $"{s}B";
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Logger.Close();
                var exception = (Exception)e.ExceptionObject;

                task.Wait();

                StringBuilder sb = new();
                sb.AppendLine($"HexaEngine {Assembly.GetExecutingAssembly().GetName().Version}");
                sb.AppendLine($"Runtime: .Net {Environment.Version}");
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine($"Unhandled exception {exception.HResult} {exception.Message} at {exception.TargetSite}");
                sb.AppendLine($"\t{Marshal.GetExceptionForHR(exception.HResult)?.Message}");
                sb.AppendLine();

                sb.AppendLine("User Info");
                sb.AppendLine($"\tUsername: {Environment.UserName}");
                sb.AppendLine($"\tUser Domain: {Environment.UserDomainName}");
                sb.AppendLine();

                sb.AppendLine("System Info:");
                sb.AppendLine($"\tOS: {info.OperatingSystem.Name} {info.OperatingSystem.VersionString}");
                sb.AppendLine($"\tCPU: {info.CpuList[0].Manufacturer} {info.CpuList[0].Name}");
                sb.AppendLine($"\tRAM Physical: {Humanize(info.MemoryStatus.TotalPhysical - info.MemoryStatus.AvailablePhysical)}/{Humanize(info.MemoryStatus.TotalPhysical)}");
                sb.AppendLine($"\tRAM Virtual: {Humanize(info.MemoryStatus.TotalVirtual - info.MemoryStatus.AvailableVirtual)}/{Humanize(info.MemoryStatus.TotalVirtual)}");
                if (GraphicsAdapter.Current != null)
                {
                    sb.AppendLine($"\tGraphics API: {GraphicsAdapter.Current.Backend}");
                    for (int i = 0; i < GraphicsAdapter.Current.GPUs.Count; i++)
                    {
                        var gpu = GraphicsAdapter.Current.GPUs[i];
                        sb.AppendLine($"\tGPU{i}: {gpu.Description}, DeviceId: {gpu.DeviceId}, VendorId: {gpu.VendorId}, Rev: {gpu.Revision}, DedicatedVideoMem: {Humanize(gpu.DedicatedVideoMemory)}, DedicatedSystemMem: {Humanize(gpu.DedicatedSystemMemory)}, SharedSystemMem: {Humanize(gpu.SharedSystemMemory)}");
                    }
                }
                else
                {
                    sb.AppendLine("\tGraphics not yet loaded!");
                }

                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("Callstack:");
                sb.AppendLine(exception.StackTrace.Replace(Environment.NewLine, "\n\t"));

                var fileInfo = new FileInfo($"logs/crash-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.log");
                fileInfo.Directory?.Create();
                File.AppendAllText(fileInfo.FullName, sb.ToString());
                ProcessStartInfo psi = new("CrashReporter.exe", fileInfo.FullName);
                psi.UseShellExecute = true;

                Process.Start(psi);
            }
        }
    }
}