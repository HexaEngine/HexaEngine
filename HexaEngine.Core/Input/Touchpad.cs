namespace HexaEngine.Core.Input
{
    using Silk.NET.SDL;

    public class Touch
    {
        private static readonly Sdl sdl = Sdl.GetApi();

        public Touch()
        {
            var touchdevCount = sdl.GetNumTouchDevices();
            for (int i = 0; i < touchdevCount; i++)
            {
                var id = sdl.GetTouchDevice(i);
            }
        }
    }

    public class TouchDevice
    {
    }
}