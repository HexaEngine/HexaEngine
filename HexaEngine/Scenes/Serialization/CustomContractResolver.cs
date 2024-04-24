namespace HexaEngine.Scenes.Serialization
{
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Reflection;

    public class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(GameObject))
            {
                if (property.PropertyName == "Components" || property.PropertyName == "Children")
                {
                    property.ShouldSerialize =
                    instance =>
                    {
                        return instance is not PrefabObject;
                    };
                }
            }

            return property;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);

            return contract;
        }
    }
}