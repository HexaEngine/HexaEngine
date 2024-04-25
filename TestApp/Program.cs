namespace TestApp
{
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.Graphics.Shaders.Reflection;
    using HexaEngine.Core.IO;
    using System;
    using System.Numerics;

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