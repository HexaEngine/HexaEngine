namespace HexaEngine.Core.Unsafes
{
    /// <summary>
    /// Represents an object that can be released to free associated resources.
    /// </summary>
    public interface IFreeable
    {
        /// <summary>
        /// Releases the object and frees any associated resources.
        /// </summary>
        void Release();
    }
}