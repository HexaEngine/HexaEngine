namespace HexaEngine.Scenes.Serialization
{
    using Newtonsoft.Json.Linq;
    using System;

    public class PropertyListConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<IComponent>);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var list = (existingValue as List<IComponent>) ?? [];
            var jArray = JArray.Load(reader);

            foreach (var item in jArray)
            {
                var component = item.ToObject<IComponent>(serializer);
                if (component != null && component.IsSerializable)
                {
                    list.Add(component);
                }
            }
            return list;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}