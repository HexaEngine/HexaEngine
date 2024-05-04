namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using System.Numerics;

    public struct GamepadSensorUpdateEvent(GamepadSensorUpdateEventArgs eventArgs)
    {
        public GamepadSensorType Type = eventArgs.Type;
        public Vector3 Data = eventArgs.Vector;

        public readonly float GetAxis(int axis)
        {
            if (axis < 4)
            {
                return Data[axis];
            }
            return 0;
        }

        public readonly float GetAxis(SensorAxis axis)
        {
            return axis switch
            {
                SensorAxis.X => Data.X,
                SensorAxis.Y => Data.Y,
                SensorAxis.Z => Data.Z,
                _ => 0
            };
        }
    }
}