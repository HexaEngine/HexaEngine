namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Graphics.Shaders;
    using System;
    using System.Buffers.Binary;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents an entry in the shader cache.
    /// </summary>
    public unsafe struct ShaderCacheEntry : IEquatable<ShaderCacheEntry>
    {
        /// <summary>
        /// Gets or sets the name of the shader.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the CRC32 hash of the shader.
        /// </summary>
        public uint Crc32Hash;

        /// <summary>
        /// Gets or sets the source language of the shader.
        /// </summary>
        public SourceLanguage Language;

        /// <summary>
        /// Gets or sets the macros used in the shader.
        /// </summary>
        public ShaderMacro[] Macros;

        /// <summary>
        /// Gets or sets the input elements of the shader.
        /// </summary>
        public InputElementDescription[] InputElements;

        /// <summary>
        /// Gets or sets the pointer to the shader bytecode.
        /// </summary>
        public Shader* Shader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderCacheEntry"/> struct.
        /// </summary>
        /// <param name="name">The name of the shader.</param>
        /// <param name="crc32Hash">The CRC32 hash of the shader.</param>
        /// <param name="language">The source language of the shader.</param>
        /// <param name="macros">The macros used in the shader.</param>
        /// <param name="inputElements">The input elements of the shader.</param>
        /// <param name="bytecode">The pointer to the shader bytecode.</param>
        public ShaderCacheEntry(string name, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, InputElementDescription[] inputElements, Shader* bytecode)
        {
            Name = name;
            Crc32Hash = crc32Hash;
            Language = language;
            Macros = macros;
            InputElements = inputElements;
            Shader = bytecode;
        }

        /// <summary>
        /// Writes the shader cache entry to the specified destination span.
        /// </summary>
        /// <param name="dest">The destination span to write to.</param>
        /// <param name="encoder">The encoder to use for string encoding.</param>
        /// <returns>The number of bytes written to the destination span.</returns>
        public int Write(Span<byte> dest, Encoder encoder)
        {
            int idx = 0;
            idx += WriteString(dest[idx..], Name, encoder);
            idx += WriteUInt32(dest[idx..], Crc32Hash);
            idx += WriteInt32(dest[idx..], (int)Language);
            idx += WriteInt32(dest[idx..], Macros.Length);

            for (int i = 0; i < Macros.Length; i++)
            {
                var macro = Macros[i];
                idx += WriteString(dest[idx..], macro.Name, encoder);
                idx += WriteString(dest[idx..], macro.Definition, encoder);
            }
            idx += WriteInt32(dest[idx..], InputElements.Length);
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
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], (int)Shader->Length);
            }
            else
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], 0);
            }

            idx += 4;

            if (Shader != null)
            {
                Shader->CopyTo(dest[idx..]);
                idx += (int)Shader->Length;
            }
            else
            {
                idx += 0;
            }

            return idx;
        }

        /// <summary>
        /// Reads the shader cache entry from the specified source span.
        /// </summary>
        /// <param name="src">The source span to read from.</param>
        /// <param name="decoder">The decoder to use for string decoding.</param>
        /// <returns>The number of bytes read from the source span.</returns>
        public int Read(ReadOnlySpan<byte> src, Decoder decoder)
        {
            int idx = 0;
            idx += ReadString(src, out Name, decoder);
            idx += ReadUInt32(src[idx..], out var crc32Hash);
            Crc32Hash = crc32Hash;
            idx += ReadInt32(src[idx..], out var lang);
            Language = (SourceLanguage)lang;

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
            Shader = AllocT<Shader>();
            Shader->Bytecode = AllocT<byte>(len);
            Shader->Length = (nuint)len;
            fixed (void* ptr = src.Slice(idx, len))
            {
                Buffer.MemoryCopy(ptr, Shader->Bytecode, len, len);
            }
            idx += len;
            return idx;
        }

        /// <summary>
        /// Writes a string to the specified destination span.
        /// </summary>
        /// <param name="dest">The destination span to write to.</param>
        /// <param name="str">The string to write.</param>
        /// <param name="encoder">The encoder to use for string encoding.</param>
        /// <returns>The number of bytes written to the destination span.</returns>
        private static int WriteString(Span<byte> dest, string str, Encoder encoder)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, encoder.GetByteCount(str, true));
            return encoder.GetBytes(str, dest[4..], true) + 4;
        }

        /// <summary>
        /// Reads a string from the specified source span.
        /// </summary>
        /// <param name="src">The source span to read from.</param>
        /// <param name="str">The output string.</param>
        /// <param name="decoder">The decoder to use for string decoding.</param>
        /// <returns>The number of bytes read from the source span.</returns>
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

        /// <summary>
        /// Writes a 32-bit signed integer to the specified destination span.
        /// </summary>
        /// <param name="dest">The destination span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written to the destination span.</returns>
        private static int WriteInt32(Span<byte> dest, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, value);
            return 4;
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the specified source span.
        /// </summary>
        /// <param name="src">The source span to read from.</param>
        /// <param name="value">The output value.</param>
        /// <returns>The number of bytes read from the source span.</returns>
        private static int ReadInt32(ReadOnlySpan<byte> src, out int value)
        {
            value = BinaryPrimitives.ReadInt32LittleEndian(src);
            return 4;
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to the specified destination span.
        /// </summary>
        /// <param name="dest">The destination span to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written to the destination span.</returns>
        private static int WriteUInt32(Span<byte> dest, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(dest, value);
            return 4;
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the specified source span.
        /// </summary>
        /// <param name="src">The source span to read from.</param>
        /// <param name="value">The output value.</param>
        /// <returns>The number of bytes read from the source span.</returns>
        private static int ReadUInt32(ReadOnlySpan<byte> src, out uint value)
        {
            value = BinaryPrimitives.ReadUInt32LittleEndian(src);
            return 4;
        }

        /// <summary>
        /// Gets the size of the specified string including its length.
        /// </summary>
        /// <param name="str">The string to calculate the size of.</param>
        /// <param name="encoder">The encoder to use for string encoding.</param>
        /// <returns>The size of the string including its length.</returns>
        private static int SizeOf(string str, Encoder encoder)
        {
            return 4 + encoder.GetByteCount(str, true);
        }

        /// <summary>
        /// Gets the size of the shader cache entry in bytes.
        /// </summary>
        /// <param name="encoder">The encoder to use for string encoding.</param>
        /// <returns>The size of the shader cache entry in bytes.</returns>
        public int SizeOf(Encoder encoder)
        {
            if (Shader != null)
            {
                return 28 +
                    SizeOf(Name, encoder) +
                    Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) +
                    InputElements.Sum(x => SizeOf(x.SemanticName, encoder) + 24) +
                    (int)Shader->Length;
            }
            else
            {
                return 28 +
                    SizeOf(Name, encoder) +
                    Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) +
                    InputElements.Sum(x => SizeOf(x.SemanticName, encoder) + 24);
            }
        }

        /// <summary>
        /// Frees the allocated memory for the shader bytecode.
        /// </summary>
        public void Free()
        {
            Utils.Free(Shader);
            Shader = null;
        }

        /// <inheritdoc/>
        public readonly bool Equals(ShaderCacheEntry other)
        {
            if (Name != other.Name)
            {
                return false;
            }

            if (Language != other.Language)
            {
                return false;
            }

            if (Macros == other.Macros && Macros == null && other.Macros == null)
            {
                return true;
            }

            if (Macros != other.Macros && (Macros == null || other.Macros == null))
            {
                return false;
            }

            if (Macros.Length != (other.Macros?.Length ?? 0))
            {
                return false;
            }

            for (int i = 0; i < Macros.Length; i++)
            {
#nullable disable
                if (Macros[i].Name != other.Macros[i].Name ||
                    Macros[i].Definition != other.Macros[i].Definition)
                {
                    return false;
                }
#nullable enable
            }
            return true;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is ShaderCacheEntry entry && Equals(entry);
        }

        /// <summary>
        /// Indicates whether two shader cache entries are equal.
        /// </summary>
        /// <param name="left">The first shader cache entry to compare.</param>
        /// <param name="right">The second shader cache entry to compare.</param>
        /// <returns>true if the two shader cache entries are equal; otherwise, false.</returns>
        public static bool operator ==(ShaderCacheEntry left, ShaderCacheEntry right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two shader cache entries are not equal.
        /// </summary>
        /// <param name="left">The first shader cache entry to compare.</param>
        /// <param name="right">The second shader cache entry to compare.</param>
        /// <returns>true if the two shader cache entries are not equal; otherwise, false.</returns>
        public static bool operator !=(ShaderCacheEntry left, ShaderCacheEntry right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Name, Macros);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return Name;
        }
    }
}