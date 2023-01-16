namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.IO;
    using System;
    using System.Buffers.Binary;
    using System.Linq;
    using System.Text;

    public unsafe class ShaderBytecodeFile
    {
        public ShaderMacro[] Macros;
        public InputElementDescription[] InputElements;
        public byte[] Shader;

        public ShaderBytecodeFile(string path)
        {
            Read(FileSystem.ReadAllBytes(path));
        }

        public ShaderBytecodeFile(ShaderMacro[] macros, InputElementDescription[] inputElements, Shader* shader)
        {
            Macros = macros;
            InputElements = inputElements;
            Shader = shader->GetBytes();
        }

        public int Write(Span<byte> dest, Encoder encoder)
        {
            int idx = 0;
            BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], Macros.Length);
            idx += 4;
            for (int i = 0; i < Macros.Length; i++)
            {
                var macro = Macros[i];
                idx += WriteString(dest[idx..], macro.Name, encoder);
                idx += WriteString(dest[idx..], macro.Definition, encoder);
            }
            BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], InputElements.Length);
            idx += 4;
            for (int i = 0; i < InputElements.Length; i++)
            {
                var element = InputElements[i];
                idx += WriteString(dest[idx..], element.SemanticName, encoder);
                idx += WriteInt32(dest[idx..], element.SemanticIndex);
                idx += WriteInt32(dest[idx..], (int)element.Format);
                idx += WriteInt32(dest[idx..], element.Slot);
                idx += WriteInt32(dest[idx..], element.AlignedByteOffset);
                idx += WriteInt32(dest[idx..], (int)element.Classification);
                idx += WriteInt32(dest[idx..], element.InstanceDataStepRate);
            }
            if (Shader != null)
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], (int)Shader.Length);
            else
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], 0);
            idx += 4;

            if (Shader != null)
            {
                Shader.CopyTo(dest[idx..]);
            }
            else
            {
                idx += 0;
            }

            return idx;
        }

        public int Read(ReadOnlySpan<byte> src, Decoder decoder)
        {
            int idx = 0;

            // read macros
            int count = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Macros = new ShaderMacro[count];
            for (var i = 0; i < count; i++)
            {
                idx += ReadString(src[idx..], out string name, decoder);
                idx += ReadString(src[idx..], out string definition, decoder);
                Macros[i] = new ShaderMacro(name, definition);
            }

            int countElements = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            InputElements = new InputElementDescription[countElements];
            for (var i = 0; i < countElements; i++)
            {
                idx += ReadString(src[idx..], out string semanticName, decoder);
                idx += ReadInt32(src[idx..], out int semanticIndex);
                idx += ReadInt32(src[idx..], out int format);
                idx += ReadInt32(src[idx..], out int slot);
                idx += ReadInt32(src[idx..], out int alignedByteOffset);
                idx += ReadInt32(src[idx..], out int classification);
                idx += ReadInt32(src[idx..], out int instanceDataStepRate);
                InputElements[i] = new(semanticName, semanticIndex, (Format)format, alignedByteOffset, slot, (InputClassification)classification, instanceDataStepRate);
            }

            int len = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Shader = new byte[len];
            fixed (void* dst = Shader)
            {
                fixed (void* ptr = src.Slice(idx, len))
                {
                    Buffer.MemoryCopy(ptr, dst, len, len);
                }
            }

            idx += len;
            return idx;
        }

        private static int WriteString(Span<byte> dest, string str, Encoder encoder)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, encoder.GetByteCount(str, true));
            return encoder.GetBytes(str, dest[4..], true) + 4;
        }

        private static int ReadString(ReadOnlySpan<byte> src, out string str, Decoder decoder)
        {
            int len = BinaryPrimitives.ReadInt32LittleEndian(src);
            ReadOnlySpan<byte> bytes = src.Slice(4, len);
            int charCount = decoder.GetCharCount(bytes, true);
            Span<char> chars = charCount < 2048 ? stackalloc char[charCount] : new char[charCount];
            decoder.GetChars(bytes, chars, true);
            str = new(chars);
            return len + 4;
        }

        private static int WriteInt32(Span<byte> dest, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, value);
            return 4;
        }

        private static int ReadInt32(ReadOnlySpan<byte> src, out int value)
        {
            value = BinaryPrimitives.ReadInt32LittleEndian(src);
            return 4;
        }

        private static int SizeOf(string str, Encoder encoder)
        {
            return 4 + encoder.GetByteCount(str, true);
        }

        public int SizeOf(Encoder encoder)
        {
            if (Shader != null)
                return 20 +
                    Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) +
                    InputElements.Sum(x => SizeOf(x.SemanticName, encoder) + 24) +
                    Shader.Length;
            else
                return 20 +
                    Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) +
                    InputElements.Sum(x => SizeOf(x.SemanticName, encoder) + 24);
        }
    }
}