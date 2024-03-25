namespace HexaEngine.Volumes
{
    using HexaEngine.Editor;
    using HexaEngine.PostFx;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a proxy for an IPostFx object, allowing dynamic access to its properties.
    /// </summary>
    public class PostFxProxy : ProxyBase
    {
        public PostFxProxy(IPostFx target) : base(target)
        {
        }

        [JsonConstructor]
        public PostFxProxy(Dictionary<string, object?> data, string typeName) : base(data, typeName)
        {
        }

        [JsonIgnore]
        public bool Enabled { get => (bool)Data["Enabled"]; set => Data["Enabled"] = value; }
    }
}