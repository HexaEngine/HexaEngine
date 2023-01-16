namespace HexaEngine.Core.Scenes
{
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
                list.RemoveAt(index);
                return true;
            }
            index = -1;
            return false;
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
    }
}