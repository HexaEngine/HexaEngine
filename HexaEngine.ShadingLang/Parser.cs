namespace HexaEngine.ShadingLang
{
    using HexaEngine.ShadingLang.LexicalAnalysis;
    using HexaEngine.ShadingLang.Text;
    using System;
    using System.Collections;
    using System.Reflection;

    public static class ReflectionCache
    {
        private static readonly Dictionary<Type, ReflectionCacheEntry> entries = [];

        public static ReflectionCacheEntry GetEntry<T>()
        {
            return GetEntry(typeof(T));
        }

        public static ReflectionCacheEntry GetEntry(Type type)
        {
            if (!entries.TryGetValue(type, out var entry))
            {
                entry = new(type);
                entries.Add(type, entry);
            }
            return entry;
        }
    }

    public struct ReflectionCacheEntry
    {
        public Type Type;
        public (PropertyInfo prop, string name, bool codeblock)[] Properties;

        public ReflectionCacheEntry(Type type)
        {
            Type = type;
            Properties = [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => (x, x.GetCustomAttribute<HXSLNameAttribute>()?.Name ?? x.Name, x.GetCustomAttribute<HXSLCodeblockAttribute>() != null))];
        }

        private readonly (PropertyInfo prop, bool codeblock) FindProperty(ReadOnlySpan<char> chars)
        {
            foreach (var prop in Properties)
            {
                if (chars.SequenceEqual(prop.name))
                {
                    return (prop.prop, prop.codeblock);
                }
            }
            throw new InvalidOperationException("Property not found.");
        }

        public readonly void Parse(object instance, ref ObjectReader parser)
        {
            while (parser.ParseProperties(out var identifier))
            {
                ParseInner(instance, ref parser, identifier);
                parser.EndProperty();
            }
        }

        public readonly void ParseOuter(object instance, ref ObjectReader parser)
        {
            while (parser.ParseOuterProperties(out var identifier))
            {
                ParseInner(instance, ref parser, identifier);
                parser.EndProperty();
            }
        }

        private readonly void ParseInner(object instance, ref ObjectReader parser, TextSpan identifier)
        {
            var (property, codeblock) = FindProperty(identifier.AsSpan());
            if (property == null) return;
            object? existingValue = property.CanRead ? property.GetValue(instance) : null;
            object? value = ParseInnerCore(ref parser, property.PropertyType, codeblock, existingValue, property.CanWrite);
            if (property.CanWrite)
            {
                property.SetValue(instance, value);
            }
        }

        private static bool TryParsePrimitive(ref ObjectReader reader, Type type, bool codeblock, out object? value)
        {
            if (type == typeof(string))
            {
                value = codeblock ? reader.ParseCodeblock() : reader.ParseString();
                return true;
            }
            else if (type == typeof(byte))
            {
                value = (byte)reader.ParseInt();
                return true;
            }
            else if (type == typeof(int))
            {
                value = reader.ParseInt();
                return true;
            }
            else if (type == typeof(float))
            {
                value = reader.ParseFloat();
                return true;
            }
            else if (type == typeof(bool))
            {
                value = reader.ParseBool();
                return true;
            }
            else if (type.IsEnum)
            {
                value = reader.ParseEnum(type);
                return true;
            }

            value = null;
            return false;
        }

        private static object? ParseInnerCore(ref ObjectReader parser, Type type, bool codeblock, object? existingValue, bool canWrite)
        {
            if (TryParsePrimitive(ref parser, type, codeblock, out var value))
            {
                return value;
            }
            else if (type.IsAssignableTo(typeof(IDictionary)) && type.IsGenericType)
            {
                IDictionary? dictionary = (IDictionary?)existingValue;
                if (!canWrite && dictionary == null)
                {
                    throw new Exception("Property with an Dictionary type must have a setter if initial value is null.");
                }
                else if (canWrite && dictionary == null)
                {
                    dictionary = (IDictionary)Activator.CreateInstance(type)!;
                }

                if (type.GenericTypeArguments[0] != typeof(string))
                {
                    throw new NotSupportedException("The type of the Dictionary key must be string.");
                }
                var valueType = type.GenericTypeArguments[1];

                parser.BeginArray();
                while (parser.ParseArray(out var identifier))
                {
                    var valueItem = ParseInnerCore(ref parser, valueType, false, null, true);
                    dictionary!.Add(identifier.ToString(), valueItem);
                    parser.EndArrayItem();
                }
                parser.EndArray();
                return dictionary;
            }
            else if (type.IsAssignableTo(typeof(IList)) && type.IsGenericType)
            {
                IList? collection = (IList?)existingValue;
                if (!canWrite && collection == null)
                {
                    throw new Exception("Property with an Collection type must have a setter if initial value is null.");
                }
                else if (canWrite && collection == null)
                {
                    collection = (IList)Activator.CreateInstance(type)!;
                }
                var innerType = type.GenericTypeArguments[0];
                var named = innerType.IsAssignableTo(typeof(IHXSLName));

                parser.BeginArray();
                if (named)
                {
                    while (parser.ParseArray(out var arrayIdentifer))
                    {
                        var nameInterface = (IHXSLName)ParseInnerCore(ref parser, innerType, false, null, true)!;
                        nameInterface.Name = arrayIdentifer.ToString();
                        collection!.Add(nameInterface);
                        parser.EndArrayItem();
                    }
                }
                else
                {
                    while (parser.ParseArray())
                    {
                        var arrayItem = ParseInnerCore(ref parser, innerType, false, null, true)!;
                        collection!.Add(arrayItem);
                        parser.EndArrayItem();
                    }
                }
                parser.EndArray();
                return collection;
            }
            else
            {
                var propInstance = Activator.CreateInstance(type);
                var entry = ReflectionCache.GetEntry(type);
                parser.BeginObject();
                entry.Parse(propInstance, ref parser);
                parser.EndObject();
                return propInstance;
            }
        }
    }

    public unsafe class Parser
    {
        public static HXSLModule Parse(string input)
        {
            fixed (char* pText = input)
            {
                TokenStream stream = new(new(pText, input.Length), LexerConfig.HSLConfig);
                return Parse(ref stream);
            }
        }

        public static HXSLModule Parse(ref TokenStream stream)
        {
            HXSLModule result = new();
            stream.Advance();
            ObjectReader parser = new(ref stream);
            ReflectionCache.GetEntry<HXSLModule>().ParseOuter(result, ref parser);
            return result;
        }
    }
}