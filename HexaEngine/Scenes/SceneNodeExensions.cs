namespace HexaEngine.Scenes
{
    using HexaEngine.Objects;
    using System.Collections.Generic;

    public static class SceneNodeExensions
    {
        public static void AddIfIs<T>(this IList<T> list, GameObject obj)
        {
            if (obj is T t)
            {
                list.Add(t);
            }
        }

        public static void RemoveIfIs<T>(this IList<T> list, GameObject obj)
        {
            if (obj is T t)
            {
                list.Remove(t);
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