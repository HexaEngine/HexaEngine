namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;

    public interface IInputManager : IDisposable
    {
        public InputEventBuffer InputBuffer { get; }

        float GetAxis(string name);

        float GetAxisRaw(string name);

        bool GetButton(string name);

        bool GetButtonDown(string name);

        bool GetButtonUp(string name);

        bool GetKey(Key key);

        bool GetKeyDown(Key key);

        bool GetKeyUp(Key key);

        bool GetMouseButton(MouseButton button);

        bool GetMouseButtonDown(MouseButton button);

        bool GetMouseButtonUp(MouseButton button);

        public void Update();
    }
}