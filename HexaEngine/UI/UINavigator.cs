namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;

    public class UINavigator
    {
        private readonly IUINavigationElement current;

        public IUINavigationElement Current => current;

        public void Update()
        {
            var primary = Gamepads.Controllers.First(x => x.PlayerIndex == 0);
            if (primary != null)
            {
                var x = (ushort)(primary.AxisStates[GamepadAxis.LeftX] + short.MaxValue) / (float)ushort.MaxValue - 0.5f;
                var y = (ushort)(primary.AxisStates[GamepadAxis.LeftY] + short.MaxValue) / (float)ushort.MaxValue - 0.5f;
            }
        }
    }
}