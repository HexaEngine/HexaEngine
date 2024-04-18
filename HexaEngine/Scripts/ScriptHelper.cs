namespace HexaEngine.Scripts
{
    using HexaEngine.Scenes;

    public static class ScriptHelper
    {
        public static T? FindScript<T>(this GameObject gameObject) where T : ScriptBehaviour
        {
            foreach (var component in gameObject.GetComponents<ScriptComponent>())
            {
                if (component.Instance is T scriptInstance)
                {
                    return scriptInstance;
                }
            }

            return null;
        }

        public static T? FindScript<T>(this GameObject gameObject, Func<T, bool> selector) where T : ScriptBehaviour
        {
            foreach (var component in gameObject.GetComponents<ScriptComponent>())
            {
                if (component.Instance is T scriptInstance && selector(scriptInstance))
                {
                    return scriptInstance;
                }
            }

            return null;
        }
    }
}