namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Security;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using HashAlgorithm = HashAlgorithm;

    public class CredentialsContainer : DisposableBase, IDictionary<string, ICredentials>
    {
        private readonly Dictionary<string, ICredentials> credentials = new();

        public CredentialsContainer()
        {
        }

        public ICredentials this[string name]
        {
            get => credentials[name];
            set => credentials[name] = value;
        }

        public int Count => credentials.Count;

        public ICollection<string> Keys => ((IDictionary<string, ICredentials>)credentials).Keys;

        public ICollection<ICredentials> Values => ((IDictionary<string, ICredentials>)credentials).Values;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, ICredentials>>)credentials).IsReadOnly;

        public ICredentials Get(string name)
        {
            return credentials[name];
        }

        public bool TryGetValue(string name, [NotNullWhen(true)] out ICredentials? result)
        {
            return credentials.TryGetValue(name, out result);
        }

        public void Add(string name, ICredentials credentials)
        {
            this.credentials.Add(name, credentials);
        }

        public bool Remove(string name)
        {
            return credentials.Remove(name);
        }

        public void Clear()
        {
            credentials.Clear();
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, ICredentials>)credentials).ContainsKey(key);
        }

        public void Add(KeyValuePair<string, ICredentials> item)
        {
            ((ICollection<KeyValuePair<string, ICredentials>>)credentials).Add(item);
        }

        public bool Contains(KeyValuePair<string, ICredentials> item)
        {
            return ((ICollection<KeyValuePair<string, ICredentials>>)credentials).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, ICredentials>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, ICredentials>>)credentials).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, ICredentials> item)
        {
            return ((ICollection<KeyValuePair<string, ICredentials>>)credentials).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, ICredentials>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, ICredentials>>)credentials).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)credentials).GetEnumerator();
        }

        public unsafe void Encrypt(Stream stream, SecureString password)
        {
            CredentialsContainerHeader header = default;
            CredentialRecord[] records = new CredentialRecord[credentials.Count];
            header.RecordsCount = records.Length;

            int recordIdx = 0;
            int offset = 0;
            foreach (var credentialPair in credentials)
            {
                var name = credentialPair.Key;
                var credential = credentialPair.Value;
                var size = credential.SizeOf();
                records[recordIdx++] = new(name, credential.Type, offset, size);
                offset += size;
            }

            int recordTableSize = records.Sum(x => x.SizeOf());
            int dataSize = records.Sum(x => x.Size);
            int plainSize = recordTableSize + dataSize;

            header.RecordTableLength = recordTableSize;
            header.DataLength = dataSize;

            byte* plainData = (byte*)Marshal.AllocHGlobal(plainSize);
            Span<byte> plainBuffer = new(plainData, plainSize);

            int idx = 0;
            for (int i = 0; i < records.Length; i++)
            {
                idx += records[i].Write(plainBuffer[idx..]);
            }

            header.DataOffset = idx;

            foreach (var credentialPair in credentials)
            {
                idx += credentialPair.Value.Write(plainBuffer[idx..]);
            }

            int headerSize = CredentialsContainerHeader.SizeOf();
            int hashSize = Helper.MeasureHashSize(plainSize, HashAlgorithm.HMAC_SHA_512);
            int cipherSize = Helper.MeasureCipherSize(plainSize, Cipher.AES_GCM_256);
            int containerSize = headerSize + hashSize + cipherSize;

            byte* containerData = (byte*)Marshal.AllocHGlobal(containerSize);
            Span<byte> containerBuffer = new(containerData, containerSize);
            Span<byte> headerBuffer = containerBuffer[..headerSize];
            Span<byte> hashBuffer = containerBuffer.Slice(headerSize, hashSize);
            Span<byte> cipherBuffer = containerBuffer.Slice(headerSize + hashSize, cipherSize);

            password.HashSecureString(header.PasswordHash, out var hash, out var hashPin);

            AesGcm aes = new(hash, AesGcm.TagByteSizes.MaxSize);
            Helper.Encrypt(aes, plainBuffer, cipherBuffer);
            aes.Dispose();

            plainBuffer.Clear();
            Marshal.FreeHGlobal((nint)plainData);

            HMACSHA512 hmac = new(hash.ToArray());
            hmac.TryComputeHash(cipherBuffer, hashBuffer, out _);
            hmac.Dispose();

            hash.Clear();
            hashPin.Free();

            header.Write(headerBuffer);

            stream.Write(containerBuffer);

            containerBuffer.Clear();
            Marshal.FreeHGlobal((nint)containerData);
        }

        public unsafe void Decrypt(Stream stream, SecureString password)
        {
            int headerSize = CredentialsContainerHeader.SizeOf();
            Span<byte> headerBuffer = stackalloc byte[headerSize];
            stream.Read(headerBuffer);
            CredentialsContainerHeader header = default;
            header.Read(headerBuffer);

            int plainSize = header.RecordTableLength + header.DataLength;
            int hashSize = Helper.MeasureHashSize(plainSize, header.Hash);
            int cipherSize = Helper.MeasureCipherSize(plainSize, header.Cipher);
            int containerSize = hashSize + cipherSize;

            byte* containerData = (byte*)Marshal.AllocHGlobal(containerSize);
            Span<byte> containerBuffer = new(containerData, containerSize);
            stream.Read(containerBuffer);

            byte* plainData = (byte*)Marshal.AllocHGlobal(plainSize);
            Span<byte> plainBuffer = new(plainData, plainSize);

            Span<byte> hashBuffer = containerBuffer[..hashSize];
            Span<byte> cipherBuffer = containerBuffer.Slice(hashSize, cipherSize);

            byte* computedHashData = (byte*)Marshal.AllocHGlobal(hashSize);
            Span<byte> computedHashBuffer = new(computedHashData, hashSize);

            password.HashSecureString(PasswordHashAlgorithm.SHA_512_256, out var hash, out var hashPin);
            HMACSHA512 hmac = new(hash.ToArray());
            hmac.TryComputeHash(cipherBuffer, computedHashBuffer, out _);
            hmac.Dispose();
            bool equal = computedHashBuffer.SequenceEqual(hashBuffer);
            Marshal.FreeHGlobal((nint)computedHashData);

            if (!equal)
            {
                throw new InvalidOperationException("");
            }

            AesGcm aes = new(hash, AesGcm.TagByteSizes.MaxSize);
            Helper.Decrypt(aes, cipherBuffer, plainBuffer);
            aes.Dispose();

            hash.Clear();
            hashPin.Free();

            containerBuffer.Clear();
            Marshal.FreeHGlobal((nint)containerData);

            CredentialRecord[] records = new CredentialRecord[header.RecordsCount];

            int idx = 0;
            for (int i = 0; i < header.RecordsCount; i++)
            {
                idx += records[i].Read(plainBuffer[idx..]);
            }

            for (int i = 0; i < header.RecordsCount; i++)
            {
                var record = records[i];
                ICredentials? cred = CreateCredentialsFrom(record);

                if (cred == null)
                {
                    continue;
                }

                cred.Read(plainBuffer.Slice((header.DataOffset + record.Offset), record.Size));
                credentials.Add(record.Name, cred);
            }

            plainBuffer.Clear();
            Marshal.FreeHGlobal((nint)plainData);
        }

        public unsafe bool TryDecrypt(Stream stream, SecureString password)
        {
            int headerSize = CredentialsContainerHeader.SizeOf();
            Span<byte> headerBuffer = stackalloc byte[headerSize];
            stream.Read(headerBuffer);
            CredentialsContainerHeader header = default;

            if (!header.TryRead(headerBuffer, out _))
            {
                return false;
            }

            int plainSize = header.RecordTableLength + header.DataLength;
            int hashSize = Helper.MeasureHashSize(plainSize, header.Hash);
            int cipherSize = Helper.MeasureCipherSize(plainSize, header.Cipher);
            int containerSize = hashSize + cipherSize;

            byte* containerData = (byte*)Marshal.AllocHGlobal(containerSize);
            Span<byte> containerBuffer = new(containerData, containerSize);
            stream.Read(containerBuffer);

            byte* plainData = (byte*)Marshal.AllocHGlobal(plainSize);
            Span<byte> plainBuffer = new(plainData, plainSize);

            Span<byte> hashBuffer = containerBuffer[..hashSize];
            Span<byte> cipherBuffer = containerBuffer.Slice(hashSize, cipherSize);

            byte* computedHashData = (byte*)Marshal.AllocHGlobal(hashSize);
            Span<byte> computedHashBuffer = new(computedHashData, hashSize);

            password.HashSecureString(PasswordHashAlgorithm.SHA_512_256, out var hash, out var hashPin);
            HMACSHA512 hmac = new(hash.ToArray());
            hmac.TryComputeHash(cipherBuffer, computedHashBuffer, out _);
            hmac.Dispose();
            bool equal = computedHashBuffer.SequenceEqual(hashBuffer);
            Marshal.FreeHGlobal((nint)computedHashData);

            if (!equal)
            {
                containerBuffer.Clear();
                Marshal.FreeHGlobal((nint)containerData);

                plainBuffer.Clear();
                Marshal.FreeHGlobal((nint)plainData);

                return false;
            }

            AesGcm aes = new(hash, AesGcm.TagByteSizes.MaxSize);
            Helper.Decrypt(aes, cipherBuffer, plainBuffer);
            aes.Dispose();

            hash.Clear();
            hashPin.Free();

            containerBuffer.Clear();
            Marshal.FreeHGlobal((nint)containerData);

            CredentialRecord[] records = new CredentialRecord[header.RecordsCount];

            int idx = 0;
            for (int i = 0; i < header.RecordsCount; i++)
            {
                idx += records[i].Read(plainBuffer[idx..]);
            }

            for (int i = 0; i < header.RecordsCount; i++)
            {
                var record = records[i];
                ICredentials? cred = CreateCredentialsFrom(record);

                if (cred == null)
                {
                    continue;
                }

                cred.Read(plainBuffer.Slice(header.DataOffset + record.Offset, record.Size));
                credentials.Add(record.Name, cred);
            }

            plainBuffer.Clear();
            Marshal.FreeHGlobal((nint)plainData);

            return true;
        }

        private static ICredentials CreateCredentialsFrom(CredentialRecord record)
        {
            return record.Type switch
            {
                CredentialType.Binary => new BinaryCredentials(),
                CredentialType.UsernamePassword => new UsernamePasswordCredentials(),
                CredentialType.Token => new TokenCredentials(),
                CredentialType.X509Certificate => new X509CertificateCredentials(),
                _ => new UnknownCredentials(record.Type),
            };
        }

        protected override void DisposeCore()
        {
            foreach (var record in credentials)
            {
                record.Value.Dispose();
            }
            credentials.Clear();
        }
    }
}