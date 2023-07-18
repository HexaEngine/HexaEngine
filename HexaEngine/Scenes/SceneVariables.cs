﻿namespace HexaEngine.Scenes
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
        public override void Add(string key, string value)
        {
            lock (this)
            {
                base.Add(key, value);
            }
        }

        public bool TryAdd(string key, string value)
        {
            lock (this)
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
            lock (this)
            {
                return this[key];
            }
        }

        public void AddOrUpdate(string key, string value)
        {
            lock (this)
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
            lock (this)
            {
                return base.ContainsKey(key);
            }
        }

        public override bool Remove(string key)
        {
            lock (this)
            {
                return base.Remove(key);
            }
        }

#pragma warning disable CS8765 // Nullability of type of parameter 'value' doesn't match overridden member (possibly because of nullability attributes).

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
#pragma warning restore CS8765 // Nullability of type of parameter 'value' doesn't match overridden member (possibly because of nullability attributes).
        {
            lock (this)
            {
                return base.TryGetValue(key, out value);
            }
        }

        public override void Add(KeyValuePair<string, string> item)
        {
            lock (this)
            {
                base.Add(item);
            }
        }

        public override void Clear()
        {
            lock (this)
            {
                base.Clear();
            }
        }

        public override bool Contains(KeyValuePair<string, string> item)
        {
            lock (this)
            {
                return base.Contains(item);
            }
        }

        public override void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            lock (this)
            {
                ((ICollection<KeyValuePair<string, string>>)this).CopyTo(array, arrayIndex);
            }
        }

        public override bool Remove(KeyValuePair<string, string> item)
        {
            lock (this)
            {
                return ((ICollection<KeyValuePair<string, string>>)this).Remove(item);
            }
        }

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            lock (this)
            {
                return base.GetEnumerator();
            }
        }
    }
}