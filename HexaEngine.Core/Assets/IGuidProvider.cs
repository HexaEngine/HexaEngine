namespace HexaEngine.Core.Assets
{
    using System;

    public interface IGuidProvider
    {
        public Guid ParentGuid { get; }

        Guid GetGuid(string name);
    }
}