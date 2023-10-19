namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using System.Numerics;

    public struct GamepadSensorUpdateEvent(GamepadSensorUpdateEventArgs eventArgs)
    {
        public GamepadSensorType Type = eventArgs.Type;
        public Vector3 Data = eventArgs.Vector;

        public float GetAxis(int axis)
        {
            if (axis < 4)
            {
                return Data[axis];
            }
            return 0;
        }
    }
}