namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;

    public static class Input
    {
        public static IInputManager Current { get; set; } = new InputManager();

        public static float GetAxis(string name)
        {
            return Current.GetAxis(name);
        }

        public static float GetAxisRaw(string name)
        {
            return Current.GetAxisRaw(name);
        }

        public static bool GetButton(string name)
        {
            return Current.GetButton(name);
        }

        public static bool GetButtonDown(string name)
        {
            return Current.GetButtonDown(name);
        }

        public static bool GetButtonUp(string name)
        {
            return Current.GetButtonUp(name);
        }

        public static bool GetKey(Key key)
        {
            return Current.GetKey(key);
        }

        public static bool GetKeyDown(Key key)
        {
            return Current.GetKeyDown(key);
        }

        public static bool GetKeyUp(Key key)
        {
            return Current.GetKeyUp(key);
        }

        public static bool GetMouseButton(MouseButton mouseButton)
        {
            return Current.GetMouseButton(mouseButton);
        }

        public static bool GetMouseButtonDown(MouseButton mouseButton)
        {
            return Current.GetMouseButtonDown(mouseButton);
        }

        public static bool GetMouseButtonUp(MouseButton mouseButton)
        {
            return Current.GetMouseButtonUp(mouseButton);
        }
    }
}