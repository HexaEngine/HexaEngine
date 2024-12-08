namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.UI;
    using HexaEngine.Input;

    public class InputManagerWindow : EditorWindow
    {
        private InputMap inputMap = null!;

        private VirtualAxis? currentAxis = null;
        private int currentBinding = -1;

        private bool recordKey;
        private bool recordMouseButton;

        private bool unsavedChanges = false;
        private bool unsavedDataDialogIsOpen;

        public InputManagerWindow()
        {
#if DEBUG
            //IsShown = true;
#endif
        }

        protected override string Name { get; } = $"{UwU.Keyboard} Input Manager";

        protected override void OnShown()
        {
            Load();
            Keyboard.KeyDown += OnKeyDown;
            Mouse.ButtonDown += OnButtonDown;
            Application.MainWindow.Closing += MainWindowClosing;
            base.OnShown();
        }

        private void MainWindowClosing(object? sender, Core.Windows.Events.CloseEventArgs e)
        {
            if (unsavedChanges)
            {
                e.Handled = true;
                if (unsavedDataDialogIsOpen)
                {
                    MessageBox.Show("(Input Manager) Unsaved changes", $"Do you want to save the changes in input map?", this, static (messageBox, state) =>
                    {
                        if (state is not InputManagerWindow inputManager)
                        {
                            return;
                        }

                        if (messageBox.Result == MessageBoxResult.Yes)
                        {
                            inputManager.Save();
                            Application.MainWindow.Close();
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            inputManager.unsavedChanges = false;
                            Application.MainWindow.Close();
                        }

                        inputManager.unsavedDataDialogIsOpen = false;
                    }, MessageBoxType.YesNoCancel);
                    unsavedDataDialogIsOpen = true;
                }
            }
        }

        protected override void OnClosed()
        {
            Application.MainWindow.Closing -= MainWindowClosing;
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

        private void Load()
        {
            if (Platform.AppConfig == null)
            {
                return;
            }

            inputMap = Platform.AppConfig.InputMap;
        }

        private void Save()
        {
            if (Platform.AppConfig == null)
            {
                return;
            }

            Platform.AppConfig.Save();
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            if (Platform.AppConfig != null && ImGui.Button("Save"))
            {
                Save();
            }

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
                if (currentAxis != null)
                {
                    var axisName = currentAxis.Name;
                    if (ImGui.InputText("##Name", ref axisName, 1024))
                    {
                        currentAxis.Name = axisName;
                    }
                }

                for (int i = 0; i < inputMap.VirtualAxes.Count; i++)
                {
                    var axis = inputMap.VirtualAxes[i];
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

            if (ImGui.Button($"{UwU.SquarePlus}##Axis")) // Add
            {
                AddNew();
            }

            if (currentAxis == null)
            {
                return;
            }

            ImGui.SameLine();

            if (ImGui.Button($"{UwU.TrashCan}##Axis")) // Add
            {
                inputMap.VirtualAxes.Remove(currentAxis);
                unsavedChanges = true;
                currentAxis = null;
                return;
            }

            ImGui.SeparatorText("Bindings");

            ImGui.BeginTable("##Table", 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("", 200f);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button($"{UwU.SquarePlus}##Binding"))
            {
                AddNewBinding();
            }

            if (currentBinding != -1)
            {
                ImGui.SameLine();

                if (ImGui.Button($"{UwU.TrashCan}##Binding"))
                {
                    currentAxis.Bindings.RemoveAt(currentBinding);
                    currentBinding = -1;
                }
            }

            if (ImGui.BeginListBox("##List", new(200, 0)))
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
                        VirtualAxisBindingType.JoystickHat => $"Joystick Hat: {binding.JoystickHatBinding.State}##{i}",
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

                changed |= ImGui.InputInt("Device Id", ref binding.DeviceId);
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
                        if (ImGui.Button($"{UwU.CircleDot}"))
                        {
                            recordKey = true;
                        }
                        break;

                    case VirtualAxisBindingType.MouseButton:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<MouseButton>.Combo("Button", ref binding.MouseButtonBinding.Button);
                        ImGui.SameLine();
                        if (ImGui.Button($"{UwU.CircleDot}"))
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
                        changed |= ImGui.InputFloat("Sensitivity", ref binding.MouseMovementBinding.Sensitivity);
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
                        changed |= ImGui.InputInt("Deadzone", ref binding.JoystickAxisBinding.Deadzone);
                        changed |= ImGui.InputFloat("Sensitivity", ref binding.JoystickAxisBinding.Sensitivity);
                        break;

                    case VirtualAxisBindingType.JoystickHat:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<JoystickHatState>.Combo("Hat", ref binding.JoystickHatBinding.State);
                        break;

                    case VirtualAxisBindingType.GamepadAxis:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<GamepadAxis>.Combo("Axis", ref binding.GamepadAxisBinding.Axis);
                        changed |= ImGui.InputInt("Deadzone", ref binding.GamepadAxisBinding.Deadzone);
                        changed |= ImGui.InputFloat("Sensitivity", ref binding.GamepadAxisBinding.Sensitivity);
                        break;

                    case VirtualAxisBindingType.GamepadTouchMovement:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.GamepadTouchMovementBinding.Axis);
                        break;

                    case VirtualAxisBindingType.GamepadSensor:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<SensorAxis>.Combo("Axis", ref binding.GamepadSensorBinding.Axis);
                        break;

                    case VirtualAxisBindingType.TouchMovement:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.TouchMovementBinding.Axis);
                        break;
                }

                if (changed)
                {
                    unsavedChanges = true;
                    currentAxis.Bindings[currentBinding] = binding;
                }
            }

            ImGui.EndGroup();

            ImGui.EndTable();
        }

        private void AxisContextMenu(string name, VirtualAxis axis)
        {
            if (ImGui.BeginPopupContextItem(name))
            {
                if (ImGui.MenuItem($"{UwU.TrashCan} Delete"))
                {
                    if (currentAxis == axis)
                    {
                        currentAxis = null;
                    }

                    inputMap.VirtualAxes.Remove(axis);
                    unsavedChanges = true;
                }
                ImGui.EndPopup();
            }
        }

        private void BindingContextMenu(string name, VirtualAxis axis, VirtualAxisBinding binding, ref int index)
        {
            if (ImGui.BeginPopupContextItem(name))
            {
                if (ImGui.MenuItem($"{UwU.TrashCan} Delete"))
                {
                    if (currentBinding == index)
                    {
                        currentBinding = -1;
                    }

                    axis.Bindings.RemoveAt(index);
                    index--;
                    unsavedChanges = true;
                }
                ImGui.EndPopup();
            }
        }

        private void AddNewBinding()
        {
            currentAxis?.Bindings.Add(new() { DeviceId = -1 });
            unsavedChanges = true;
        }

        private void AddNew()
        {
            VirtualAxis virtualAxis = new(GetNewUniqueName("New Axis"));
            inputMap.VirtualAxes.Add(virtualAxis);
            unsavedChanges = true;
        }

        private bool Exists(string name)
        {
            for (var i = 0; i < inputMap.VirtualAxes.Count; i++)
            {
                var v = inputMap.VirtualAxes[i];
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