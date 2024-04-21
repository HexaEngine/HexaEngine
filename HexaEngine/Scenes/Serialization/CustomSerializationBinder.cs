namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.Scripts;
    using Newtonsoft.Json.Serialization;

    public class CustomSerializationBinder : DefaultSerializationBinder
    {
        public override Type BindToType(string? assemblyName, string typeName)
        {
            if (assemblyName == "HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" && typeName == "HexaEngine.Components.ScriptBehaviour")
            {
                return typeof(ScriptComponent);
            }
            if (assemblyName == "HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" && typeName == "HexaEngine.Scenes.Scene+SceneRootNode")
            {
                return typeof(SceneRootNode);
            }
            return base.BindToType(assemblyName, typeName);
        }
    }
}