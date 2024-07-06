using HexaEngine.Core.Graphics.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;

namespace HexaEngine.Scenes.Serialization
{
    /// <summary>
    /// A part of the extensive scene serialization system.
    /// </summary>
    public static class ComponentRegistry
    {
        private static readonly Dictionary<string, Type> components = [];

        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : IXmlSerializable, new()
        {
            Register<T>(nameof(T));
        }

        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(string name) where T : IXmlSerializable, new()
        {
        }
    }

    public abstract class SerializationCacheEntry
    {
        public abstract bool CanSerialize(object? value);

        public abstract void Serialize(XmlWriter xmlWriter, object? value);

        public bool TrySerialize(XmlWriter xmlWriter, object? value)
        {
            if (CanSerialize(value))
            {
                Serialize(xmlWriter, value);
                return true;
            }
            return false;
        }
    }

    public class SerializationCacheEntry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T> : SerializationCacheEntry where T : IXmlSerializable, new()
    {
        public override bool CanSerialize(object? value)
        {
            return value is T;
        }

        public override void Serialize(XmlWriter xmlWriter, object? value)
        {
            if (value is not T t)
            {
                return;
            }

            t.WriteXml(xmlWriter);
        }
    }

    public class SceneSerializer2
    {
        public void Serialize(XmlWriter writer, Scene scene)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ComponentAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T> : Attribute where T : IXmlSerializable, new()
    {
        public ComponentAttribute()
        {
            ComponentRegistry.Register<T>();
        }
    }
}