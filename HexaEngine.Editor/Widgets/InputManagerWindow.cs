namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.UI;
    using HexaEngine.Input;

    public class InputMap
    {
        public InputMap(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public List<VirtualAxis1> VirtualAxes { get; } = new();

        public void Save(string path)
        {
            var serializer = JsonSerializer.Create();

            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public void LoadFrom(string path)
        {
            var serializer = JsonSerializer.Create();

            TextReader reader = new StreamReader(path);
            serializer.Populate(reader, this);
            reader.Close();
        }

        public static InputMap Read(string path)
        {
            var serializer = JsonSerializer.Create();

            TextReader reader = new StreamReader(path);
            JsonTextReader jsonTextReader = new(reader);
            var inputMap = serializer.Deserialize<InputMap>(jsonTextReader) ?? new("NULL");
            reader.Close();
            return inputMap;
        }
    }

    public class InputManagerWindow : EditorWindow
    {
        private readonly List<VirtualAxis1> virtualAxes = new();

        private VirtualAxis1? currentAxis = null;
        private int currentBinding = -1;

        private bool recordKey;
        private bool recordMouseButton;

        public InputManagerWindow()
        {
#if DEBUG
            IsShown = true;
#endif
        }

        protected override string Name { get; } = "Input Manager";

        protected override void OnShown()
        {
            Keyboard.KeyDown += OnKeyDown;
            Mouse.ButtonDown += OnButtonDown;
            base.OnShown();
        }

        protected override void OnClosed()
        {
            Mouse.ButtonDown -= OnButtonDown;
            Keyboard.KeyDown -= OnKeyDown;
            base.OnClosed();
        }

        private void OnButtonDown(object? sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            if (!recordMouseButton)
            {
                return;
            }
            recordMouseButton = false;

            if (currentAxis == null || currentBinding == -1)
            {
                return;
            }

            var binding = currentAxis.Bindings[currentBinding];
            if (binding.Type != VirtualAxisBindingType.MouseButton)
            {
                return;
            }

            binding.MouseButtonBinding.Button = e.Button;
            currentAxis.Bindings[currentBinding] = binding;
        }

        private void OnKeyDown(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            if (!recordKey)
            {
                return;
            }
            recordKey = false;

            if (currentAxis == null || currentBinding == -1)
            {
                return;
            }

            var binding = currentAxis.Bindings[currentBinding];
            if (binding.Type != VirtualAxisBindingType.KeyboardKey)
            {
                return;
            }

            binding.KeyboardKeyBinding.Key = e.KeyCode;
            currentAxis.Bindings[currentBinding] = binding;
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            bool comboOpen;
            if (currentAxis != null)
            {
                comboOpen = ImGui.BeginCombo("##T", currentAxis.Name);
            }
            else
            {
                comboOpen = ImGui.BeginCombo("##T", (byte*)null);
            }

            if (comboOpen)
            {
                for (int i = 0; i < virtualAxes.Count; i++)
                {
                    var axis = virtualAxes[i];
                    bool isSelected = currentAxis == axis;
                    if (ImGui.Selectable(axis.Name, isSelected))
                    {
                        currentAxis = axis;
                        currentBinding = -1;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();

            if (ImGui.Button("\xE710##Axis")) // Add
            {
                AddNew();
            }

            if (currentAxis == null)
            {
                return;
            }

            ImGui.InputText("Name", ref currentAxis.Name, 1028);

            ImGui.SeparatorText("Bindings");

            ImGui.SameLine();

            if (ImGui.Button("\xE710##Binding"))
            {
                AddNewBinding();
            }

            ImGui.BeginTable("##Table", 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.BeginListBox("##List"))
            {
                for (int i = 0; i < currentAxis.Bindings.Count; i++)
                {
                    var binding = currentAxis.Bindings[i];
                    string? name = binding.Type switch
                    {
                        VirtualAxisBindingType.KeyboardKey => $"Keyboard Key: {binding.KeyboardKeyBinding.Key}##{i}",
                        VirtualAxisBindingType.MouseButton => $"Mouse Button: {binding.MouseButtonBinding.Button}##{i}",
                        VirtualAxisBindingType.JoystickButton => $"Joystick Button: {binding.JoystickButtonBinding.Button}##{i}",
                        VirtualAxisBindingType.GamepadButton => $"Gamepad Button: {binding.GamepadButtonBinding.Button}##{i}",
                        VirtualAxisBindingType.GamepadTouch => $"Gamepad Touch: {binding.DeviceId}##{i}",
                        VirtualAxisBindingType.GamepadTouchPressure => $"Gamepad Touch Pressure: {binding.DeviceId}##{i}",
                        VirtualAxisBindingType.Touch => $"Touch: {binding.DeviceId}##{i}",
                        VirtualAxisBindingType.TouchPressure => $"Touch Pressure: {binding.DeviceId}##{i}",
                        VirtualAxisBindingType.MouseWheel => $"Mouse Wheel: {binding.MouseWheelBinding.Wheel}##{i}",
                        VirtualAxisBindingType.MouseMovement => $"Mouse Move: {binding.MouseMovementBinding.Axis}##{i}",
                        VirtualAxisBindingType.JoystickBall => $"Joystick Ball: {binding.JoystickBallBinding.Ball}, {binding.JoystickBallBinding.Axis}##{i}",
                        VirtualAxisBindingType.JoystickAxis => $"Joystick Axis: {binding.JoystickAxisBinding.Axis}##{i}",
                        VirtualAxisBindingType.JoystickHat => $"Joystick Hat: {binding.JoystickHatBinding.Hat}##{i}",
                        VirtualAxisBindingType.GamepadAxis => $"Gamepad Axis: {binding.GamepadAxisBinding.Axis}##{i}",
                        VirtualAxisBindingType.GamepadTouchMovement => $"Gamepad Touch Move: {binding.GamepadTouchMovementBinding.Axis}##{i}",
                        VirtualAxisBindingType.GamepadSensor => $"Gamepad Sensor: {binding.MouseMovementBinding.Axis}##{i}",
                        VirtualAxisBindingType.TouchMovement => $"Touch Move: {binding.TouchMovementBinding.Axis}##{i}",
                        _ => "unknown",
                    };
                    var selected = currentBinding == i;
                    if (ImGui.Selectable(name, selected))
                    {
                        currentBinding = i;
                    }

                    BindingContextMenu(name, currentAxis, binding, ref i);
                }

                ImGui.EndListBox();
            }

            ImGui.TableSetColumnIndex(1);

            ImGui.BeginGroup();

            if (currentBinding != -1)
            {
                var binding = currentAxis.Bindings[currentBinding];
                bool changed = false;
                changed |= ComboEnumHelper<VirtualAxisBindingType>.Combo("Type", ref binding.Type);

                ImGui.Separator();

                changed |= ImGui.InputInt("DeviceId", ref binding.DeviceId);
                TooltipHelper.Tooltip("Set to -1 for all devices");

                changed |= ImGui.Checkbox("Invert", ref binding.Invert);
                TooltipHelper.Tooltip("Invert the value eg. when key w is pressed value == -1");

                ImGui.Separator();

                switch (binding.Type)
                {
                    case VirtualAxisBindingType.KeyboardKey:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Key>.Combo("Key", ref binding.KeyboardKeyBinding.Key);
                        ImGui.SameLine();
                        if (ImGui.Button("\xE7C8"))
                        {
                            recordKey = true;
                        }
                        break;

                    case VirtualAxisBindingType.MouseButton:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<MouseButton>.Combo("Button", ref binding.MouseButtonBinding.Button);
                        ImGui.SameLine();
                        if (ImGui.Button("\xE7C8"))
                        {
                            recordMouseButton = true;
                        }
                        break;

                    case VirtualAxisBindingType.JoystickButton:
                        ImGui.SetNextItemWidth(100);
                        changed |= ImGui.InputInt("Button", ref binding.JoystickButtonBinding.Button);
                        break;

                    case VirtualAxisBindingType.GamepadButton:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<GamepadButton>.Combo("Button", ref binding.GamepadButtonBinding.Button);
                        break;

                    case VirtualAxisBindingType.GamepadTouch:
                        break;

                    case VirtualAxisBindingType.GamepadTouchPressure:
                        break;

                    case VirtualAxisBindingType.Touch:
                        break;

                    case VirtualAxisBindingType.TouchPressure:
                        break;

                    case VirtualAxisBindingType.MouseWheel:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.MouseWheelBinding.Wheel);
                        break;

                    case VirtualAxisBindingType.MouseMovement:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.MouseMovementBinding.Axis);
                        break;

                    case VirtualAxisBindingType.JoystickBall:
                        ImGui.SetNextItemWidth(100);
                        changed |= ImGui.InputInt("Button", ref binding.JoystickBallBinding.Ball);
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.JoystickBallBinding.Axis);
                        break;

                    case VirtualAxisBindingType.JoystickAxis:
                        ImGui.SetNextItemWidth(100);
                        changed |= ImGui.InputInt("Axis", ref binding.JoystickAxisBinding.Axis);
                        break;

                    case VirtualAxisBindingType.JoystickHat:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<JoystickHatState>.Combo("Hat", ref binding.JoystickHatBinding.Hat);
                        break;

                    case VirtualAxisBindingType.GamepadAxis:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<GamepadAxis>.Combo("Axis", ref binding.GamepadAxisBinding.Axis);
                        break;

                    case VirtualAxisBindingType.GamepadTouchMovement:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.GamepadTouchMovementBinding.Axis);
                        break;

                    case VirtualAxisBindingType.GamepadSensor:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.GamepadSensorBinding.Axis);
                        break;

                    case VirtualAxisBindingType.TouchMovement:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.TouchMovementBinding.Axis);
                        break;
                }

                if (changed)
                {
                    currentAxis.Bindings[currentBinding] = binding;
                }
            }

            ImGui.EndGroup();

            ImGui.EndTable();
        }

        private void BindingContextMenu(string name, VirtualAxis1 axis, VirtualAxisBinding binding, ref int index)
        {
            if (ImGui.BeginPopupContextItem(name))
            {
                if (ImGui.MenuItem("\xE74D Delete"))
                {
                    if (currentBinding == index)
                    {
                        currentBinding = -1;
                    }
                    axis.Bindings.RemoveAt(index);
                    index--;
                }
                ImGui.EndPopup();
            }
        }

        private void AddNewBinding()
        {
            currentAxis?.Bindings.Add(new() { DeviceId = -1 });
        }

        private void AddNew()
        {
            VirtualAxis1 virtualAxis = new(GetNewUniqueName("New Axis"));
            virtualAxes.Add(virtualAxis);
        }

        private bool Exists(string name)
        {
            for (var i = 0; i < virtualAxes.Count; i++)
            {
                var v = virtualAxes[i];
                if (v.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetNewUniqueName(string name)
        {
            string currentName = name;
            int counter = 1;
            while (Exists(currentName))
            {
                currentName = $"{currentName} {counter}";
                counter++;
            }
            return currentName;
        }
    }
}