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

        public static void Main()
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