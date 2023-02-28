namespace HexaEngine.Core.IO.Json
{
    using HexaEngine.Core.Unsafes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Runtime.InteropServices;

    public unsafe class UnmanagedPointerConverter<T> : JsonConverter where T : unmanaged
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Pointer<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
                throw new InvalidDataException();
            byte[] value = token.Value<byte[]>();
            var ptr = Alloc<T>();
            MemoryMarshal.Cast<byte, T>(value.AsSpan()).CopyTo(new Span<T>(ptr, 1));
            return new Pointer<T>(ptr);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Pointer<T> ptr)
            {
                JToken t = JToken.FromObject(MemoryMarshal.AsBytes(new Span<T>(ptr, 1)).ToArray());
                t.WriteTo(writer);
            }
        }
    }
}