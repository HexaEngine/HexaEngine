namespace HexaEngine.Collections
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Scenes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public interface IHasFlags<T> where T : Enum
    {
        public T Flags { get; }
    }

    public interface INotifyFlagsChanged<T> : IHasFlags<T> where T : Enum
    {
        public event Action<IHasFlags<T>, T>? FlagsChanged;
    }

    public class FlaggedList<TFlags, TItem> : IList<TItem>, IDictionary<TFlags, IList<TItem>>, IReadOnlyDictionary<TFlags, IList<TItem>> where TFlags : unmanaged, Enum where TItem : IHasFlags<TFlags>
    {
        private readonly Dictionary<TFlags, IList<TItem>> lists = new();
        private readonly List<TItem> values = new();
        private readonly TFlags[] flags;
        private readonly int[] flagValues;

        public FlaggedList()
        {
            flags = Enum.GetValues<TFlags>();
            flagValues = new int[flags.Length];
            for (int i = 0; i < flags.Length; i++)
            {
                lists.Add(flags[i], new List<TItem>());
                flagValues[i] = flags[i].AsInteger<TFlags, int>();
            }
        }

        public int Count => values.Count;

        public bool IsReadOnly => ((IList<TItem>)values).IsReadOnly;

        public ICollection<TFlags> Keys => lists.Keys;

        public ICollection<IList<TItem>> Values => lists.Values;

        IEnumerable<TFlags> IReadOnlyDictionary<TFlags, IList<TItem>>.Keys => lists.Keys;

        IEnumerable<IList<TItem>> IReadOnlyDictionary<TFlags, IList<TItem>>.Values => lists.Values;

        public IList<TItem> this[TFlags index]
        {
            get { return lists[index]; }
            set { lists[index] = value; }
        }

        public TItem this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        private void FlagsChanged(IHasFlags<TFlags> sender, TFlags e)
        {
            Remove((TItem)sender);
            Add((TItem)sender);
        }

        public void Add(TItem item)
        {
            values.Add(item);
            if (item is INotifyFlagsChanged<TFlags> flagsChanged)
            {
                flagsChanged.FlagsChanged += FlagsChanged;
            }
            int flagsA = item.Flags.AsInteger<TFlags, int>();
            for (int i = 0; i < flagValues.Length; i++)
            {
                if ((flagsA & flagValues[i]) != 0)
                {
                    lists[flags[i]].Add(item);
                }
            }
        }

        public bool Remove(TItem item)
        {
            values.Remove(item);
            if (item is INotifyFlagsChanged<TFlags> flagsChanged)
            {
                flagsChanged.FlagsChanged -= FlagsChanged;
            }
            int flagsA = item.Flags.AsInteger<TFlags, int>();
            for (int i = 0; i < flagValues.Length; i++)
            {
                if ((flagsA & flagValues[i]) != 0)
                {
                    lists[flags[i]].Remove(item);
                }
            }

            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value is INotifyFlagsChanged<TFlags> flagsChanged)
                {
                    flagsChanged.FlagsChanged -= FlagsChanged;
                }
            }
            values.Clear();
            for (int i = 0; i < flagValues.Length; i++)
            {
                lists[flags[i]].Clear();
            }
        }

        public void AddComponentIfIs<T>(GameObject obj) where T : IComponent, TItem
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    Add(t);
                }
            }
        }

        public void AddComponentIfIs<T>(GameObject obj, bool awake) where T : IComponent, TItem
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    if (awake)
                    {
                        t.GameObject = obj;
                        t.Awake();
                    }
                    Add(t);
                }
            }
        }

        public void RemoveComponentIfIs<T>(GameObject obj) where T : IComponent, TItem
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    Remove(t);
                }
            }
        }

        public void RemoveComponentIfIs<T>(GameObject obj, bool destroy) where T : IComponent, TItem
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    if (destroy)
                    {
                        t.Destroy();
                    }

                    Remove(t);
                }
            }
        }

        public int IndexOf(TItem item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, TItem item)
        {
            values.Insert(index, item);
            int flagsA = item.Flags.AsInteger<TFlags, int>();
            for (int i = 0; i < flagValues.Length; i++)
            {
                if ((flagsA & flagValues[i]) != 0)
                {
                    lists[flags[i]].Insert(index, item);

                    if (item is INotifyFlagsChanged<TFlags> flagsChanged)
                    {
                        flagsChanged.FlagsChanged -= FlagsChanged;
                    }
                }
            }
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public bool Contains(TItem item)
        {
            return values.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return ((IList<TItem>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<TItem>)values).GetEnumerator();
        }

        public void Add(TFlags key, IList<TItem> value)
        {
            lists.Add(key, value);
        }

        public bool ContainsKey(TFlags key)
        {
            return lists.ContainsKey(key);
        }

        public bool Remove(TFlags key)
        {
            return lists.Remove(key);
        }

        public bool TryGetValue(TFlags key, [MaybeNullWhen(false)] out IList<TItem> value)
        {
            return lists.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TFlags, IList<TItem>> item)
        {
            lists.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TFlags, IList<TItem>> item)
        {
            return lists.Contains(item);
        }

        public void CopyTo(KeyValuePair<TFlags, IList<TItem>>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TFlags, IList<TItem>> item)
        {
            return lists.Remove(item.Key);
        }

        IEnumerator<KeyValuePair<TFlags, IList<TItem>>> IEnumerable<KeyValuePair<TFlags, IList<TItem>>>.GetEnumerator()
        {
            return ((IDictionary<TFlags, IList<TItem>>)lists).GetEnumerator();
        }

        public void Sort(IComparer<TItem>? comparer)
        {
            for (int i = 0; i < flags.Length; i++)
            {
                ((List<TItem>)lists[flags[i]]).Sort(comparer);
            }
        }

        public void Sort(Comparison<TItem> comparison)
        {
            for (int i = 0; i < flags.Length; i++)
            {
                ((List<TItem>)lists[flags[i]]).Sort(comparison);
            }
        }

        public void Sort(int index, int count, IComparer<TItem>? comparer)
        {
            for (int i = 0; i < flags.Length; i++)
            {
                ((List<TItem>)lists[flags[i]]).Sort(index, count, comparer);
            }
        }

        public void Sort()
        {
            for (int i = 0; i < flags.Length; i++)
            {
                ((List<TItem>)lists[flags[i]]).Sort();
            }
        }
    }
}