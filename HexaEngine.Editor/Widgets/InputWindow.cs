namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImPlot;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Attributes;
    using System.Numerics;

    [EditorWindowCategory("Debug")]
    public class InputWindow : EditorWindow
    {
        protected override string Name { get; } = "Input Window";

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginTabBar("E"u8))
            {
                if (ImGui.BeginTabItem("Keyboard"u8))
                {
                    DrawKeyboard();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Mouse"u8))
                {
                    DrawMouse();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Joysticks"u8))
                {
                    DrawJoysticks();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Gamepads"u8))
                {
                    DrawGamepads();
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        private static void DrawKeyboard()
        {
            var count = Keyboard.Keys.Count;
            for (int i = 0; i < count; i++)
            {
                var key = Keyboard.Keys[i];
                var name = Keyboard.KeyNames[i];
                ImGui.Text(name);
                ImGui.SameLine();
                if (Keyboard.States[key] == KeyState.Down)
                {
                    ImGui.Text("Down"u8);
                }
                else
                {
                    ImGui.Text("Up"u8);
                }
            }
        }

        private static void DrawMouse()
        {
            ImGui.Text($"Position: {Mouse.Position}");
            ImGui.Text($"Delta: {Mouse.Delta}");
            ImGui.Text($"Wheel: {Mouse.DeltaWheel}");

            ImGui.Separator();

            var count = Mouse.Buttons.Count;
            for (int i = 0; i < count; i++)
            {
                var button = Mouse.Buttons[i];
                var name = Mouse.ButtonNames[i];
                ImGui.Text(name);
                ImGui.SameLine();
                if (Mouse.States[button] == MouseButtonState.Down)
                {
                    ImGui.Text("Down"u8);
                }
                else
                {
                    ImGui.Text("Up"u8);
                }
            }
        }

        private static void DrawJoysticks()
        {
            var joystickCount = Joysticks.Sticks.Count;
            if (ImGui.BeginTabBar("E"u8))
            {
                for (int i = 0; i < joystickCount; i++)
                {
                    var joystick = Joysticks.Sticks[i];
                    if (ImGui.BeginTabItem($"{joystick.Id}: {joystick.Name}"))
                    {
                        DrawJoystick(joystick);
                        ImGui.EndTabItem();
                    }
                }
            }
            ImGui.EndTabBar();
        }

        private static unsafe void DrawJoystick(Joystick joystick)
        {
            ImGui.Text($"Id: {joystick.Id}");
            ImGui.Text($"Name: {joystick.Name}");
            ImGui.Text($"IsAttached: {joystick.IsAttached}");

            var index = joystick.PlayerIndex;
            if (ImGui.InputInt("PlayerIndex"u8, ref index))
            {
                joystick.PlayerIndex = index;
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Rumble"u8))
            {
                var lofreq = lowFreq;
                if (ImGui.InputScalar("Low Freq (Right)"u8, ImGuiDataType.U16, &lofreq))
                {
                    lowFreq = lofreq;
                }

                var hifreq = highFreq;
                if (ImGui.InputScalar("High Freq (Left)"u8, ImGuiDataType.U16, &hifreq))
                {
                    highFreq = hifreq;
                }

                var duraMs = durationMs;
                if (ImGui.InputScalar("Duration (ms)"u8, ImGuiDataType.U32, &duraMs))
                {
                    durationMs = duraMs;
                }

                if (ImGui.Button("Rumble##BUt"u8))
                {
                    joystick.Rumble(lowFreq, highFreq, durationMs);
                }

                if (ImGui.Button("Rumble Triggers"u8))
                {
                    joystick.RumbleTriggers(lowFreq, highFreq, durationMs);
                }
            }
            ImGui.Separator();
            if (ImGui.CollapsingHeader("LED"u8))
            {
                ImGui.Text($"HasLED: {joystick.HasLED}");
                if (joystick.HasLED)
                {
                    if (ImGui.ColorEdit4("LED##ColEdit"u8, ref color.X))
                    {
                        joystick.SetLED(color);
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Axes"u8))
            {
                var dead = joystick.Deadzone;
                if (ImGui.InputScalar("Deadzone"u8, ImGuiDataType.S16, &dead))
                {
                    joystick.Deadzone = dead;
                }

                ImGui.Separator();

                var axesCount = joystick.Axes.Count;
                for (int i = 0; i < axesCount; i++)
                {
                    var value = joystick.Axes[i];
                    ImGui.Text($"{i}: {value}");
                }

                if (joystick.Axes.TryGetValue(0, out var leftX) && joystick.Axes.TryGetValue(1, out var leftY))
                {
                    ImPlot.SetNextAxesLimits(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
                    if (ImPlot.BeginPlot("Analog"u8, new Vector2(512, 512), ImPlotFlags.NoInputs))
                    {
                        ImPlot.PlotScatter("Pos"u8, &leftX, &leftY, 1);
                        ImPlot.EndPlot();
                    }
                }

                if (joystick.Axes.TryGetValue(2, out var throttle))
                {
                    ImGui.SameLine();
                    ImPlot.SetNextAxesLimits(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
                    if (ImPlot.BeginPlot("Throttle"u8, new Vector2(128, 512), ImPlotFlags.NoInputs))
                    {
                        short x = 0;
                        ImPlot.PlotScatter("Pos"u8, &x, &throttle, 1);
                        ImPlot.EndPlot();
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Balls"u8))
            {
                var ballsCount = joystick.Balls.Count;
                for (int i = 0; i < ballsCount; i++)
                {
                    var value = joystick.Balls[i];
                    ImGui.Text($"{i}: {value}");
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Buttons"u8))
            {
                var buttonsCount = joystick.Buttons.Count;
                for (int i = 0; i < buttonsCount; i++)
                {
                    var value = joystick.Buttons[i];
                    ImGui.Text($"{i}: {value}");
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Hats"u8))
            {
                var hatsCount = joystick.Hats.Count;
                for (int i = 0; i < hatsCount; i++)
                {
                    var value = joystick.Hats[i];
                    ImGui.Text($"{i}: {value}");
                }
            }
            ImGui.Separator();

            if (ImGui.CollapsingHeader("Vendor info"u8))
            {
                ImGui.Text($"Vendor: {joystick.Vendor}");
                ImGui.Text($"Product: {joystick.Product}");
                ImGui.Text($"ProductVersion: {joystick.ProductVersion}");
                ImGui.Text($"Serial: {joystick.Serial}");
                ImGui.Text($"Type: {joystick.Type}");
            }
        }

        private static void DrawGamepads()
        {
            var gamepadCount = Gamepads.Controllers.Count;
            if (ImGui.BeginTabBar("E"u8))
            {
                for (int i = 0; i < gamepadCount; i++)
                {
                    var gamepad = Gamepads.Controllers[i];
                    if (ImGui.BeginTabItem($"{gamepad.Id}: {gamepad.Name}"))
                    {
                        DrawGamepad(gamepad);
                        ImGui.EndTabItem();
                    }
                }
            }
            ImGui.EndTabBar();
        }

        private static ushort lowFreq;
        private static ushort highFreq;
        private static uint durationMs;
        private static Vector4 color;

        private static unsafe void DrawGamepad(Gamepad gamepad)
        {
            ImGui.Text($"Id: {gamepad.Id}");
            ImGui.Text($"Name: {gamepad.Name}");
            ImGui.Text($"IsAttached: {gamepad.IsAttached}");

            var index = gamepad.PlayerIndex;
            if (ImGui.InputInt("PlayerIndex"u8, ref index))
            {
                gamepad.PlayerIndex = index;
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Rumble"u8))
            {
                var lofreq = lowFreq;
                if (ImGui.InputScalar("Low Freq (Right)"u8, ImGuiDataType.U16, &lofreq))
                {
                    lowFreq = lofreq;
                }

                var hifreq = highFreq;
                if (ImGui.InputScalar("High Freq (Left)"u8, ImGuiDataType.U16, &hifreq))
                {
                    highFreq = hifreq;
                }

                var duraMs = durationMs;
                if (ImGui.InputScalar("Duration (ms)"u8, ImGuiDataType.U32, &duraMs))
                {
                    durationMs = duraMs;
                }

                if (ImGui.Button("Rumble##BUt"u8))
                {
                    gamepad.Rumble(lowFreq, highFreq, durationMs);
                }

                if (ImGui.Button("Rumble Triggers"u8))
                {
                    gamepad.RumbleTriggers(lowFreq, highFreq, durationMs);
                }
            }
            ImGui.Separator();
            if (ImGui.CollapsingHeader("LED"u8))
            {
                ImGui.Text($"HasLED: {gamepad.HasLED}");
                if (gamepad.HasLED)
                {
                    if (ImGui.ColorEdit4("LED##ColEdit"u8, ref color.X))
                    {
                        gamepad.SetLED(color);
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Axes"u8))
            {
                var dead = gamepad.Deadzone;
                if (ImGui.InputScalar("Deadzone"u8, ImGuiDataType.S16, &dead))
                {
                    gamepad.Deadzone = dead;
                }

                ImGui.Separator();

                var axesCount = Gamepad.Axes.Count;
                for (int i = 0; i < axesCount; i++)
                {
                    var axis = Gamepad.Axes[i];
                    if (gamepad.AxisStates.TryGetValue(axis, out var value))
                    {
                        ImGui.Text($"{Gamepad.AxisNames[i]}: {value}");
                    }
                }
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.LeftX, out var leftX) && gamepad.AxisStates.TryGetValue(GamepadAxis.LeftY, out var leftY))
                {
                    ImPlot.SetNextAxesLimits(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
                    if (ImPlot.BeginPlot("Left Analog"u8, new Vector2(256, 256), ImPlotFlags.NoInputs))
                    {
                        ImPlot.PlotScatter("Pos"u8, &leftX, &leftY, 1);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.SameLine();
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.RightX, out var rightX) && gamepad.AxisStates.TryGetValue(GamepadAxis.RightY, out var rightY))
                {
                    ImPlot.SetNextAxesLimits(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
                    if (ImPlot.BeginPlot("Right Analog"u8, new Vector2(256, 256), ImPlotFlags.NoInputs))
                    {
                        ImPlot.PlotScatter("Pos"u8, &rightX, &rightY, 1);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.SameLine();
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.LeftTrigger, out var leftTrigger))
                {
                    ImPlot.SetNextAxesLimits(-1, 1, 0, short.MaxValue);
                    if (ImPlot.BeginPlot("Left Trigger"u8, new Vector2(128, 256), ImPlotFlags.NoInputs))
                    {
                        short x = 0;
                        ImPlot.PlotScatter("Pos"u8, &x, &leftTrigger, 1);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.SameLine();
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.RightTrigger, out var rightTrigger))
                {
                    ImPlot.SetNextAxesLimits(-1, 1, 0, short.MaxValue);
                    if (ImPlot.BeginPlot("Right Trigger"u8, new Vector2(128, 256), ImPlotFlags.NoInputs))
                    {
                        short x = 0;
                        ImPlot.PlotScatter("Pos"u8, &x, &rightTrigger, 1);
                        ImPlot.EndPlot();
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Buttons"u8))
            {
                var buttonsCount = Gamepad.Buttons.Count;
                for (int i = 0; i < buttonsCount; i++)
                {
                    var button = Gamepad.Buttons[i];
                    if (gamepad.ButtonStates.TryGetValue(button, out var value))
                    {
                        ImGui.Text($"{Gamepad.ButtonNames[i]}: {value}");
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Touchpads"u8))
            {
                var touchpadCount = gamepad.Touchpads.Count;
                for (int i = 0; i < touchpadCount; i++)
                {
                    var touchpad = gamepad.Touchpads[i];
                    ImGui.Text($"Id: {touchpad.Id}");

                    ImGui.Separator();

                    ImPlot.SetNextAxesLimits(0f, 1f, 0f, 1f);
                    if (ImPlot.BeginPlot($"Touchpad {i}", new Vector2(256, 256), ImPlotFlags.NoInputs))
                    {
                        for (int j = 0; j < touchpad.FingerCount; j++)
                        {
                            var touchpadFinger = touchpad.GetFinger(j);
                            touchpadFinger.Y = 1 - touchpadFinger.Y;
                            if (touchpadFinger.State == FingerState.Down)
                            {
                                ImPlot.PlotScatter($"Finger {j}", ref touchpadFinger.X, ref touchpadFinger.Y, 1);
                            }
                        }
                        ImPlot.EndPlot();
                    }

                    ImGui.Separator();

                    for (int j = 0; j < touchpad.FingerCount; j++)
                    {
                        var touchpadFinger = touchpad.GetFinger(j);
                        ImGui.Text($"Finger: {j}");
                        ImGui.Text($"State: {touchpadFinger.State}");
                        ImGui.Text($"X: {touchpadFinger.X}");
                        ImGui.Text($"Y: {touchpadFinger.Y}");
                        ImGui.Text($"Pressure: {touchpadFinger.Pressure}");
                        if (j < touchpad.FingerCount - 1)
                        {
                            ImGui.Separator();
                        }
                    }

                    if (i < touchpadCount - 1)
                    {
                        ImGui.Separator();
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Sensors"u8))
            {
                var sensorCount = Gamepad.SensorTypes.Count;
                for (int i = 0; i < sensorCount; i++)
                {
                    var sensorType = Gamepad.SensorTypes[i];
                    if (gamepad.Sensors.TryGetValue(sensorType, out var sensor))
                    {
                        var enable = sensor.Enabled;
                        if (ImGui.Checkbox($"Enabled##{i}", ref enable))
                        {
                            sensor.Enabled = enable;
                        }

                        ImGui.Text($"{sensorType}: {sensor.Vector}");

                        ImGui.Separator();
                        ImPlot.SetNextAxesLimits(-10, 10, -10, 10);
                        if (ImPlot.BeginPlot($"Sensor {sensorType}", new Vector2(256, 256), ImPlotFlags.NoInputs))
                        {
                            Vector2 vector = new(sensor.Vector.X, sensor.Vector.Z);
                            ImPlot.PlotScatter($"{sensorType}", ref vector.X, ref vector.Y, 1);

                            ImPlot.EndPlot();
                        }

                        if (i < sensorCount - 1)
                        {
                            ImGui.Separator();
                        }
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Haptic"u8))
            {
                var haptic = gamepad.Haptic;
                if (haptic != null)
                {
                    ImGui.Text($"Id: {haptic.Id}");
                    ImGui.Text($"Name: {haptic.Name}");
                    ImGui.Text($"AxesCount: {haptic.AxesCount}");
                    ImGui.Text($"RumbleSupported: {haptic.RumbleSupported}");
                    ImGui.Text($"EffectsSupported: {haptic.EffectsSupported}");
                    ImGui.Text($"EffectsCount: {haptic.EffectsCount}");
                    ImGui.Text($"EffectsPlayingCount: {haptic.EffectsPlayingCount}");
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Mappings"u8))
            {
                ImGui.Text($"Mapping: {gamepad.Mapping}");
                ImGui.Separator();
                var mappingCount = gamepad.Mappings.Count;
                for (int i = 0; i < mappingCount; i++)
                {
                    ImGui.Text($"{i}: {gamepad.Mappings[i]}");
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Vendor info"u8))
            {
                ImGui.Text($"Vendor: {gamepad.Vendor}");
                ImGui.Text($"Product: {gamepad.Product}");
                ImGui.Text($"ProductVersion: {gamepad.ProductVersion}");
                ImGui.Text($"Serial: {gamepad.Serial}");
                ImGui.Text($"Type: {gamepad.Type}");
            }
        }
    }
}