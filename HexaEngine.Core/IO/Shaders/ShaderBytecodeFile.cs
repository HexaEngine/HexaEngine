namespace HexaEngine.Core.IO.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System.Text;

    public unsafe class ShaderBytecodeFile
    {
        public string Name;
        public ShaderBytecodeHeader Header;
        public byte[] Bytecode;
        public InputElementDescription[] InputElements;
        public ShaderMacro[] Macros;

        public ShaderBytecodeFile(string path) : this(path, FileSystem.Open(path))
        {
        }

        public ShaderBytecodeFile(string path, Stream fs)
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

        public ShaderBytecodeFile(string name, ShaderMacro[] macros, InputElementDescription[] inputElements, Shader* shader)
        {
            Name = name;
            Bytecode = shader->GetBytes();
            InputElements = inputElements;
            Macros = macros;
        }

        public static ShaderBytecodeFile Load(string path)
        {
            return new ShaderBytecodeFile(path);
        }

        public static ShaderBytecodeFile LoadExternal(string path)
        {
            return new ShaderBytecodeFile(path, File.OpenRead(path));
        }

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