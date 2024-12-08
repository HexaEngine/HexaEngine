namespace HexaEngine.Core.IO.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// JsonConverter for handling serialization and deserialization of unmanaged types.
    /// </summary>
    /// <typeparam name="T">The unmanaged type to be serialized or deserialized.</typeparam>
    public unsafe class UnmanagedConverter<T> : JsonConverter<T> where T : unmanaged
    {
        /// <summary>
        /// Reads JSON data and converts it to an instance of the unmanaged type.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The type of the object to deserialize.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="hasExistingValue">A flag indicating whether there is an existing value.</param>
        /// <param name="serializer">The JSON serializer.</param>
        /// <returns>An instance of the unmanaged type.</returns>
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
            {
                throw new InvalidDataException();
            }

            byte[] value = token.Value<byte[]>()!;

            return MemoryMarshal.Cast<byte, T>(value.AsSpan())[0];
        }

        /// <summary>
        /// Writes an instance of the unmanaged type to JSON.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The instance of the unmanaged type to serialize.</param>
        /// <param name="serializer">The JSON serializer.</param>
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(MemoryMarshal.AsBytes(new T[] { value }.AsSpan()).ToArray());
            t.WriteTo(writer);
        }
    }
}