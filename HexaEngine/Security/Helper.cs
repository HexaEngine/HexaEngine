namespace HexaEngine.Security
{
    using HexaEngine.Security.Credentials;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;

    public static class Helper
    {
        public static unsafe void Expose(this SecureString secstrPassword, Action<string?> action)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                /*** PLAINTEXT EXPOSURE BEGINS HERE ***/
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secstrPassword);
                var str = Marshal.PtrToStringUni(unmanagedString);

                fixed (char* pStr = str)
                {
                    action(str);

                    // fill whitespace after usage to ensure no data is still in the memory.
                    if (str != null)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            pStr[i] = ' ';
                        }
                    }
                }
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
                /*** PLAINTEXT EXPOSURE ENDS HERE ***/
            }
        }

        public static unsafe string? ConvertToString(this SecureString secstrPassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secstrPassword);
                var str = Marshal.PtrToStringUni(unmanagedString);
                return str;
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }
        }

        public static int WriteSecureString(this SecureString? str, Span<byte> dest)
        {
            int written = str?.Length * 2 ?? 0;
            IntPtr bstr = IntPtr.Zero;
            byte[]? workArray = null;
            GCHandle? handle = null;
            try
            {
                /*** PLAINTEXT EXPOSURE BEGINS HERE ***/
                bstr = Marshal.SecureStringToBSTR(str ?? new());
                unsafe
                {
                    byte* bstrBytes = (byte*)bstr;
                    workArray = new byte[written];
                    handle = GCHandle.Alloc(workArray, GCHandleType.Pinned);

                    for (int i = 0; i < workArray.Length; i++)
                    {
                        workArray[i] = *bstrBytes++;
                    }
                }

                BinaryPrimitives.WriteInt32LittleEndian(dest, written);
                workArray.CopyTo(dest[4..]);
            }
            finally
            {
                if (workArray != null)
                {
                    for (int i = 0; i < workArray.Length; i++)
                    {
                        workArray[i] = 0;
                    }
                }

                if (bstr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr);
                }

                handle?.Free();

                /*** PLAINTEXT EXPOSURE ENDS HERE ***/
            }

            return written + 4;
        }

        public static int SizeOfSecureString(this SecureString? str)
        {
            return (str?.Length ?? 0) * 2 + 4;
        }

        public static int ReadSecureString(this Span<byte> source, out SecureString str)
        {
            int sizeBytes = BinaryPrimitives.ReadInt32LittleEndian(source);

            if (sizeBytes == 0)
            {
                str = new();
                return 4;
            }

            unsafe
            {
                char* src = (char*)Unsafe.AsPointer(ref source.Slice(4, sizeBytes)[0]);
                str = new(src, sizeBytes / 2);
            }

            return 4 + sizeBytes;
        }

        public static void HashSecureString(this SecureString input, PasswordHashAlgorithm algorithm, out Span<byte> hash, out GCHandle hashPin)
        {
            var bstr = Marshal.SecureStringToBSTR(input);
            var length = Marshal.ReadInt32(bstr, -4);
            var bytes = new byte[length];

            var hash512 = new byte[64];

            hash = new byte[32];
            hashPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(bstr, bytes, 0, length);
                Marshal.ZeroFreeBSTR(bstr);

                switch (algorithm)
                {
                    case PasswordHashAlgorithm.SHA_512_256:
                        SHA512.TryHashData(bytes, hash512, out _);
                        SHA256.TryHashData(bytes, hash, out _);
                        break;

                    default:
                        throw new NotSupportedException($"The algorithm {algorithm} is not supported");
                }
            }
            finally
            {
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = 0;
                }

                bytesPin.Free();
            }
        }

        public static void Encrypt(this AesGcm aes, Span<byte> source, Span<byte> destination)
        {
            // Get parameter sizes
            var nonceSize = AesGcm.NonceByteSizes.MaxSize;
            var tagSize = AesGcm.TagByteSizes.MaxSize;
            var cipherSize = source.Length;

            // We write everything into one big array for easier encoding
            var encryptedDataLength = 4 + nonceSize + 4 + tagSize + cipherSize;

            Span<byte> encryptedData = new byte[encryptedDataLength];

            // Copy parameters
            BinaryPrimitives.WriteInt32LittleEndian(encryptedData[..4], nonceSize);
            BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4), tagSize);
            var nonce = encryptedData.Slice(4, nonceSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            // Generate secure nonce
            RandomNumberGenerator.Fill(nonce);

            // Encrypt
            aes.Encrypt(nonce, source, cipherBytes, tag);

            // Encode for transmission
            encryptedData.CopyTo(destination);
        }

        public static void Decrypt(this AesGcm aes, Span<byte> source, Span<byte> destination)
        {
            // Extract parameter sizes
            var nonceSize = BinaryPrimitives.ReadInt32LittleEndian(source);
            var tagSize = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4 + nonceSize, 4));
            var cipherSize = source.Length - 4 - nonceSize - 4 - tagSize;

            // Extract parameters
            var nonce = source.Slice(4, nonceSize);
            var tag = source.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = source.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            // Decrypt
            aes.Decrypt(nonce, cipherBytes, tag, destination);
        }

        public static int MeasureCipherSize(int size, Cipher algorithm)
        {
            return algorithm switch
            {
                Cipher.AES_GCM_256 => 4 + AesGcm.NonceByteSizes.MaxSize + 4 + AesGcm.TagByteSizes.MaxSize + size,
                _ => throw new NotSupportedException(),
            };
        }

        public static int MeasureHashSize(int size, HashAlgorithm algorithm)
        {
            return algorithm switch
            {
                HashAlgorithm.None => 0,
                HashAlgorithm.HMAC_SHA_512 => 512 / 8,
                _ => throw new NotSupportedException(),
            };
        }

        public static int MeasureDecryptedSize(Span<byte> source, Cipher algorithm)
        {
            switch (algorithm)
            {
                case Cipher.AES_GCM_256:
                    var nonceSize = BinaryPrimitives.ReadInt32LittleEndian(source[..4]);
                    var tagSize = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4 + nonceSize, 4));
                    var cipherSize = source.Length - 4 - nonceSize - 4 - tagSize;
                    return cipherSize;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}