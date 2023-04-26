namespace HexaEngine.Core.IO.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Runtime.InteropServices;

    public unsafe class UnmanagedConverter<T> : JsonConverter<T> where T : unmanaged
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
            {
                throw new InvalidDataException();
            }

            byte[] value = token.Value<byte[]>();

            return MemoryMarshal.Cast<byte, T>(value.AsSpan())[0];
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(MemoryMarshal.AsBytes(new T[] { value }.AsSpan()).ToArray());
            t.WriteTo(writer);
        }
    }
}