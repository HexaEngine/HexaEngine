using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes
{
    using Silk.NET.Core.Native;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public static class SceneExensions
    {
        public static bool AddIfIs<T>(this IList<T> list, GameObject obj)
        {
            if (obj is T t)
            {
                list.Add(t);
                return true;
            }
            return false;
        }

        public static bool RemoveIfIs<T>(this IList<T> list, GameObject obj)
        {
            if (obj is T t)
            {
                list.Remove(t);
                return true;
            }
            return false;
        }

        public static bool RemoveIfIs<T>(this IList<T> list, GameObject obj, out int index)
        {
            if (obj is T t)
            {
                index = list.IndexOf(t);
                if (index == -1)
                {
                    return false;
                }

                list.RemoveAt(index);
                return true;
            }
            index = -1;
            return false;
        }

        public static void EnqueueComponentIfIs<T>(this Queue<T> queue, GameObject obj) where T : IComponent
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    queue.Enqueue(t);
                }
            }
        }

        public static void EnqueueComponentIfIs<T>(this ConcurrentQueue<T> queue, GameObject obj) where T : IComponent
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    queue.Enqueue(t);
                }
            }
        }

        public static void AddComponentIfIs<T>(this IList<T> list, GameObject obj) where T : IComponent
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    list.Add(t);
                }
            }
        }

        public static void AddComponentIfIs<T>(this IList<T> list, GameObject obj, bool awake) where T : IComponent
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    if (awake)
                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        t.Awake(null, obj);
                    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    list.Add(t);
                }
            }
        }

        public static void RemoveComponentIfIs<T>(this IList<T> list, GameObject obj) where T : IComponent
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    list.Remove(t);
                }
            }
        }

        public static void RemoveComponentIfIs<T>(this IList<T> list, GameObject obj, bool destroy) where T : IComponent
        {
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    if (destroy)
                    {
                        t.Destory();
                    }

                    list.Remove(t);
                }
            }
        }

        public static bool AddComponentIfIs<T>(this GameObject obj, Action<T> add) where T : IComponent
        {
            bool result = false;
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    add(t);
                    result = true;
                }
            }
            return result;
        }

        public static bool RemoveComponentIfIs<T>(this GameObject obj, Action<T> remove) where T : IComponent
        {
            bool result = false;
            for (int i = 0; i < obj.Components.Count; i++)
            {
                if (obj.Components[i] is T t)
                {
                    remove(t);
                    result = true;
                }
            }
            return result;
        }
    }
}