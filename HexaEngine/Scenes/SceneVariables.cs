namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Thread safe wrapper around a dictionary with helper methods integrated
    /// </summary>
    /// <seealso cref="IDictionary&lt;string, string&gt;" />
    public class SceneVariables : ObservableDictionary<string, string>
    {
        private readonly object _lock = new();

        public override void Add(string key, string value)
        {
            lock (_lock)
            {
                base.Add(key, value);
            }
        }

        public bool TryAdd(string key, string value)
        {
            lock (_lock)
            {
                if (base.ContainsKey(key))
                {
                    return false;
                }

                base.Add(key, value);
                return true;
            }
        }

        public string Get(string key)
        {
            lock (_lock)
            {
                return this[key];
            }
        }

        public void AddOrUpdate(string key, string value)
        {
            lock (_lock)
            {
                if (base.ContainsKey(key))
                {
                    this[key] = value;
                }
                else
                {
                    base.Add(key, value);
                }
            }
        }

        public override bool ContainsKey(string key)
        {
            lock (_lock)
            {
                return base.ContainsKey(key);
            }
        }

        public override bool Remove(string key)
        {
            lock (_lock)
            {
                return base.Remove(key);
            }
        }

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out string? value)
        {
            lock (_lock)
            {
                return base.TryGetValue(key, out value);
            }
        }

        public override void Add(KeyValuePair<string, string> item)
        {
            lock (_lock)
            {
                base.Add(item);
            }
        }

        public override void Clear()
        {
            lock (_lock)
            {
                base.Clear();
            }
        }

        public override bool Contains(KeyValuePair<string, string> item)
        {
            lock (_lock)
            {
                return base.Contains(item);
            }
        }

        public override void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            lock (_lock)
            {
                ((ICollection<KeyValuePair<string, string>>)this).CopyTo(array, arrayIndex);
            }
        }

        public override bool Remove(KeyValuePair<string, string> item)
        {
            lock (_lock)
            {
                return ((ICollection<KeyValuePair<string, string>>)this).Remove(item);
            }
        }

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            lock (_lock)
            {
                return base.GetEnumerator();
            }
        }
    }
}