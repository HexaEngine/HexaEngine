namespace TestApp
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.Graphics.Shaders.Reflection;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public static unsafe class StringExtensions
    {
        public static void ReverseString(this string s)
        {
            fixed (char* pStr = s)
            {
                char* pChar = pStr;
                char* pStrEnd = pStr + s.Length - 1;
                while (pChar < pStrEnd)
                {
                    (*pStrEnd, *pChar) = (*pChar, *pStrEnd);

                    pChar++;
                    pStrEnd--;
                }
            }
        }
    }

    public static unsafe partial class Program
    {
        public static void Main()
        {
            FileStream fs = File.OpenRead("Terrain.terrain");

            TerrainFile file = TerrainFile.Load(fs, TerrainLoadMode.Streaming);
            LayerBlendMask? blendMask = null;

            // extract a blend mask.
            for (int i = 0; i < file.LayerGroups.Count; i++)
            {
                var group = file.LayerGroups[i];
                if (group.Count == 1)
                    continue;
                blendMask = group.Mask;
                blendMask.ReadMaskData(fs);
                break;
            }

            fs.Close();

            if (blendMask == null)
            {
                return;
            }

            uint sizeRaw = blendMask.Width * blendMask.Height * sizeof(ulong);
            MemoryStream stream = new();

            for (int i = 0; i < 5000; i++)
            {
                stream.Position = 0;
                Compress(blendMask, stream);
            }

            List<double> samples = new();

            for (int i = 0; i < 5000; i++)
            {
                stream.Position = 0;
                long begin = Stopwatch.GetTimestamp();
                Compress(blendMask, stream);
                long end = Stopwatch.GetTimestamp();

                double delta = (end - begin) / (double)Stopwatch.Frequency;
                samples.Add(delta);
            }

            var min = samples.Min();
            var max = samples.Max();
            var avg = samples.Average();
            var stdDev = StandardDeviation(samples);

            Console.WriteLine($"Compression Ratio: {stream.Length / (double)sizeRaw} ({sizeRaw.FormatDataSize()}) ({stream.Length.FormatDataSize()})");
            Console.WriteLine($"Compression Time: Mean: {avg * 1000}ms, StdDev: {stdDev * 1000}ms, Min: {min * 1000}ms, Max: {max * 1000}ms");
        }

        public static double StandardDeviation(this List<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        private struct Run
        {
            public uint Length;
            public uint Offset;
            public ulong Value;
        }

        private static void Compress(LayerBlendMask mask, Stream stream)
        {
            var data = mask.Data;

            long basePosition = stream.Position;
            stream.Position += 4;

            const int RunBufferSize = 203;
            Span<Run> buffer = stackalloc Run[RunBufferSize]; // 203 * 10B = 2030B
            Span<byte> binBuffer = MemoryMarshal.AsBytes(buffer);

            int blocksWritten = 0;

            int bufferIndex = 0;

            Run run = default;

            bool newRun = true;

            for (uint i = 0; i < data.Length; i++)
            {
                var value = data[i];

                if (newRun && value != 0)
                {
                    run.Value = value;
                    run.Offset = i;
                    run.Length++;
                    newRun = false;
                }
                else if (run.Value == value)
                {
                    run.Length++;
                }
                else
                {
                    buffer[bufferIndex++] = run;
                    blocksWritten++;

                    if (value != 0)
                    {
                        run.Value = value;
                        run.Offset = i;
                        run.Length = 1;
                    }
                    else
                    {
                        newRun = true;
                    }

                    if (bufferIndex == RunBufferSize)
                    {
                        stream.Write(binBuffer);
                        bufferIndex = 0;
                    }
                }
            }

            if (!newRun)
            {
                buffer[bufferIndex++] = run;
                blocksWritten++;
            }

            if (bufferIndex != 0)
            {
                stream.Write(binBuffer[..(bufferIndex * sizeof(Run))]);
                bufferIndex = 0;
            }

            long endPosition = stream.Position;
            stream.Position = basePosition;
            stream.WriteInt32(blocksWritten, HexaEngine.Mathematics.Endianness.LittleEndian);
            stream.Position = endPosition;
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

    public unsafe class UniformScale
    {
        private Vector4[] vectors = new Vector4[WorkSize];

        public const int WorkSize = 1024;

        public UniformScale()
        {
        }

        private struct Particle
        {
            public float X, Y, Z, W;
        }

        public void Baseline()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new() { X = i, Y = i, Z = i, W = i };
                Particle* p = &particle;
                Vector4* myVec = (Vector4*)p;
                vectors[i] = *myVec;
            }
        }

        public void BitwiseMask()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new() { X = i, Y = i, Z = i, W = i };
                Particle* p = &particle;
                float* f = (float*)p;
                Vector4 myVec = new(f[0], f[1], f[2], f[3]);
                vectors[i] = myVec;
            }
        }
    }
}