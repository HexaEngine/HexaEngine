namespace HexaEngine.Core.IO.Json
{
    using Hexa.NET.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// JsonConverter for handling serialization and deserialization of unmanaged pointers.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of the pointer.</typeparam>
    public unsafe class UnmanagedPointerConverter<T> : JsonConverter where T : unmanaged
    {
        /// <summary>
        /// Determines whether this converter can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type of the object to convert.</param>
        /// <returns><c>true</c> if the converter can convert the specified type; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Pointer<T>);
        }

        /// <summary>
        /// Reads JSON data and converts it to an instance of the unmanaged pointer.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The type of the object to deserialize.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="serializer">The JSON serializer.</param>
        /// <returns>An instance of the unmanaged pointer.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
            {
                throw new InvalidDataException();
            }

            byte[] value = token.Value<byte[]>()!;
            var ptr = AllocT<T>();
            MemoryMarshal.Cast<byte, T>(value.AsSpan()).CopyTo(new Span<T>(ptr, 1));
            return new Pointer<T>(ptr);
        }

        /// <summary>
        /// Writes an instance of the unmanaged pointer to JSON.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The instance of the unmanaged pointer to serialize.</param>
        /// <param name="serializer">The JSON serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is Pointer<T> ptr)
            {
                JToken t = JToken.FromObject(MemoryMarshal.AsBytes(new Span<T>(ptr, 1)).ToArray());
                t.WriteTo(writer);
            }
        }

        /// <summary>
        /// Allocates unmanaged memory for the specified type.
        /// </summary>
        /// <typeparam name="TU">The unmanaged type.</typeparam>
        /// <returns>A pointer to the allocated memory.</returns>
        private static unsafe TU* AllocT<TU>() where TU : unmanaged
        {
            int size = sizeof(TU);
            TU* ptr = (TU*)Marshal.AllocHGlobal(size);
            return ptr;
        }
    }
}