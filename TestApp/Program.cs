namespace TestApp
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.Graphics.Shaders.Reflection;
    using HexaEngine.Core.IO;
    using HexaEngine.Graphics;
    using System;
    using System.Numerics;

    public static unsafe partial class Program
    {
        public static void Main(string[] args)
        {
            Viewport mainViewport = new(80, 80, 2560, 1440);
            Viewport renderViewport = new(30, 40, 860, 483.75f);
            Viewport virtualWindow = new(0, 0, 1920, 1080);

            Matrix3x2 translationToRenderViewport = Matrix3x2.CreateTranslation(-(mainViewport.Offset + renderViewport.Offset));

            Vector2 scaleToRenderViewport = Vector2.One / renderViewport.Size;
            Matrix3x2 scaleMatrixToRenderViewport = Matrix3x2.CreateScale(scaleToRenderViewport);

            Vector2 scaleToVirtualWindow = virtualWindow.Size;
            Matrix3x2 scaleMatrixToVirtualWindow = Matrix3x2.CreateScale(scaleToVirtualWindow);

            Matrix3x2 inputTransform = translationToRenderViewport * scaleMatrixToRenderViewport * scaleMatrixToVirtualWindow;

            var pos1Test = mainViewport.Offset + renderViewport.Offset + renderViewport.Size;

            var pos0 = Vector2.Transform(mainViewport.Offset + renderViewport.Offset, inputTransform);
            var pos1 = Vector2.Transform(pos1Test, inputTransform);

            pos1Test -= mainViewport.Offset;
            pos1Test -= renderViewport.Offset;
            pos1Test /= renderViewport.Size;
            pos1Test *= virtualWindow.Size;

            if (pos0 != virtualWindow.Offset)
            {
                throw new Exception();
            }

            if (pos1 != virtualWindow.Size)
            {
                throw new Exception();
            }
        }

        /*
        public static void Main(string[] args)
        {
            string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, "assets"));
            SourceAssetsDatabase.Init(root, new ProgressDummy()).Wait();

            var path = "gltftest/DamagedHelmet.gltf";

            SourceAssetMetadata metadata = new(path, Guid.Empty, DateTime.Now, 0x0);
            metadata.Save();
            ImportContext context = new(DefaultGuidProvider.Instance, metadata, "gltftest/DamagedHelmet.gltf", null);

            SourceAssetsDatabase.ImportFile(path);
        }
        */

        private struct ProgressDummy : IProgress<float>
        {
            public readonly void Report(float value)
            {
            }
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