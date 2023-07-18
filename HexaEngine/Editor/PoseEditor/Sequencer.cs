namespace HexaEngine.Editor.PoseEditor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Animations;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class Sequencer : EditorWindow
    {
        protected override string Name => "Sequencer";

        public int Frame { get; set; } = 0;

        public Animation? Animation { get; set; }

        private long scroll = 0;
        private int zoom = 1;

        public Sequencer()
        {
            Animation = new("Test", 10, 10);
        }

        public float TimeToFrame(float time)
        {
            float framerateInv = 1 / (float)Animation.TicksPerSecond;
            return time / framerateInv;
        }

        public float FrameToTime(float frame)
        {
            float framerateInv = 1 / (float)Animation.TicksPerSecond;
            return frame * framerateInv;
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (Animation == null)
                return;
            ImGui.PushItemWidth(80);

            var frame = Frame;
            ImGui.InputInt("Frame", ref frame);
            Frame = (int)Math.Clamp(frame, 0, Animation.Duration * Animation.TicksPerSecond);

            ImGui.SameLine();

            var framerate = Animation.TicksPerSecond;
            ImGui.InputDouble("Framerate", ref framerate);
            Animation.TicksPerSecond = framerate;

            ImGui.SameLine();

            var duration = Animation.Duration;
            ImGui.InputDouble("Duration", ref duration);
            Animation.Duration = duration;

            ImGui.PopItemWidth();

            var cursor = ImGui.GetCursorPos();
            var avail = ImGui.GetContentRegionAvail();
            var size = new Vector2(200, 0);
            var sizeChannels = avail - size;
            sizeChannels.Y = 90;

            for (int i = 0; i < Animation.NodeChannels.Count; i++)
            {
                var channel = Animation.NodeChannels[i];
                DrawChannel(ref channel, sizeChannels);
                Animation.NodeChannels[i] = channel;
                if (i < Animation.NodeChannels.Count - 1)
                    ImGui.Spacing();
            }
        }

        public void DrawChannel(ref NodeChannel channel, Vector2 maxSize)
        {
            int frameCount = (int)(Animation.Duration * Animation.TicksPerSecond);
            ImGui.InputText("Node:", ref channel.NodeName, 256);
            var cursor = ImGui.GetCursorPos();
            ImGui.BeginChild($"CH{channel.NodeName}", maxSize);
            bool isPopupOpen = false;
            if (ImGui.BeginPopupContextWindow())
            {
                isPopupOpen = true;
                if (ImGui.MenuItem("Insert Keyframes"))
                {
                    channel.PositionKeyframes.Add(new() { Time = FrameToTime(Frame), Value = default });
                    channel.RotationKeyframes.Add(new() { Time = FrameToTime(Frame), Value = default });
                    channel.ScaleKeyframes.Add(new() { Time = FrameToTime(Frame), Value = Vector3.One });
                }
                if (ImGui.BeginMenu("Keyframes"))
                {
                    if (ImGui.MenuItem("Position Keyframe"))
                    {
                        channel.PositionKeyframes.Add(new() { Time = FrameToTime(Frame), Value = default });
                    }
                    if (ImGui.MenuItem("Rotation Keyframe"))
                    {
                        channel.RotationKeyframes.Add(new() { Time = FrameToTime(Frame), Value = default });
                    }
                    if (ImGui.MenuItem("Scale Keyframe"))
                    {
                        channel.ScaleKeyframes.Add(new() { Time = FrameToTime(Frame), Value = Vector3.One });
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndPopup();
            }

            var style = ImGui.GetStyle();
            var frameCol = ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.FrameBg]);
            var textCol = ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.Text]);
            var itemCol = ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.TextDisabled]);

            var winPos = ImGui.GetWindowPos();
            var pos = winPos + ImGui.GetCursorPos();
            var size = ImGui.GetContentRegionAvail();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(pos, pos + new Vector2(60, 70), frameCol);
            ImGui.Text("Position");
            ImGui.Spacing();
            ImGui.Text("Rotation");
            ImGui.Spacing();
            ImGui.Text("Scale");
            pos += new Vector2(60, 0);
            size -= new Vector2(60, 0);

            size.Y = 90;

            drawList.AddRectFilled(pos, pos + size, frameCol);

            int itemSpacing = 30;
            int itemHeight = 70;

            var padd = style.FramePadding;
            float maxWidth = (float)frameCount / zoom * itemSpacing + itemSpacing * 2;
            var min = pos.X;
            min -= padd.X;
            min += scroll;
            min -= itemSpacing;
            min /= itemSpacing;
            min--;

            for (int i = (int)min; i < frameCount / zoom; i++)
            {
                var p1 = pos;
                p1.X += i * itemSpacing - scroll + itemSpacing;
                p1 += padd;
                if (pos.X > p1.X)
                    continue;
                if ((pos + size).X < p1.X)
                    break;
                var p2 = p1;
                p2.Y += itemHeight - padd.Y * 2;
                drawList.AddLine(p1, p2, itemCol);
            }

            var posCol = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1));
            var rotCol = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1, 0, 1));
            var sclCol = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 1, 1));
            int keyframeHeight = 25;
            for (int i = 0; i < channel.PositionKeyframes.Count; i++)
            {
                var keyframe = channel.PositionKeyframes[i];
                var time = (int)TimeToFrame((float)keyframe.Time) / zoom;
                var p1 = pos + padd;
                p1.X += time * itemSpacing - scroll + itemSpacing;
                p1.Y += 5;
                if (i != 0)
                {
                    var keyframe1 = channel.PositionKeyframes[i - 1];
                    var time1 = (int)TimeToFrame((float)keyframe1.Time) / zoom;
                    var p2 = pos + padd;
                    p2.X += time1 * itemSpacing - scroll + itemSpacing;
                    p2.Y += 5;
                    drawList.AddLine(Vector2.Clamp(p2, pos, pos + size), Vector2.Clamp(p1, pos, pos + size), posCol);
                }
                if (pos.X > p1.X)
                    continue;
                if ((pos + size).X < p1.X)
                    break;
                drawList.AddCircleFilled(p1, 5, posCol);
            }

            for (int i = 0; i < channel.RotationKeyframes.Count; i++)
            {
                var keyframe = channel.RotationKeyframes[i];
                var time = (int)TimeToFrame((float)keyframe.Time) / zoom;
                var p1 = pos + padd;
                p1.X += time * itemSpacing - scroll + itemSpacing;
                p1.Y += 5 + keyframeHeight;
                if (i != 0)
                {
                    var keyframe1 = channel.RotationKeyframes[i - 1];
                    var time1 = (int)TimeToFrame((float)keyframe1.Time) / zoom;
                    var p2 = pos + padd;
                    p2.X += time1 * itemSpacing - scroll + itemSpacing;
                    p2.Y += 5 + keyframeHeight;
                    drawList.AddLine(Vector2.Clamp(p2, pos, pos + size), Vector2.Clamp(p1, pos, pos + size), rotCol);
                }
                if (pos.X > p1.X)
                    continue;
                if ((pos + size).X < p1.X)
                    break;
                drawList.AddCircleFilled(p1, 5, rotCol);
            }

            for (int i = 0; i < channel.ScaleKeyframes.Count; i++)
            {
                var keyframe = channel.ScaleKeyframes[i];
                var time = (int)TimeToFrame((float)keyframe.Time) / zoom;
                var p1 = pos + padd;
                p1.X += time * itemSpacing - scroll + itemSpacing;
                p1.Y += 5 + keyframeHeight * 2;
                if (i != 0)
                {
                    var keyframe1 = channel.ScaleKeyframes[i - 1];
                    var time1 = (int)TimeToFrame((float)keyframe1.Time) / zoom;
                    var p2 = pos + padd;
                    p2.X += time1 * itemSpacing - scroll + itemSpacing;
                    p2.Y += 5 + keyframeHeight * 2;
                    drawList.AddLine(Vector2.Clamp(p2, pos, pos + size), Vector2.Clamp(p1, pos, pos + size), sclCol);
                }
                if (pos.X > p1.X)
                    continue;
                if ((pos + size).X < p1.X)
                    break;
                drawList.AddCircleFilled(p1, 5, sclCol);
            }

            if (ImGui.IsMouseHoveringRect(pos, pos + new Vector2(size.X, 70)) && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !isPopupOpen)
            {
                var mousePos = ImGui.GetMousePos();
                mousePos -= pos;
                var frame = (int)(((mousePos.X + scroll) / itemSpacing - 1) * zoom);

                Frame = Math.Clamp(frame, 0, frameCount);
            }

            {
                var p1 = pos + padd;
                p1.X += (float)Frame / zoom * itemSpacing - scroll + itemSpacing;
                if (pos.X <= p1.X)
                {
                    var p2 = p1;
                    p2.Y += itemHeight - padd.Y * 2;
                    drawList.AddLine(p1, p2, textCol);
                }
            }

            Vector2 scrollPos = pos;
            scrollPos.Y += size.Y - style.ScrollbarSize;
            Vector2 scrollSize = new(size.X, style.ScrollbarSize);
            var scrollRect = new ImRect() { Min = scrollPos, Max = scrollPos + scrollSize };
            ImGui.ScrollbarEx(scrollRect, 10, ImGuiAxis.X, ref scroll, (long)size.X, (long)maxWidth, ImDrawFlags.None);

            ImGui.EndChild();

            ImGui.SetCursorPos(cursor + new Vector2(maxSize.X, 0));

            ImGui.BeginChild($"CH1{channel.NodeName}", new Vector2(0, maxSize.Y));

            for (int i = 0; i < channel.PositionKeyframes.Count; i++)
            {
                var keyframe = channel.PositionKeyframes[i];
                var time = (int)TimeToFrame((float)keyframe.Time);
                if (time == Frame)
                {
                    if (ImGui.InputFloat3("Position", ref keyframe.Value))
                    {
                        channel.PositionKeyframes[i] = keyframe;
                    }
                }
            }
            for (int i = 0; i < channel.RotationKeyframes.Count; i++)
            {
                var keyframe = channel.RotationKeyframes[i];
                var time = (int)TimeToFrame((float)keyframe.Time);
                if (time == Frame)
                {
                    var rot = keyframe.Value.GetYawPitchRoll().ToDeg();
                    if (ImGui.InputFloat3("Rotation", ref rot))
                    {
                        keyframe.Value = rot.ToRad().GetQuaternion();
                        channel.RotationKeyframes[i] = keyframe;
                    }
                }
            }
            for (int i = 0; i < channel.ScaleKeyframes.Count; i++)
            {
                var keyframe = channel.ScaleKeyframes[i];
                var time = (int)TimeToFrame((float)keyframe.Time);
                if (time == Frame)
                {
                    if (ImGui.InputFloat3("Scale", ref keyframe.Value))
                    {
                        channel.ScaleKeyframes[i] = keyframe;
                    }
                }
            }
            ImGui.EndChild();
        }
    }
}