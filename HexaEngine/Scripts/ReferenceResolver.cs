namespace HexaEngine.Scripts
{
    using HexaEngine.Scenes;
    using System;

    public class ScriptReferenceResolver
    {
        public virtual object? ResolveReference(Scene scene, Type type, Guid guid)
        {
            if (type.IsAssignableTo(typeof(GameObject)))
            {
                return scene.FindByGuid(guid);
            }
            if (type.IsAssignableTo(typeof(IComponent)))
            {
                return scene.FindComponentByGuid(guid);
            }
            if (type.IsAssignableTo(typeof(ScriptBehaviour)))
            {
                return ((ScriptComponent?)scene.FindComponentByGuid(guid))?.Instance;
            }

            return null;
        }
    }
}