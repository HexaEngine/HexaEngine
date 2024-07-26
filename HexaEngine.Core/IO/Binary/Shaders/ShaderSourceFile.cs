namespace HexaEngine.Core.IO.Binary.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents a shader source file containing header information, bytecode, input elements, and macros.
    /// </summary>
    public unsafe class ShaderSourceFile
    {
        /// <summary>
        /// The name of the shader source file.
        /// </summary>
        public string Name;

        /// <summary>
        /// The header information for the shader source.
        /// </summary>
        public ShaderSourceHeader Header;

        /// <summary>
        /// The bytecode of the shader.
        /// </summary>
        public byte[] Bytecode;

        /// <summary>
        /// The input elements used by the shader.
        /// </summary>
        public InputElementDescription[] InputElements;

        /// <summary>
        /// The shader macros.
        /// </summary>
        public ShaderMacro[] Macros;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderSourceFile"/> class by loading the shader source from a file.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        public ShaderSourceFile(string path) : this(path, FileSystem.OpenRead(path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderSourceFile"/> class by loading the shader source from a stream.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        /// <param name="fs">The stream containing the shader source data.</param>
        public ShaderSourceFile(string path, Stream fs)
        {
            Name = path;
            Header.Read(fs);

            var stream = fs;

            Bytecode = stream.ReadBytes((int)Header.BytecodeLength);

            InputElements = new InputElementDescription[Header.InputElementCount];
            for (uint i = 0; i < Header.InputElementCount; i++)
            {
                InputElements[i].Read(stream, Header.Encoding, Header.Endianness);
            }

            Macros = new ShaderMacro[Header.MacroCount];
            for (uint i = 0; i < Header.MacroCount; i++)
            {
                Macros[i].Read(stream, Header.Encoding, Header.Endianness);
            }
            fs.Close();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderSourceFile"/> class with the provided data.
        /// </summary>
        /// <param name="name">The name of the shader source file.</param>
        /// <param name="macros">An array of shader macros.</param>
        /// <param name="inputElements">An array of input elements.</param>
        /// <param name="shader">A pointer to the shader.</param>
        public ShaderSourceFile(string name, ShaderMacro[] macros, InputElementDescription[] inputElements, Shader* shader)
        {
            Name = name;
            Bytecode = shader->ToArray();
            InputElements = inputElements;
            Macros = macros;
        }

        /// <summary>
        /// Loads a <see cref="ShaderSourceFile"/> from the specified path.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        /// <returns>A <see cref="ShaderSourceFile"/> loaded from the file.</returns>
        public static ShaderSourceFile Load(string path)
        {
            return new ShaderSourceFile(path);
        }

        /// <summary>
        /// Loads a <see cref="ShaderSourceFile"/> from an external file using the specified path.
        /// </summary>
        /// <param name="path">The path to the external shader source file.</param>
        /// <returns>A <see cref="ShaderSourceFile"/> loaded from the external file.</returns>
        public static ShaderSourceFile LoadExternal(string path)
        {
            return new ShaderSourceFile(path, File.OpenRead(path));
        }

        /// <summary>
        /// Saves the shader source to the specified directory with optional encoding, endianness, and compression settings.
        /// </summary>
        /// <param name="dir">The directory where the shader source will be saved.</param>
        /// <param name="encoding">The encoding to use when saving the shader source.</param>
        /// <param name="endianness">The endianness of the shader source.</param>
        /// <param name="compression">The compression method to use when saving the shader source.</param>
        public void Save(string dir, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.None)
        {
            Directory.CreateDirectory(dir);
            Stream fs = File.Create(Path.Combine(dir, Path.GetFileNameWithoutExtension(Name) + ".bytecode"));

            Header.Encoding = encoding;
            Header.Endianness = endianness;
            Header.Compression = compression;
            Header.Write(fs);

            var stream = fs;

            stream.Write(Bytecode);
            for (uint i = 0; i < Header.InputElementCount; i++)
            {
                InputElements[i].Write(stream, Header.Encoding, Header.Endianness);
            }
            for (uint i = 0; i < Header.MacroCount; i++)
            {
                Macros[i].Write(stream, Header.Encoding, Header.Endianness);
            }
            stream.Close();
            fs.Close();
        }
    }
}