namespace HexaEngine.UI.Markup
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using HexaEngine.UI.Xaml;

    public class XmlnsDictionary : IDictionary<string, string>, IDictionary, IXamlNamespaceResolver
    {
        private readonly Stack<Dictionary<string, string>> scopeStack = [];
        private bool @sealed;
        private Dictionary<string, string> prefixToNamespace = [];

        public string this[string key] { get => prefixToNamespace[key]; set => prefixToNamespace[key] = value; }

        public ICollection<string> Keys => prefixToNamespace.Keys;

        public ICollection<string> Values => prefixToNamespace.Values;

        public int Count => prefixToNamespace.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        ICollection IDictionary.Keys => prefixToNamespace.Keys;

        ICollection IDictionary.Values => prefixToNamespace.Values;

        public bool IsSynchronized => false;

        public object SyncRoot => prefixToNamespace;

        public object? this[object key] { get => ((IDictionary)prefixToNamespace)[key]; set => ((IDictionary)prefixToNamespace)[key] = value; }

        public bool Sealed => @sealed;

        public string DefaultNamespace()
        {
            return "http://hexaengine.com/ui/v0/xaml";
        }

        public void Seal()
        {
            @sealed = true;
        }

        public void PopScope()
        {
            if (@sealed)
                throw new InvalidOperationException("Instance was sealed");
            prefixToNamespace = scopeStack.Pop();
        }

        public void PushScope()
        {
            if (@sealed)
                throw new InvalidOperationException("Instance was sealed");
            Dictionary<string, string> next = prefixToNamespace.ToDictionary();
            scopeStack.Push(prefixToNamespace);
            prefixToNamespace = next;
        }

        public void Add(string key, string value)
        {
            prefixToNamespace.Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            prefixToNamespace.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            prefixToNamespace.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return prefixToNamespace.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return prefixToNamespace.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>)prefixToNamespace).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return prefixToNamespace.GetEnumerator();
        }

        public string GetNamespace(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return DefaultNamespace();
            }
            return prefixToNamespace[prefix];
        }

        public string? LookupNamespace(string prefix)
        {
            prefixToNamespace.TryGetValue(prefix, out string? @namespace);
            return @namespace;
        }

        public string? LookupPrefix(string xmlNamespace)
        {
            foreach (KeyValuePair<string, string> keyValuePair in this)
            {
                if (keyValuePair.Value == xmlNamespace)
                {
                    return keyValuePair.Key;
                }
            }
            return null;
        }

        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            foreach (KeyValuePair<string, string> keyValuePair in this)
            {
                yield return new NamespaceDeclaration(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public bool Remove(string key)
        {
            return prefixToNamespace.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return prefixToNamespace.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return prefixToNamespace.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return prefixToNamespace.GetEnumerator();
        }

        public void Add(object key, object? value)
        {
            ((IDictionary)prefixToNamespace).Add(key, value);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)prefixToNamespace).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)prefixToNamespace).GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)prefixToNamespace).Remove(key);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)prefixToNamespace).CopyTo(array, index);
        }
    }
}