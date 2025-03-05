namespace HexaEngine.Core.Logging
{
    using Hardware.Info;
    using Hexa.NET.Logging;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A class responsible for handling unhandled exceptions and logging crash information.
    /// </summary>
    public static class CrashLogger
    {
#nullable disable
        private static HardwareInfo info;
        private static Task task;
#nullable restore

        private static readonly ILogger logger = LoggerFactory.GetLogger(nameof(CrashLogger));

        /// <summary>
        /// Initializes the CrashLogger, setting up crash handling and logging.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                info = new HardwareInfo();
                task = new(info.RefreshAll);
                task.Start();
            }
            catch (Exception ex)
            {
                logger.Log(ex);
            }
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            LoggerFactory.CloseAll();
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                LoggerFactory.CloseAll();
                var exception = (Exception)e.ExceptionObject;

                task?.Wait();

                StringBuilder sb = new();
                sb.AppendLine($"HexaEngine {Assembly.GetExecutingAssembly().GetName().Version}");
                sb.AppendLine($"Runtime: .Net {Environment.Version}");
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine($"Unhandled exception {exception.HResult} {exception.Message} at {exception.TargetSite}");
                sb.AppendLine($"\t{Marshal.GetExceptionForHR(exception.HResult)?.Message}");
                sb.AppendLine();

                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("Callstack:");
                sb.AppendLine(exception.StackTrace?.Replace(Environment.NewLine, "\n\t"));
                sb.AppendLine();
                sb.AppendLine();

                if (info != null)
                {
                    sb.AppendLine("System Info:");
                    sb.AppendLine($"\tOS: {info.OperatingSystem.Name} {info.OperatingSystem.VersionString}");

                    sb.AppendLine($"\tCPUs:");
                    for (int i = 0; i < info.CpuList.Count; i++)
                    {
                        CPU cpu = info.CpuList[i];
                        sb.AppendLine($"\t\tCPU {i}:");
                        sb.AppendLine($"\t\t\tCaption: {cpu.Caption}");
                        sb.AppendLine($"\t\t\tCurrent Clock Speed: {cpu.CurrentClockSpeed}MHz");
                        sb.AppendLine($"\t\t\tDescription: {cpu.Description}");
                        sb.AppendLine($"\t\t\tL1 Instruction Cache Size: {cpu.L1InstructionCacheSize.FormatDataSize()}");
                        sb.AppendLine($"\t\t\tL1 Data Cache Size: {cpu.L1DataCacheSize.FormatDataSize()}");
                        sb.AppendLine($"\t\t\tL2 Cache Size: {cpu.L2CacheSize.FormatDataSize()}");
                        sb.AppendLine($"\t\t\tL3 Cache Size: {cpu.L3CacheSize.FormatDataSize()}");
                        sb.AppendLine($"\t\t\tManufacturer: {cpu.Manufacturer}");
                        sb.AppendLine($"\t\t\tMax Clock Speed: {cpu.MaxClockSpeed}MHz");
                        sb.AppendLine($"\t\t\tName: {cpu.Name}");
                        sb.AppendLine($"\t\t\tNumber Of Cores: {cpu.NumberOfCores}");
                        sb.AppendLine($"\t\t\tNumber Of Logical Processors: {cpu.NumberOfLogicalProcessors}");
                        sb.AppendLine($"\t\t\tSecond Level Address Translation Extensions: {cpu.SecondLevelAddressTranslationExtensions}");
                        sb.AppendLine($"\t\t\tSocket Designation: {cpu.SocketDesignation}");
                        sb.AppendLine($"\t\t\tVirtualization Firmware Enabled: {cpu.VirtualizationFirmwareEnabled}");
                        sb.AppendLine($"\t\t\tVM Monitor Mode Extensions: {cpu.VMMonitorModeExtensions}");
                        sb.AppendLine($"\t\t\tProcessor Time: {cpu.PercentProcessorTime}%");
                        sb.AppendLine($"\t\t\tCPU Cores:");
                        for (int j = 0; j < cpu.CpuCoreList.Count; j++)
                        {
                            CpuCore core = cpu.CpuCoreList[j];
                            sb.AppendLine($"\t\t\t\tCPU Core {j}:");
                            sb.AppendLine($"\t\t\t\t\tName {core.Name}");
                            sb.AppendLine($"\t\t\t\t\tProcessor Time {core.PercentProcessorTime}%");
                        }
                    }

                    sb.AppendLine($"\tRAM Physical: {(info.MemoryStatus.TotalPhysical - info.MemoryStatus.AvailablePhysical).FormatDataSize()}/{info.MemoryStatus.TotalPhysical.FormatDataSize()}");
                    sb.AppendLine($"\tRAM Virtual: {(info.MemoryStatus.TotalVirtual - info.MemoryStatus.AvailableVirtual).FormatDataSize()}/{info.MemoryStatus.TotalVirtual.FormatDataSize()}");

                    sb.AppendLine();

                    if (GraphicsAdapter.Current != null)
                    {
                        sb.AppendLine($"\tGraphics API: {GraphicsAdapter.Current.Backend}");

                        var adapterIndex = GraphicsAdapter.Current.AdapterIndex;

                        sb.AppendLine("\tGPUs (API):");
                        for (int i = 0; i < GraphicsAdapter.Current.GPUs.Count; i++)
                        {
                            var gpu = GraphicsAdapter.Current.GPUs[i];

                            sb.AppendLine($"\t\tGPU {i}:");

                            sb.AppendLine($"\t\t\tDescription: {gpu.Description}");
                            sb.AppendLine($"\t\t\tVendor Id: {gpu.VendorId}");
                            sb.AppendLine($"\t\t\tDevice Id: {gpu.DeviceId}");
                            sb.AppendLine($"\t\t\tSub Sys Id: {gpu.SubSysId}");
                            sb.AppendLine($"\t\t\tRev: {gpu.Revision}");

                            if (adapterIndex == i)
                            {
                                sb.AppendLine($"\t\t\tMemory Budget: {GraphicsAdapter.Current.GetMemoryBudget().FormatDataSize()}");
                                sb.AppendLine($"\t\t\tMemory Current Usage: {GraphicsAdapter.Current.GetMemoryCurrentUsage().FormatDataSize()}");
                                sb.AppendLine($"\t\t\tMemory Available For Reservation: {GraphicsAdapter.Current.GetMemoryAvailableForReservation().FormatDataSize()}");
                                sb.AppendLine($"\t\t\tDedicated Video Mem: {gpu.DedicatedVideoMemory.FormatDataSize()}");
                                sb.AppendLine($"\t\t\tDedicated System Mem: {gpu.DedicatedSystemMemory.FormatDataSize()}");
                                sb.AppendLine($"\t\t\tShared System Mem: {gpu.SharedSystemMemory.FormatDataSize()}");
                            }
                            else
                            {
                                sb.AppendLine($"\t\t\tDedicated Video Mem: {gpu.DedicatedVideoMemory.FormatDataSize()}");
                                sb.AppendLine($"\t\t\tDedicated System Mem: {gpu.DedicatedSystemMemory.FormatDataSize()}");
                                sb.AppendLine($"\t\t\tShared System Mem: {gpu.SharedSystemMemory.FormatDataSize()}");
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine("\tGraphics not yet loaded!");
                    }

                    sb.AppendLine("\tVideo Controllers:");
                    for (int i = 0; i < info.VideoControllerList.Count; i++)
                    {
                        VideoController video = info.VideoControllerList[i];

                        sb.AppendLine($"\t\tVideo Controller {i}:");

                        sb.AppendLine($"\t\t\tAdapter RAM: {video.AdapterRAM.FormatDataSize()}");
                        sb.AppendLine($"\t\t\tCaption: {video.Caption}");
                        sb.AppendLine($"\t\t\tCurrent Bits Per Pixel: {video.CurrentBitsPerPixel}");
                        sb.AppendLine($"\t\t\tCurrent Horizontal Resolution: {video.CurrentHorizontalResolution}");
                        sb.AppendLine($"\t\t\tCurrent Number Of Colors: {video.CurrentNumberOfColors}");
                        sb.AppendLine($"\t\t\tCurrent Refresh Rate: {video.CurrentRefreshRate}");
                        sb.AppendLine($"\t\t\tCurrent Vertical Resolution: {video.CurrentVerticalResolution}");
                        sb.AppendLine($"\t\t\tDescription: {video.Description}");
                        sb.AppendLine($"\t\t\tDriver Date: {video.DriverDate}");
                        sb.AppendLine($"\t\t\tDriver Version: {video.DriverVersion}");
                        sb.AppendLine($"\t\t\tManufacturer: {video.Manufacturer}");
                        sb.AppendLine($"\t\t\tMax Refresh Rate: {video.MaxRefreshRate}hz");
                        sb.AppendLine($"\t\t\tMin Refresh Rate: {video.MinRefreshRate}hz");
                        sb.AppendLine($"\t\t\tName: {video.Name}");
                        sb.AppendLine($"\t\t\tVideo Mode Description: {video.VideoModeDescription}");
                        sb.AppendLine($"\t\t\tVideo Processor: {video.VideoProcessor}");
                    }

                    sb.AppendLine($"\tMonitors:");

                    for (int i = 0; i < info.MonitorList.Count; i++)
                    {
                        var monitor = info.MonitorList[i];

                        sb.AppendLine($"\t\tMonitor {i}:");

                        sb.AppendLine($"\t\t\tCaption: {monitor.Caption}");
                        sb.AppendLine($"\t\t\tDescription: {monitor.Description}");
                        sb.AppendLine($"\t\t\tMonitor Manufacturer: {monitor.MonitorManufacturer}");
                        sb.AppendLine($"\t\t\tMonitor Type: {monitor.MonitorType}");
                        sb.AppendLine($"\t\t\tName: {monitor.Name}");
                        sb.AppendLine($"\t\t\tPixels Per X Logical Inch: {monitor.PixelsPerXLogicalInch}");
                        sb.AppendLine($"\t\t\tPixels Per Y Logical Inch: {monitor.PixelsPerYLogicalInch}");
                        sb.AppendLine($"\t\t\tActive: {monitor.Active}");
                        sb.AppendLine($"\t\t\tManufacturer Name: {monitor.ManufacturerName}");
                        sb.AppendLine($"\t\t\tProduct Code ID: {monitor.ProductCodeID}");
                        sb.AppendLine($"\t\t\tUser Friendly Name: {monitor.UserFriendlyName}");
                        sb.AppendLine($"\t\t\tWeek Of Manufacture: {monitor.WeekOfManufacture}");
                        sb.AppendLine($"\t\t\tYear Of Manufacture: {monitor.YearOfManufacture}");
                    }

                    if (AudioAdapter.Current != null)
                    {
                        sb.AppendLine($"\tAudio API: {AudioAdapter.Current.Backend}");
                        var devices = AudioAdapter.Current.GetAvailableDevices();
                        sb.AppendLine("\tAudio Devices (API):");
                        for (int i = 0; i < devices.Count; i++)
                        {
                            var device = devices[i];

                            sb.AppendLine($"\t\tAudio Device {i}:");

                            sb.AppendLine($"\t\t\tDescription: {device}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("\tAudio not yet loaded!");
                    }

                    sb.AppendLine("\tSound Devices:");
                    for (int i = 0; i < info.SoundDeviceList.Count; i++)
                    {
                        SoundDevice sound = info.SoundDeviceList[i];

                        sb.AppendLine($"\t\tSound Device {i}:");

                        sb.AppendLine($"\t\t\tCaption: {sound.Caption}");
                        sb.AppendLine($"\t\t\tDescription: {sound.Description}");
                        sb.AppendLine($"\t\t\tManufacturer: {sound.Manufacturer}");
                        sb.AppendLine($"\t\t\tName: {sound.Name}");
                        sb.AppendLine($"\t\t\tProduct Name: {sound.ProductName}");
                    }

                    sb.AppendLine("\tNetwork Adapters:");
                    for (int i = 0; i < info.NetworkAdapterList.Count; i++)
                    {
                        NetworkAdapter network = info.NetworkAdapterList[i];

                        sb.AppendLine($"\t\tNetwork Adapter {i}:");

                        sb.AppendLine($"\t\t\tAdapter Type: {network.AdapterType}");
                        sb.AppendLine($"\t\t\tCaption: {network.Caption}");
                        sb.AppendLine($"\t\t\tDescription: {network.Description}");
                        sb.AppendLine($"\t\t\tName: {network.Name}");
                        sb.AppendLine($"\t\t\tNet Connection ID: {network.NetConnectionID}");
                        sb.AppendLine($"\t\t\tProduct Name: {network.ProductName}");
                        sb.AppendLine($"\t\t\tSpeed: {network.Speed}");
                        sb.AppendLine($"\t\t\tBytes Sent Per sec: {network.BytesSentPersec}");
                        sb.AppendLine($"\t\t\tBytes Received Per sec: {network.BytesReceivedPersec}");
                    }

                    sb.AppendLine($"\tDrives:");
                    for (int i = 0; i < info.DriveList.Count; i++)
                    {
                        Drive drive = info.DriveList[i];
                        sb.AppendLine($"\t\tDrive {i}:");

                        sb.AppendLine($"\t\t\tCaption: {drive.Caption}");
                        sb.AppendLine($"\t\t\tDescription: {drive.Description}");
                        sb.AppendLine($"\t\t\tFirmware Revision: {drive.FirmwareRevision}");
                        sb.AppendLine($"\t\t\tIndex: {drive.Index}");
                        sb.AppendLine($"\t\t\tManufacturer: {drive.Manufacturer}");
                        sb.AppendLine($"\t\t\tModel: {drive.Model}");
                        sb.AppendLine($"\t\t\tName: {drive.Name}");
                        sb.AppendLine($"\t\t\tPartitions: {drive.Partitions}");
                        sb.AppendLine($"\t\t\tSize: {drive.Size.FormatDataSize()}");

                        sb.AppendLine($"\t\t\tPartitions:");
                        for (int j = 0; j < drive.PartitionList.Count; j++)
                        {
                            Partition partition = drive.PartitionList[j];
                            sb.AppendLine($"\t\t\t\tPartition {i}:");
                            sb.AppendLine($"\t\t\t\t\tBootable: {partition.Bootable}");
                            sb.AppendLine($"\t\t\t\t\tBoot Partition: {partition.BootPartition}");
                            sb.AppendLine($"\t\t\t\t\tCaption: {partition.Caption}");
                            sb.AppendLine($"\t\t\t\t\tDescription: {partition.Description}");
                            sb.AppendLine($"\t\t\t\t\tDisk Index: {partition.DiskIndex}");
                            sb.AppendLine($"\t\t\t\t\tIndex: {partition.Index}");
                            sb.AppendLine($"\t\t\t\t\tName: {partition.Name}");
                            sb.AppendLine($"\t\t\t\t\tPrimary Partition: {partition.PrimaryPartition}");
                            sb.AppendLine($"\t\t\t\t\tSize: {partition.Size.FormatDataSize()}");
                            sb.AppendLine($"\t\t\t\t\tStarting Offset: {partition.StartingOffset}");
                            sb.AppendLine($"\t\t\t\t\tVolumes:");
                            for (int ii = 0; ii < partition.VolumeList.Count; ii++)
                            {
                                Volume volume = partition.VolumeList[ii];
                                sb.AppendLine($"\t\t\t\t\t\tVolume {ii}:");
                                sb.AppendLine($"\t\t\t\t\t\tCaption: {volume.Caption}");
                                sb.AppendLine($"\t\t\t\t\t\tCompressed: {volume.Compressed}");
                                sb.AppendLine($"\t\t\t\t\t\tDescription: {volume.Description}");
                                sb.AppendLine($"\t\t\t\t\t\tFile System: {volume.FileSystem}");
                                sb.AppendLine($"\t\t\t\t\t\tFree Space: {volume.FreeSpace.FormatDataSize()}");
                                sb.AppendLine($"\t\t\t\t\t\tName: {volume.Name}");
                                sb.AppendLine($"\t\t\t\t\t\tSize: {volume.Size.FormatDataSize()}");
                                sb.AppendLine($"\t\t\t\t\t\tVolume Name: {volume.VolumeName}");
                            }
                        }
                    }

                    sb.AppendLine($"\tKeyboards:");
                    for (int i = 0; i < info.KeyboardList.Count; i++)
                    {
                        Keyboard keyboard = info.KeyboardList[i];
                        sb.AppendLine($"\t\tKeyboard {i}:");

                        sb.AppendLine($"\t\t\tCaption: {keyboard.Caption}");
                        sb.AppendLine($"\t\t\tDescription: {keyboard.Description}");
                        sb.AppendLine($"\t\t\tName: {keyboard.Name}");
                        sb.AppendLine($"\t\t\tNumber Of Function Keys: {keyboard.NumberOfFunctionKeys}");
                    }

                    sb.AppendLine($"\tMice:");
                    for (int i = 0; i < info.MouseList.Count; i++)
                    {
                        Mouse mouse = info.MouseList[i];
                        sb.AppendLine($"\t\tMouse {i}:");

                        sb.AppendLine($"\t\t\tCaption: {mouse.Caption}");
                        sb.AppendLine($"\t\t\tDescription: {mouse.Description}");
                        sb.AppendLine($"\t\t\tManufacturer: {mouse.Manufacturer}");
                        sb.AppendLine($"\t\t\tName: {mouse.Name}");
                        sb.AppendLine($"\t\t\tNumber Of Buttons: {mouse.NumberOfButtons}");
                    }
                }

                sb.AppendLine($"\tGamepads:");
                for (int i = 0; i < Input.Gamepads.Controllers.Count; i++)
                {
                    Input.Gamepad gamepad = Input.Gamepads.Controllers[i];
                    sb.AppendLine($"\t\tGamepad {i}:");

                    sb.AppendLine($"\t\t\tName: {gamepad.Name}");
                    sb.AppendLine($"\t\t\tVendor: {gamepad.Vendor}");
                    sb.AppendLine($"\t\t\tProduct: {gamepad.Product}");
                    sb.AppendLine($"\t\t\tProduct Version: {gamepad.ProductVersion}");
                    sb.AppendLine($"\t\t\tIs Attached: {gamepad.IsAttached}");
                    sb.AppendLine($"\t\t\tIs Haptic: {gamepad.IsHaptic}");
                    sb.AppendLine($"\t\t\tHas LED: {gamepad.HasLED}");
                    sb.AppendLine($"\t\t\tType: {gamepad.Type}");
                    sb.AppendLine($"\t\t\tPlayer Index: {gamepad.PlayerIndex}");
                    sb.AppendLine($"\t\t\tMapping: {gamepad.Mapping}");
                }

                sb.AppendLine($"\tJoysticks:");
                for (int i = 0; i < Input.Joysticks.Sticks.Count; i++)
                {
                    Input.Joystick joystick = Input.Joysticks.Sticks[i];
                    sb.AppendLine($"\t\tJoystick {i}:");

                    sb.AppendLine($"\t\t\tName: {joystick.Name}");
                    sb.AppendLine($"\t\t\tVendor: {joystick.Vendor}");
                    sb.AppendLine($"\t\t\tProduct: {joystick.Product}");
                    sb.AppendLine($"\t\t\tProduct Version: {joystick.ProductVersion}");
                    sb.AppendLine($"\t\t\tIs Attached: {joystick.IsAttached}");
                    sb.AppendLine($"\t\t\tIs Virtual: {joystick.IsVirtual}");
                    sb.AppendLine($"\t\t\tHas LED: {joystick.HasLED}");
                    sb.AppendLine($"\t\t\tType: {joystick.Type}");
                    sb.AppendLine($"\t\t\tPlayer Index: {joystick.PlayerIndex}");
                    sb.AppendLine($"\t\t\tPower Level: {joystick.ConnectionState}");
                }

                sb.AppendLine($"\tTouch Devices:");
                for (int i = 0; i < Input.TouchDevices.Devices.Count; i++)
                {
                    Input.TouchDevice touch = Input.TouchDevices.Devices[i];
                    sb.AppendLine($"\t\tTouch Device {i}:");

                    sb.AppendLine($"\t\t\tName: {touch.Name}");
                    sb.AppendLine($"\t\t\tType: {touch.Type}");
                    sb.AppendLine($"\t\t\tFinger Count: {touch.FingerCount}");
                }

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