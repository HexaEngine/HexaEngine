using Hexa.NET.Logging;
using HexaEngine.Core.IO;
using System.Text;

namespace HexaEngine.Core.Graphics
{
    public abstract class ShaderSource
    {
        /// <summary>
        /// Used for the shader cache.
        /// </summary>
        public abstract string Identifier { get; }

        public abstract byte[] GetData();

        public abstract string WorkingDir { get; }

        public static FileShaderSource FromFile(string file)
        {
            return new FileShaderSource(file);
        }

        public static CodeShaderSource FromCode(string identifer, string code, string? workingDir = null)
        {
            return new CodeShaderSource(identifer, code, workingDir ?? "");
        }

        public static BytecodeShaderSource FromBytecode(byte[] bytecode)
        {
            return new BytecodeShaderSource(bytecode);
        }
    }

    public class FileShaderSource : ShaderSource
    {
        private readonly string filePath;

        public FileShaderSource(string filePath)
        {
            this.filePath = filePath;
        }

        public override string Identifier => filePath;

        public override string WorkingDir => Path.GetDirectoryName(filePath)!;

        public override byte[] GetData()
        {
            AssetPath path = new(filePath);
            if (path.HasNamespace)
            {
                return Encoding.UTF8.GetBytes(FileSystem.ReadAllText(path.Raw));
            }
            return Encoding.UTF8.GetBytes(FileSystem.ReadAllText(Paths.CurrentShaderPath + filePath));
        }
    }

    public class CodeShaderSource : ShaderSource
    {
        private readonly string name;
        private readonly string workingDir;
        private string shaderCode;

        public CodeShaderSource(string name, string shaderCode, string workingDir)
        {
            this.name = name;
            this.shaderCode = shaderCode;
            this.workingDir = workingDir;
        }

        public override string Identifier => name;

        public string Code { get => shaderCode; set => shaderCode = value; }

        public override string WorkingDir => workingDir;

        public override byte[] GetData()
        {
            return Encoding.UTF8.GetBytes(shaderCode);
        }
    }

    public class BytecodeShaderSource : ShaderSource
    {
        private byte[] bytecode;

        public BytecodeShaderSource(byte[] bytecode)
        {
            this.bytecode = bytecode;
        }

        public override string Identifier => null!; // not used for pre compiled.

        public byte[] Bytecode { get => bytecode; set => bytecode = value; }

        public override string WorkingDir => "";

        public override byte[] GetData()
        {
            return bytecode;
        }
    }
}