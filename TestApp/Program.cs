namespace TestApp
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.Graphics.Shaders.Reflection;
    using HexaEngine.Core.IO;
    using HexaEngine.Network;
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;
    using System;
    using System.Net;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe partial class Program
    {
        private sealed class ConsoleLogWriter : ILogWriter
        {
            public void Clear()
            {
                Console.Clear();
            }

            public void Dispose()
            {
            }

            public void Flush()
            {
            }

            public void Write(string message)
            {
                Console.Write(message);
            }

            public Task WriteAsync(string message)
            {
                Console.Write(message);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// The factor to convert degrees to radians.
        /// </summary>
        public const double DegToRadFactor = float.Pi / 180;

        /// <summary>
        /// The factor to convert radians to degrees.
        /// </summary>
        public const double RadToDefFactor = 180 / float.Pi;

        /// <summary>
        /// Converts a <see cref="Vector3"/> from radians to degrees.
        /// </summary>
        /// <param name="v">The input vector in radians.</param>
        /// <returns>A <see cref="Vector3"/> with values converted to degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToDeg(this Vector3 v)
        {
            return new Vector3((float)(v.X * RadToDefFactor), (float)(v.Y * RadToDefFactor), (float)(v.Z * RadToDefFactor));
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> from degrees to radians.
        /// </summary>
        /// <param name="v">The input vector in degrees.</param>
        /// <returns>A <see cref="Vector3"/> with values converted to radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToRad(this Vector3 v)
        {
            // Using doubles reduces floating-point error.
            return new Vector3((float)(v.X * DegToRadFactor), (float)(v.Y * DegToRadFactor), (float)(v.Z * DegToRadFactor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToYawPitchRoll(this Quaternion r)
        {
            float yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            float pitch = MathF.Asin((float)Math.Clamp(2.0f * (r.X * r.W - r.Y * r.Z), -1, 1));
            float roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
            return new Vector3(yaw, pitch, roll);
        }

        public static void Main(string[] args)
        {
            Vector3 rotationEulerDeg = new(480, 45, 20);
            Vector3 rotationEulerRad = rotationEulerDeg.ToRad();
            Quaternion quat = Quaternion.CreateFromYawPitchRoll(rotationEulerRad.X, rotationEulerRad.Y, rotationEulerRad.Z);
            Vector3 rotationEulerRad2 = quat.ToYawPitchRoll();
            Vector3 rotationEulerDeg2 = rotationEulerRad2.ToDeg();

            Console.WriteLine("Original Degrees \t\t\t\t" + rotationEulerDeg);
            Console.WriteLine("Original Radiants \t\t\t\t" + rotationEulerRad);
            Console.WriteLine("(Euler to qaut to euler) Converted Radiants \t" + rotationEulerRad2);
            Console.WriteLine("Converted Degrees \t\t\t\t" + rotationEulerDeg2);
        }

        /* public static void Main()
         {
             Console.WriteLine("Starting Server ...");

             // Init in headless mode.
             Application.Boot(GraphicsBackend.Disabled, AudioBackend.Disabled);
             Platform.Init(false);

             // dummy call.
             ScriptAssemblyManager.SetInvalid(true);

             Scene scene = new();
             IScene scene1 = scene;
             scene1.Initialize(SceneInitFlags.None);

             using Server server = new(new(IPAddress.Parse("127.0.0.1"), 28900));
             server.Init();

             bool running = true;
             Console.CancelKeyPress += (sender, e) =>
             {
                 e.Cancel = true;
                 running = false;
             };

             Console.WriteLine("Started Server");
             Console.WriteLine("Listening to 127.0.0.1:28900");

             List<Client> clients = new();

             LoggerFactory.AddGlobalWriter(new ConsoleLogWriter());

             CrashLogger.Initialize();

             for (int i = 0; i < 1; i++)
             {
                 Task.Run(() =>
                 {
                     Client client = new(new(IPAddress.Parse("127.0.0.1"), 28900));
                     client.Init();
                     lock (clients)
                     {
                         clients.Add(client);
                     }
                 });
             }

             var serverTask = Task.Run(() =>
             {
                 while (running)
                 {
                     try
                     {
                         server.Tick();
                         lock (clients)
                         {
                             for (int i = 0; i < clients.Count; i++)
                             {
                                 clients[i].Tick();
                             }
                         }

                         Thread.Sleep(1); // Reduce CPU usage
                     }
                     catch (Exception ex)
                     {
                         // Log the error for debugging purposes
                         LoggerFactory.General.Log(ex);
                         // Optionally, you can break the loop or handle it accordingly
                     }
                 }
             });

             while (running)
             {
                 string? line = Console.ReadLine();
                 if (line == null)
                     continue;
                 switch (line)
                 {
                     case "exit":
                         running = false;
                         break;

                     case "d":
                         lock (clients)
                         {
                             for (int i = 0; i < clients.Count; i++)
                             {
                                 clients[i].Disconnect();
                             }
                         }
                         break;

                     case "c":
                         lock (clients)
                         {
                             for (int i = 0; i < 1; i++)
                             {
                                 Client client = new(new(IPAddress.Parse("127.0.0.1"), 28900));
                                 client.Init();
                                 lock (clients)
                                 {
                                     clients[i] = client;
                                 }
                             }
                         }
                         break;

                     default:
                         continue;
                 }
             }

             serverTask.Wait();

             Console.WriteLine("Exiting...");

             LoggerFactory.CloseAll();
         }
        */

        private static void NewMethod()
        {
            FileSystem.Initialize();
            string path = "C:\\Users\\juna\\source\\repos\\JunaMeinhold\\HexaEngine\\TestApp\\assets\\shared\\shaders\\compute\\occlusion\\occlusion.hlsl";
            string code = File.ReadAllText(path);
            bool result = CrossCompiler.CompileSPIRVFromSource(code, path, "main", [], ShaderKind.ComputeShader, SourceLanguage.HLSL, out var shader, out var error);

            if (result)
            {
                if (!ShaderReflector.ReflectShader(shader, out ShaderReflection? reflection))
                {
                    return;
                }

                foreach (var cb in reflection.ConstantBuffers)
                {
                    Console.WriteLine($"CB: {cb.Name}, Size {cb.Size}, Padded Size {cb.PaddedSize}, Slot {cb.Slot}");
                    foreach (var member in cb.Members)
                    {
                        var type = *member.Type;
                        var isVector = (type.TypeFlags & TypeFlags.Vector) != 0;
                        var isMatrix = (type.TypeFlags & TypeFlags.Matrix) != 0;
                        var scalarTraits = type.Traits.Numeric.Scalar;
                        var vectorTraits = type.Traits.Numeric.Vector;
                        var matrixTraits = type.Traits.Numeric.Matrix;

                        if ((type.TypeFlags & TypeFlags.Int) != 0)
                        {
                            string typeName = scalarTraits.Signed ? "int" : "uint";
                            string fullTypeName = isMatrix ? $"{typeName}{matrixTraits.ColumnCount}x{matrixTraits.RowCount}" : isVector ? $"{typeName}{vectorTraits.ComponentCount}" : typeName;
                            Console.WriteLine($"Member: {member.Name}, Offset: {member.Offset}, Absolute Offset: {member.AbsoluteOffset}, Size: {member.Size}, Padded Size: {member.PaddedSize}, Type: {fullTypeName}");
                        }
                        else if ((type.TypeFlags & TypeFlags.Float) != 0)
                        {
                            string typeName = isMatrix ? $"float{matrixTraits.ColumnCount}x{matrixTraits.RowCount}" : isVector ? $"float{vectorTraits.ComponentCount}" : "float";
                            Console.WriteLine($"Member: {member.Name}, Offset: {member.Offset}, Absolute Offset: {member.AbsoluteOffset}, Size: {member.Size}, Padded Size: {member.PaddedSize}, Type: {typeName}");
                        }
                        else
                        {
                            Console.WriteLine($"Member: {member.Name}, Offset: {member.Offset}, Absolute Offset: {member.AbsoluteOffset}, Size: {member.Size}, Padded Size: {member.PaddedSize}, Type: {member.Type->TypeName}");
                        }
                    }
                }

                reflection.Dispose();
            }
        }
    }
}