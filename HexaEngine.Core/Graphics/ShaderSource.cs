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

        public static FileShaderSource FromFile(string file)
        {
            return new FileShaderSource(file);
        }

        public static CodeShaderSource FromCode(string identifer, string code)
        {
            return new CodeShaderSource(identifer, code);
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

        public override byte[] GetData()
        {
            return Encoding.UTF8.GetBytes(FileSystem.ReadAllText(Paths.CurrentShaderPath + filePath));
        }
    }

    public class CodeShaderSource : ShaderSource
    {
        private readonly string name;
        private string shaderCode;

        public CodeShaderSource(string name, string shaderCode)
        {
            this.name = name;
            this.shaderCode = shaderCode;
        }

        public override string Identifier => name;

        public string Code { get => shaderCode; set => shaderCode = value; }

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

        public override byte[] GetData()
        {
            return bytecode;
        }
    }
}