namespace VkTesting.Input.Events
{
    using System.Numerics;
    using VkTesting.Input;

    public class GamepadSensorUpdateEventArgs : EventArgs
    {
        public GamepadSensorUpdateEventArgs()
        {
        }

        public unsafe GamepadSensorUpdateEventArgs(GamepadSensorType type, float* data, int length)
        {
            Type = type;
            Data = data;
            Length = length;
        }

        public GamepadSensorType Type { get; internal set; }

        public unsafe float* Data { get; internal set; }

        public int Length { get; internal set; }

        public unsafe Vector3 Vector => *(Vector3*)Data;
    }
}