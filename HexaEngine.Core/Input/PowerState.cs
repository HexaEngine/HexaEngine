namespace HexaEngine.Core.Input
{
    [Flags]
    public enum PowerState
    {
        /// <summary>
        /// Error determining power status.
        /// </summary>
        Error = -1,

        /// <summary>
        /// Cannot determine power status.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Not plugged in, running on the battery.
        /// </summary>
        OnBattery = 1,

        /// <summary>
        /// Plugged in, no battery available.
        /// </summary>
        NoBattery = 2,

        /// <summary>
        /// Plugged in, charging battery.
        /// </summary>
        Charging = 3,

        /// <summary>
        /// Plugged in, battery charged.
        /// </summary>
        Charged = 4
    }
}