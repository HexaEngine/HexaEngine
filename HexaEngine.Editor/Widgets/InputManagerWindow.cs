namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Editor.Extensions;
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
            if (Platform.AppConfig != null && ImGui.Button("Save"u8))
            {
                Save();
            }

            bool comboOpen;
            if (currentAxis != null)
            {
                comboOpen = ImGui.BeginCombo("##T"u8, currentAxis.Name);
            }
            else
            {
                comboOpen = ImGui.BeginCombo("##T"u8, (byte*)null);
            }

            if (comboOpen)
            {
                if (currentAxis != null)
                {
                    var axisName = currentAxis.Name;
                    if (ImGui.InputText("##Name"u8, ref axisName, 1024))
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

            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            if (ImGui.Button(builder.BuildLabelId(UwU.SquarePlus, "AxisAdd"u8)))
            {
                AddNew();
            }

            if (currentAxis == null)
            {
                return;
            }

            ImGui.SameLine();

            if (ImGui.Button(builder.BuildLabelId(UwU.TrashCan, "AxisDelete"u8)))
            {
                inputMap.VirtualAxes.Remove(currentAxis);
                unsavedChanges = true;
                currentAxis = null;
                return;
            }

            ImGui.SeparatorText("Bindings"u8);

            ImGui.BeginTable("##Table"u8, 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn(""u8, 200f);
            ImGui.TableSetupColumn(""u8, ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button(builder.BuildLabelId(UwU.SquarePlus, "BindingAdd"u8)))
            {
                AddNewBinding();
            }

            if (currentBinding != -1)
            {
                ImGui.SameLine();

                if (ImGui.Button(builder.BuildLabelId(UwU.TrashCan, "BindingDelete"u8)))
                {
                    currentAxis.Bindings.RemoveAt(currentBinding);
                    currentBinding = -1;
                }
            }

            if (ImGui.BeginListBox("##List"u8, new(200, 0)))
            {
                for (int i = 0; i < currentAxis.Bindings.Count; i++)
                {
                    var binding = currentAxis.Bindings[i];
                    byte* name = binding.Type switch
                    {
                        VirtualAxisBindingType.KeyboardKey => BuildLabel(builder, "Keyboard Key: "u8, binding.KeyboardKeyBinding.Key, i),
                        VirtualAxisBindingType.MouseButton => BuildLabel(builder, "Mouse Button: "u8, binding.MouseButtonBinding.Button, i),
                        VirtualAxisBindingType.JoystickButton => BuildLabel(builder, "Joystick Button: "u8, binding.JoystickButtonBinding.Button, i),
                        VirtualAxisBindingType.GamepadButton => BuildLabel(builder, "Gamepad Button: "u8, binding.GamepadButtonBinding.Button, i),
                        VirtualAxisBindingType.GamepadTouch => BuildLabel(builder, "Gamepad Touch: "u8, binding.DeviceId, i),
                        VirtualAxisBindingType.GamepadTouchPressure => BuildLabel(builder, "Gamepad Touch Pressure: "u8, binding.DeviceId, i),
                        VirtualAxisBindingType.Touch => BuildLabel(builder, "Touch: "u8, binding.DeviceId, i),
                        VirtualAxisBindingType.TouchPressure => BuildLabel(builder, "Touch Pressure: "u8, binding.DeviceId, i),
                        VirtualAxisBindingType.MouseWheel => BuildLabel(builder, "Mouse Wheel: "u8, binding.MouseWheelBinding.Wheel, i),
                        VirtualAxisBindingType.MouseMovement => BuildLabel(builder, "Mouse Move: "u8, binding.MouseMovementBinding.Axis, i),
                        VirtualAxisBindingType.JoystickBall => BuildLabel(builder, "Joystick Ball: "u8, binding.JoystickBallBinding.Ball, binding.JoystickBallBinding.Axis, i),
                        VirtualAxisBindingType.JoystickAxis => BuildLabel(builder, "Joystick Axis: "u8, binding.JoystickAxisBinding.Axis, i),
                        VirtualAxisBindingType.JoystickHat => BuildLabel(builder, "Joystick Hat: "u8, binding.JoystickHatBinding.State, i),
                        VirtualAxisBindingType.GamepadAxis => BuildLabel(builder, "Gamepad Axis: "u8, binding.GamepadAxisBinding.Axis, i),
                        VirtualAxisBindingType.GamepadTouchMovement => BuildLabel(builder, "Gamepad Touch Move: "u8, binding.GamepadTouchMovementBinding.Axis, i),
                        VirtualAxisBindingType.GamepadSensor => BuildLabel(builder, "Gamepad Sensor: "u8, binding.MouseMovementBinding.Axis, i),
                        VirtualAxisBindingType.TouchMovement => BuildLabel(builder, "Touch Move: "u8, binding.TouchMovementBinding.Axis, i),
                        _ => builder.BuildLabel("unknown"u8),
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

                changed |= ImGui.InputInt("Device Id"u8, ref binding.DeviceId);
                TooltipHelper.Tooltip("Set to -1 for all devices");

                changed |= ImGui.Checkbox("Invert"u8, ref binding.Invert);
                TooltipHelper.Tooltip("Invert the value eg. when key w is pressed value == -1");

                ImGui.Separator();

                switch (binding.Type)
                {
                    case VirtualAxisBindingType.KeyboardKey:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Key>.Combo("Key", ref binding.KeyboardKeyBinding.Key);
                        ImGui.SameLine();
                        if (ImGui.Button(builder.BuildLabel(UwU.CircleDot)))
                        {
                            recordKey = true;
                        }
                        break;

                    case VirtualAxisBindingType.MouseButton:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<MouseButton>.Combo("Button", ref binding.MouseButtonBinding.Button);
                        ImGui.SameLine();
                        if (ImGui.Button(builder.BuildLabel(UwU.CircleDot)))
                        {
                            recordMouseButton = true;
                        }
                        break;

                    case VirtualAxisBindingType.JoystickButton:
                        ImGui.SetNextItemWidth(100);
                        changed |= ImGui.InputInt("Button"u8, ref binding.JoystickButtonBinding.Button);
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
                        changed |= ImGui.InputFloat("Sensitivity"u8, ref binding.MouseMovementBinding.Sensitivity);
                        break;

                    case VirtualAxisBindingType.JoystickBall:
                        ImGui.SetNextItemWidth(100);
                        changed |= ImGui.InputInt("Button"u8, ref binding.JoystickBallBinding.Ball);
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<Axis>.Combo("Axis", ref binding.JoystickBallBinding.Axis);
                        break;

                    case VirtualAxisBindingType.JoystickAxis:
                        ImGui.SetNextItemWidth(100);
                        changed |= ImGui.InputInt("Axis"u8, ref binding.JoystickAxisBinding.Axis);
                        changed |= ImGui.InputInt("Deadzone"u8, ref binding.JoystickAxisBinding.Deadzone);
                        changed |= ImGui.InputFloat("Sensitivity"u8, ref binding.JoystickAxisBinding.Sensitivity);
                        break;

                    case VirtualAxisBindingType.JoystickHat:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<JoystickHatState>.Combo("Hat", ref binding.JoystickHatBinding.State);
                        break;

                    case VirtualAxisBindingType.GamepadAxis:
                        ImGui.SetNextItemWidth(100);
                        changed |= ComboEnumHelper<GamepadAxis>.Combo("Axis", ref binding.GamepadAxisBinding.Axis);
                        changed |= ImGui.InputInt("Deadzone"u8, ref binding.GamepadAxisBinding.Deadzone);
                        changed |= ImGui.InputFloat("Sensitivity"u8, ref binding.GamepadAxisBinding.Sensitivity);
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

        private static unsafe byte* BuildLabel<T>(StrBuilder builder, ReadOnlySpan<byte> preText, int value1, T value2, int id) where T : struct, Enum
        {
            builder.Reset();
            builder.Append(preText);
            builder.Append(value1);
            builder.Append(", "u8);
            builder.Append(EnumHelper<T>.GetName(value2));
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        private static unsafe byte* BuildLabel<T>(StrBuilder builder, ReadOnlySpan<byte> preText, T value, int id) where T : struct, Enum
        {
            builder.Reset();
            builder.Append(preText);
            builder.Append(EnumHelper<T>.GetName(value));
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        private static unsafe byte* BuildLabel(StrBuilder builder, ReadOnlySpan<byte> preText, int value, int id)
        {
            builder.Reset();
            builder.Append(preText);
            builder.Append(value);
            builder.Append("##"u8);
            builder.Append(id);
            builder.End();
            return builder;
        }

        private unsafe void AxisContextMenu(string name, VirtualAxis axis)
        {
            byte* buffer = stackalloc byte[256];
            StrBuilder builder = new(buffer, 256);
            if (ImGui.BeginPopupContextItem(name))
            {
                if (ImGui.MenuItem(builder.BuildLabel(UwU.TrashCan, " Delete"u8)))
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

        private unsafe void BindingContextMenu(byte* name, VirtualAxis axis, VirtualAxisBinding binding, ref int index)
        {
            byte* buffer = stackalloc byte[256];
            StrBuilder builder = new(buffer, 256);
            if (ImGui.BeginPopupContextItem(name))
            {
                if (ImGui.MenuItem(builder.BuildLabel(UwU.TrashCan, " Delete"u8)))
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