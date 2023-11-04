namespace HexaEngine.Core.Debugging
{
    /// <summary>
    /// Represents an interface for a terminal component.
    /// </summary>
    public interface ITerminal
    {
        /// <summary>
        /// Gets a value indicating whether the terminal is currently shown.
        /// </summary>
        bool Shown { get; }

        /// <summary>
        /// Draws the terminal.
        /// </summary>
        void Draw();

        /// <summary>
        /// Gives focus to the terminal, allowing user interaction.
        /// </summary>
        void Focus();

        /// <summary>
        /// Closes and hides the terminal.
        /// </summary>
        void Close();

        /// <summary>
        /// Shows and makes the terminal visible.
        /// </summary>
        void Show();
    }
}