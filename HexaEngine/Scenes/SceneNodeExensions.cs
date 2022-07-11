namespace HexaEngine.Scenes
{
    using System.Collections.Generic;

    public static class SceneNodeExensions
    {
        public static void AddIfIs<T>(this IList<T> list, object obj)
        {
            if (obj is T t)
            {
                list.Add(t);
            }
        }

        public static void RemoveIfIs<T>(this IList<T> list, object obj)
        {
            if (obj is T t)
            {
                list.Remove(t);
            }
        }
    }
}