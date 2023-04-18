namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using ImGuiNET;
    using ImPlotNET;
    using System;
    using System.Numerics;

    public class InputWindow : ImGuiWindow
    {
        protected override string Name => "Input Window";

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.BeginTabBar("E"))
            {
                if (ImGui.BeginTabItem("Keyboard"))
                {
                    DrawKeyboard();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Mouse"))
                {
                    DrawMouse();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Joysticks"))
                {
                    DrawJoysticks();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Gamepads"))
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
                    ImGui.Text("Down");
                }
                else
                {
                    ImGui.Text("Up");
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
                    ImGui.Text("Down");
                }
                else
                {
                    ImGui.Text("Up");
                }
            }
        }

        private static void DrawJoysticks()
        {
            var joystickCount = Joysticks.Sticks.Count;
            if (ImGui.BeginTabBar("E"))
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
            if (ImGui.InputInt("PlayerIndex", ref index))
                joystick.PlayerIndex = index;

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Rumble"))
            {
                var lofreq = lowFreq;
                if (ImGui.InputScalar("Low Freq (Right)", ImGuiDataType.U16, (nint)(&lofreq)))
                    lowFreq = lofreq;

                var hifreq = highFreq;
                if (ImGui.InputScalar("High Freq (Left)", ImGuiDataType.U16, (nint)(&hifreq)))
                    highFreq = hifreq;

                var duraMs = durationMs;
                if (ImGui.InputScalar("Duration (ms)", ImGuiDataType.U32, (nint)(&duraMs)))
                    durationMs = duraMs;

                if (ImGui.Button("Rumble##BUt"))
                {
                    joystick.Rumble(lowFreq, highFreq, durationMs);
                }

                if (ImGui.Button("Rumble Triggers"))
                {
                    joystick.RumbleTriggers(lowFreq, highFreq, durationMs);
                }
            }
            ImGui.Separator();
            if (ImGui.CollapsingHeader("LED"))
            {
                ImGui.Text($"HasLED: {joystick.HasLED}");
                if (joystick.HasLED)
                {
                    if (ImGui.ColorEdit4("LED##ColEdit", ref color))
                    {
                        joystick.SetLED(color);
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Axes"))
            {
                var dead = joystick.Deadzone;
                if (ImGui.InputScalar("Deadzone", ImGuiDataType.S16, (nint)(&dead)))
                    joystick.Deadzone = dead;

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
                    if (ImPlot.BeginPlot("Analog", new Vector2(512, 512), ImPlotFlags.NoInputs))
                    {
                        ImPlot.PlotScatter("Pos", ref leftX, ref leftY, 1);
                        ImPlot.EndPlot();
                    }
                }

                if (joystick.Axes.TryGetValue(2, out var throttle))
                {
                    ImGui.SameLine();
                    ImPlot.SetNextAxesLimits(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
                    if (ImPlot.BeginPlot("Throttle", new Vector2(128, 512), ImPlotFlags.NoInputs))
                    {
                        short x = 0;
                        ImPlot.PlotScatter("Pos", ref x, ref throttle, 1);
                        ImPlot.EndPlot();
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Balls"))
            {
                var ballsCount = joystick.Balls.Count;
                for (int i = 0; i < ballsCount; i++)
                {
                    var value = joystick.Balls[i];
                    ImGui.Text($"{i}: {value}");
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Buttons"))
            {
                var buttonsCount = joystick.Buttons.Count;
                for (int i = 0; i < buttonsCount; i++)
                {
                    var value = joystick.Buttons[i];
                    ImGui.Text($"{i}: {value}");
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Hats"))
            {
                var hatsCount = joystick.Hats.Count;
                for (int i = 0; i < hatsCount; i++)
                {
                    var value = joystick.Hats[i];
                    ImGui.Text($"{i}: {value}");
                }
            }
            ImGui.Separator();

            if (ImGui.CollapsingHeader("Vendor info"))
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
            if (ImGui.BeginTabBar("E"))
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
            if (ImGui.InputInt("PlayerIndex", ref index))
                gamepad.PlayerIndex = index;

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Rumble"))
            {
                var lofreq = lowFreq;
                if (ImGui.InputScalar("Low Freq (Right)", ImGuiDataType.U16, (nint)(&lofreq)))
                    lowFreq = lofreq;

                var hifreq = highFreq;
                if (ImGui.InputScalar("High Freq (Left)", ImGuiDataType.U16, (nint)(&hifreq)))
                    highFreq = hifreq;

                var duraMs = durationMs;
                if (ImGui.InputScalar("Duration (ms)", ImGuiDataType.U32, (nint)(&duraMs)))
                    durationMs = duraMs;

                if (ImGui.Button("Rumble##BUt"))
                {
                    gamepad.Rumble(lowFreq, highFreq, durationMs);
                }

                if (ImGui.Button("Rumble Triggers"))
                {
                    gamepad.RumbleTriggers(lowFreq, highFreq, durationMs);
                }
            }
            ImGui.Separator();
            if (ImGui.CollapsingHeader("LED"))
            {
                ImGui.Text($"HasLED: {gamepad.HasLED}");
                if (gamepad.HasLED)
                {
                    if (ImGui.ColorEdit4("LED##ColEdit", ref color))
                    {
                        gamepad.SetLED(color);
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Axes"))
            {
                var dead = gamepad.Deadzone;
                if (ImGui.InputScalar("Deadzone", ImGuiDataType.S16, (nint)(&dead)))
                    gamepad.Deadzone = dead;

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
                    if (ImPlot.BeginPlot("Left Analog", new Vector2(256, 256), ImPlotFlags.NoInputs))
                    {
                        ImPlot.PlotScatter("Pos", ref leftX, ref leftY, 1);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.SameLine();
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.RightX, out var rightX) && gamepad.AxisStates.TryGetValue(GamepadAxis.RightY, out var rightY))
                {
                    ImPlot.SetNextAxesLimits(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
                    if (ImPlot.BeginPlot("Right Analog", new Vector2(256, 256), ImPlotFlags.NoInputs))
                    {
                        ImPlot.PlotScatter("Pos", ref rightX, ref rightY, 1);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.SameLine();
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.TriggerLeft, out var leftTrigger))
                {
                    ImPlot.SetNextAxesLimits(-1, 1, 0, short.MaxValue);
                    if (ImPlot.BeginPlot("Left Trigger", new Vector2(128, 256), ImPlotFlags.NoInputs))
                    {
                        short x = 0;
                        ImPlot.PlotScatter("Pos", ref x, ref leftTrigger, 1);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.SameLine();
                if (gamepad.AxisStates.TryGetValue(GamepadAxis.TriggerRight, out var rightTrigger))
                {
                    ImPlot.SetNextAxesLimits(-1, 1, 0, short.MaxValue);
                    if (ImPlot.BeginPlot("Right Trigger", new Vector2(128, 256), ImPlotFlags.NoInputs))
                    {
                        short x = 0;
                        ImPlot.PlotScatter("Pos", ref x, ref rightTrigger, 1);
                        ImPlot.EndPlot();
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Buttons"))
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

            if (ImGui.CollapsingHeader("Touchpads"))
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
                                ImPlot.PlotScatter($"Finger {j}", ref touchpadFinger.X, ref touchpadFinger.Y, 1);
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
                            ImGui.Separator();
                    }

                    if (i < touchpadCount - 1)
                        ImGui.Separator();
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Sensors"))
            {
                var sensorCount = Gamepad.SensorTypes.Count;
                for (int i = 0; i < sensorCount; i++)
                {
                    var sensorType = Gamepad.SensorTypes[i];
                    if (gamepad.Sensors.TryGetValue(sensorType, out var sensor))
                    {
                        var enable = sensor.Enabled;
                        if (ImGui.Checkbox($"Enabled##{i}", ref enable))
                            sensor.Enabled = enable;

                        ImGui.Text($"{sensorType}: {sensor.Vector}");

                        ImGui.Separator();
                        ImPlot.SetNextAxesLimits(-10, 10, -10, 10);
                        if (ImPlot.BeginPlot($"Sensor {sensorType}", new Vector2(256, 256), ImPlotFlags.NoInputs))
                        {
                            Vector2 vector = new Vector2(sensor.Vector.X, sensor.Vector.Z);
                            ImPlot.PlotScatter($"{sensorType}", ref vector.X, ref vector.Y, 1);

                            ImPlot.EndPlot();
                        }

                        if (i < sensorCount - 1)
                            ImGui.Separator();
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Haptic"))
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

            if (ImGui.CollapsingHeader("Mappings"))
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

            if (ImGui.CollapsingHeader("Vendor info"))
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