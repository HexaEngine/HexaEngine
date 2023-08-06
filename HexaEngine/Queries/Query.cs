namespace HexaEngine.Queries
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public enum ChangeAction
    {
        Add,
        Remove,
        Refresh
    }

    public abstract class Query : IDisposable
    {
        private bool disposedValue;

        public abstract void Execute(IList<GameObject> objects);

        public abstract void Add(GameObject gameObject);

        public abstract void Update(GameObject gameObject, IComponent component, ChangeAction action);

        public abstract void Remove(GameObject gameObject);

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        ~Query()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class Query<T> : Query, IList<T> where T : GameObject
    {
        private readonly List<T> objects = new();

        public T this[int index] { get => ((IList<T>)objects)[index]; set => ((IList<T>)objects)[index] = value; }

        public int Count => ((ICollection<T>)objects).Count;

        public bool IsReadOnly => ((ICollection<T>)objects).IsReadOnly;

        public override void Add(GameObject gameObject)
        {
            if (gameObject is T t && !objects.Contains(t))
            {
                objects.Add(t);
            }
        }

        public void Add(T item)
        {
            ((ICollection<T>)objects).Add(item);
        }

        public void Clear()
        {
            ((ICollection<T>)objects).Clear();
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)objects).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)objects).CopyTo(array, arrayIndex);
        }

        public override void Execute(IList<GameObject> objects)
        {
            this.objects.AddRange(objects.Where(obj => obj is T).Cast<T>());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)objects).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)objects).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)objects).Insert(index, item);
        }

        public override void Remove(GameObject gameObject)
        {
            if (gameObject is T t)
            {
                objects.Remove(t);
            }
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)objects).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)objects).RemoveAt(index);
        }

        public override void Update(GameObject gameObject, IComponent component, ChangeAction action)
        {
        }

        protected override void DisposeCore()
        {
            objects.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)objects).GetEnumerator();
        }
    }

    public class Query<T, T1> : Query, IList<(T, T1)> where T : GameObject where T1 : IComponent
    {
        private readonly List<(T, T1)> values = new();

        public (T, T1) this[int index] { get => ((IList<(T, T1)>)values)[index]; set => ((IList<(T, T1)>)values)[index] = value; }

        public int Count => ((ICollection<(T, T1)>)values).Count;

        public bool IsReadOnly => ((ICollection<(T, T1)>)values).IsReadOnly;

        public override void Add(GameObject gameObject)
        {
            if (gameObject is T t)
            {
                foreach (var t1 in gameObject.GetComponents<T1>())
                {
                    var tuple = (t, t1);
                    if (!values.Contains(tuple))
                        values.Add(tuple);
                }
            }
        }

        public void Add((T, T1) item)
        {
            ((ICollection<(T, T1)>)values).Add(item);
        }

        public void Clear()
        {
            ((ICollection<(T, T1)>)values).Clear();
        }

        public bool Contains((T, T1) item)
        {
            return ((ICollection<(T, T1)>)values).Contains(item);
        }

        public void CopyTo((T, T1)[] array, int arrayIndex)
        {
            ((ICollection<(T, T1)>)values).CopyTo(array, arrayIndex);
        }

        public override void Execute(IList<GameObject> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                Add(objects[i]);
            }
        }

        public IEnumerator<(T, T1)> GetEnumerator()
        {
            return ((IEnumerable<(T, T1)>)values).GetEnumerator();
        }

        public int IndexOf((T, T1) item)
        {
            return ((IList<(T, T1)>)values).IndexOf(item);
        }

        public void Insert(int index, (T, T1) item)
        {
            ((IList<(T, T1)>)values).Insert(index, item);
        }

        public override void Remove(GameObject gameObject)
        {
            if (gameObject is T t)
            {
                foreach (var t1 in gameObject.GetComponents<T1>())
                {
                    var tuple = (t, t1);
                    values.Remove(tuple);
                }
            }
        }

        public bool Remove((T, T1) item)
        {
            return ((ICollection<(T, T1)>)values).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<(T, T1)>)values).RemoveAt(index);
        }

        public override void Update(GameObject gameObject, IComponent component, ChangeAction action)
        {
            if (gameObject is T t && component is T1 t1)
            {
                var tuple = (t, t1);
                if (action == ChangeAction.Add && !values.Contains(tuple))
                {
                    values.Add((t, t1));
                }
                else if (action == ChangeAction.Remove)
                {
                    values.Remove((t, t1));
                }
            }
        }

        protected override void DisposeCore()
        {
            values.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)values).GetEnumerator();
        }
    }

    public class Query<T, T1, T2> : Query where T : GameObject where T1 : IComponent where T2 : IComponent
    {
        private readonly List<(T, T1, T2)> values = new();

        public override void Add(GameObject gameObject)
        {
            if (gameObject is T t)
            {
                foreach (var t1 in gameObject.GetComponents<T1>())
                {
                    foreach (var t2 in gameObject.GetComponents<T2>())
                    {
                        var tuple = (t, t1, t2);
                        if (!values.Contains(tuple))
                            values.Add(tuple);
                    }
                }
            }
        }

        public override void Execute(IList<GameObject> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                Add(objects[i]);
            }
        }

        public override void Remove(GameObject gameObject)
        {
            if (gameObject is T t)
            {
                foreach (var t1 in gameObject.GetComponents<T1>())
                {
                    foreach (var t2 in gameObject.GetComponents<T2>())
                    {
                        var tuple = (t, t1, t2);
                        values.Remove(tuple);
                    }
                }
            }
        }

        public override void Update(GameObject gameObject, IComponent component, ChangeAction action)
        {
            {
                if (gameObject is T t && component is T2 t2)
                {
                    foreach (T1 t1 in gameObject.GetComponents<T1>())
                    {
                        var tuple = (t, t1, t2);

                        if (action == ChangeAction.Add && !values.Contains(tuple))
                        {
                            values.Add(tuple);
                        }
                        else if (action == ChangeAction.Remove)
                        {
                            values.Remove(tuple);
                        }
                    }
                }
            }

            {
                if (gameObject is T t && component is T1 t1)
                {
                    foreach (T2 t2 in gameObject.GetComponents<T2>())
                    {
                        var tuple = (t, t1, t2);

                        if (action == ChangeAction.Add && !values.Contains(tuple))
                        {
                            values.Add(tuple);
                        }
                        else if (action == ChangeAction.Remove)
                        {
                            values.Remove(tuple);
                        }
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            values.Clear();
        }
    }
}