namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Physics;
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;
    using HexaEngine.Volumes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Text;

    // TODO: Needs major overhaul.
    /// <summary>
    ///
    /// </summary>
    public class HierarchyWidget : EditorWindow
    {
        private readonly Dictionary<string, EditorGameObjectAttribute> cache = new();
        private bool showHidden;
        private bool focused;

        private readonly uint[] levelColorPalette =
        [// 0xAABBGGRR
            0x8F0000FF,
            0x8F00FF00,
            0x8FFF0000,
            0x8FFFFF00,
            0x8FFF00FF,
            0x8F00FFFF,
            0x8F800080,
            0x8F008080,
        ];

        private enum HierarchyLevelColoring
        {
            Mono,
            Color,
            Multi
        }

        private HierarchyLevelColoring coloring = HierarchyLevelColoring.Color;
        private uint levelColor = 0xffcf7334;
        private uint prefabLevelColor = 0x8526D65F;
        private uint sceneLevelColor = 0xffcf7334;
        private byte levelAlpha = 0xFF;
        private byte monochromeBrightness = 0xac;
        private bool reverseColoring = false;

        private UnsafeList<char> labelBuffer = new();
        private UnsafeList<byte> labelOutBuffer = new();
        private readonly List<bool> isLastInLevel = [];
        private string searchString = string.Empty;
        private bool windowHovered;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public HierarchyWidget()
        {
            IsShown = true;

            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x
                    .GetTypes()
                    .AsParallel()
                    .Where(x =>
                    x.IsAssignableTo(typeof(GameObject)) &&
                    x.GetCustomAttribute<EditorGameObjectAttribute>() != null &&
                    !x.IsAbstract)))
            {
                var attr = type.GetCustomAttribute<EditorGameObjectAttribute>();
                if (attr == null)
                {
                    continue;
                }

                cache.Add(attr.Name, attr);
            }
            cache.Add("Object", new EditorGameObjectAttribute("Object", typeof(GameObject), () => new GameObject(), x => x is not null));
            cache = cache.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        protected override string Name { get; } = $"{UwU.Sitemap} Hierarchy";

        public override void DrawContent(IGraphicsContext context)
        {
            focused = ImGui.IsWindowFocused();
            var scene = SceneManager.Current;

            if (scene == null)
            {
                return;
            }

            ImGui.InputTextWithHint("##SearchBar", $"{UwU.MagnifyingGlass} Search...", ref searchString, 1024);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xff1c1c1c);

            ImGui.BeginChild("LayoutContent");

            windowHovered = ImGui.IsWindowHovered();

            DisplayContextMenu();

            Vector2 avail = ImGui.GetContentRegionAvail();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (scene.IsPrefabScene)
            {
                ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, 0x8526D65F); // RGBA ABGR
                levelColor = prefabLevelColor;
            }
            else
            {
                levelColor = sceneLevelColor;
            }

            if (ImGui.BeginTable("Table", 1, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.PreciseWidths))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

                ImGui.PushStyleColor(ImGuiCol.TableRowBg, 0xff1c1c1c);
                ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, 0xff232323);

                ImGuiTablePtr table = ImGuiP.GetCurrentTable();

                ImGui.Indent();
                ImGui.TableHeadersRow();
                ImGui.Unindent();

                if (ImGui.BeginDragDropTarget())
                {
                    unsafe
                    {
                        var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                        if (!payload.IsNull)
                        {
                            Guid id = *(Guid*)payload.Data;
                            var child = scene.FindByGuid(id);
                            if (child != null)
                            {
                                scene.AddChild(child);
                            }
                        }
                    }
                    ImGui.EndDragDropTarget();
                }
                if (scene.IsPrefabScene)
                {
                    DisplayNode(scene.Root, true, false, drawList, table, avail, 0, true);
                }
                else
                {
                    for (int i = 0; i < scene.Root.Children.Count; i++)
                    {
                        var element = scene.Root.Children[i];
                        bool isLast = i == scene.Root.Children.Count - 1;
                        DisplayNode(element, false, !element.Name.Contains(searchString), drawList, table, avail, 0, isLast);
                    }
                }

                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.EndTable();
            }

            if (scene.IsPrefabScene)
            {
                ImGui.PopStyleColor();
            }

            var space = ImGui.GetContentRegionAvail();
            ImGui.Dummy(space);

            ImGui.EndChild();

            ImGui.PopStyleColor();
        }

        private void DrawSettingsMenu()
        {
            if (ImGui.BeginMenu($"{UwU.Gear} Settings"))
            {
                ImGui.Checkbox("Show Hidden", ref showHidden);

                if (ImGui.RadioButton("Monochrome", coloring == HierarchyLevelColoring.Mono))
                {
                    coloring = HierarchyLevelColoring.Mono;
                }

                if (ImGui.RadioButton("Gradient Color", coloring == HierarchyLevelColoring.Color))
                {
                    coloring = HierarchyLevelColoring.Color;
                }

                if (ImGui.RadioButton("Multi Color", coloring == HierarchyLevelColoring.Multi))
                {
                    coloring = HierarchyLevelColoring.Multi;
                }

                if (ImGui.BeginMenu("Colors"))
                {
                    Vector4 color = Color.FromABGR(levelColor).ToVector4();
                    if (ImGui.ColorEdit4("Base Color", ref color))
                    {
                        levelColor = new Color(color).ToUIntABGR();
                    }
                    ImGui.EndMenu();
                }

                ImGui.Checkbox("Reverse Coloring", ref reverseColoring);

                ImGui.EndMenu();
            }
        }

        private void DisplayContextMenu()
        {
            var scene = SceneManager.Current;
            if (ImGui.BeginPopupContextWindow(ImGuiPopupFlags.NoOpenOverItems | ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverExistingPopup))
            {
                if (ImGui.BeginMenu($"{UwU.SquarePlus} Add"))
                {
                    foreach (var item in cache)
                    {
                        if (ImGui.MenuItem(item.Key))
                        {
                            var node = item.Value.Constructor();
                            var name = scene!.GetAvailableName(item.Key);
                            node.Name = name;
                            scene.AddChild(node);
                            scene.UnsavedChanged = true;
                        }
                    }

                    ImGui.EndMenu();
                }

                DrawSettingsMenu();

                ImGui.EndPopup();
            }
        }

        private static void DisplayNodeContextMenu(GameObject element)
        {
            if (ImGui.BeginPopupContextItem(element.FullName, ImGuiPopupFlags.MouseButtonRight))
            {
                if (ImGui.MenuItem($"{UwU.MagnifyingGlassPlus} Focus"))
                {
                    EditorCameraController.Center = element.Transform.GlobalPosition;
                }
                if (ImGui.MenuItem($"{UwU.MagnifyingGlassMinus} Defocus"))
                {
                    EditorCameraController.Center = Vector3.Zero;
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Minus} Unselect"))
                {
                    SelectionCollection.Global.ClearSelection();
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Clone} Clone"))
                {
                    for (int i = 0; i < SelectionCollection.Global.Count; i++)
                    {
                        var item = SelectionCollection.Global[i];
                        if (item is GameObject gameObject)
                        {
                            try
                            {
                                var instance = Instantiator.Instantiate(gameObject) ?? throw new Exception("Failed to Clone object.");
                                element.GetScene().AddChild(instance);
                            }
                            catch (Exception ex)
                            {
                                LoggerFactory.General.LogAndShowError("Failed to Clone object.", ex);
                            }
                        }
                    }
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Trash} Delete"))
                {
                    SelectionCollection.Global.PurgeSelection();
                }

                ImGui.EndPopup();
            }
        }

        private uint GetColorForLevel(int level)
        {
            int levelNormalized = (level - 1) % levelColorPalette.Length;

            if (reverseColoring)
            {
                levelNormalized = levelColorPalette.Length - levelNormalized - 1;
            }

            if (coloring == HierarchyLevelColoring.Mono)
            {
                uint brightness = (uint)(monochromeBrightness * (1 - (levelNormalized / (float)levelColorPalette.Length)));

                return (uint)levelAlpha << 24 | brightness << 16 | brightness << 8 | brightness; // 0xAABBGGRR
            }

            if (coloring == HierarchyLevelColoring.Color)
            {
                float value = levelNormalized / (float)levelColorPalette.Length;
                float hueShift = value * 0.1f;

                ColorHSVA hsv = Color.FromABGR(levelColor).ToHSVA();

                hsv.H += hueShift;
                hsv.S *= 1 - value;
                hsv.V /= MathF.Exp(value);

                uint color = hsv.ToRGBA().ToUIntABGR();

                return (uint)levelAlpha << 24 | color; // 0xAABBGGRR
            }

            // HierarchyLevelColoring.Multi just return here as fallback.

            return levelColorPalette[levelNormalized];
        }

        private void DisplayNode(GameObject element, bool isRoot, bool searchHidden, ImDrawListPtr drawList, ImGuiTablePtr table, Vector2 avail, int level, bool isLast)
        {
            SetLevel(level, isLast);

            if (element.IsHidden && !showHidden)
            {
                return;
            }

            if (element.IsHidden || searchHidden)
            {
                ImGui.BeginDisabled(true);
            }

            uint colHovered = ImGui.GetColorU32(ImGuiCol.HeaderHovered);
            uint colSelected = ImGui.GetColorU32(ImGuiCol.Header);

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImRect rect = ImGuiP.TableGetCellBgRect(table, 0);
            rect.Max.X = avail.X + rect.Min.X;
            rect.Max.Y += ImGui.GetTextLineHeight();

            bool hovered = ImGui.IsMouseHoveringRect(rect.Min, rect.Max) && windowHovered;
            if (hovered)
            {
                drawList.AddRectFilled(rect.Min, rect.Max, colHovered);
            }

            if (element.Children.Count == 0)
            {
                flags |= ImGuiTreeNodeFlags.Leaf;
            }

            if (isRoot)
            {
                flags = ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.DefaultOpen;
            }

            if (level > 0)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    var before = coloring;
                    coloring = HierarchyLevelColoring.Mono;
                    DrawTreeLine(drawList, rect, level, isLast);
                    coloring = before;
                }
                else
                {
                    DrawTreeLine(drawList, rect, level, isLast);
                }
            }

            if (element.IsEditorSelected)
            {
                drawList.AddRectFilled(rect.Min, rect.Max, colSelected);

                var lineMin = rect.Min;
                var lineMax = new Vector2(lineMin.X + 4, rect.Max.Y);
                drawList.AddRectFilled(lineMin, lineMax, levelColor);
            }

            bool colorText = !searchHidden && !string.IsNullOrEmpty(searchString);

            if (colorText)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, 0xff0099ff);
            }

            uint col = 0x0;
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, col);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, col);

            char icon = GetIcon(element);

            bool isOpen = ImGui.TreeNodeEx($"{icon} {element.Name}", flags);
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            if (colorText)
            {
                ImGui.PopStyleColor();
            }

            element.IsEditorOpen = isOpen;
            element.IsEditorDisplayed = true;

            HandleInput(element, hovered);
            DisplayNodeContextMenu(element);
            HandleDragDrop(element);
            DrawObjectLabels(avail, element);

            if (searchHidden && !element.IsHidden)
            {
                ImGui.EndDisabled();
            }

            if (isOpen)
            {
                for (int i = 0; i < element.Children.Count; i++)
                {
                    var child = element.Children[i];
                    bool isLastElement = i == element.Children.Count - 1;
                    DisplayNode(child, false, !child.Name.Contains(searchString), drawList, table, avail, level + 1, isLastElement);
                }
                ImGui.TreePop();
            }
            else
            {
                for (int i = 0; i < element.Children.Count; i++)
                {
                    element.Children[i].IsEditorDisplayed = false;
                }
            }

            if (element.IsHidden)
            {
                ImGui.EndDisabled();
            }
        }

        private static void HandleDragDrop(GameObject element)
        {
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                    if (!payload.IsNull)
                    {
                        //Guid id = *(Guid*)payload.Data;
                        //var gameObject = SceneManager.Current.FindByGuid(id);
                        SelectionCollection.Global.MoveSelection(element);
                        SceneManager.Current!.UnsavedChanged = true;
                    }
                }
                ImGui.EndDragDropTarget();
            }

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    Guid id = element.Guid;
                    ImGui.SetDragDropPayload(nameof(GameObject), &id, (uint)sizeof(Guid));
                }
                ImGui.Text(element.Name);
                ImGui.EndDragDropSource();
            }
        }

        private void HandleInput(GameObject element, bool hovered)
        {
            if (hovered && ImGuiP.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (ImGui.GetIO().KeyCtrl)
                {
                    SelectionCollection.Global.AddSelection(element);
                }
                else if (ImGui.GetIO().KeyShift)
                {
                    var last = SelectionCollection.Global.Last<GameObject>();
                    if (last != null)
                    {
                        SelectionCollection.Global.AddMultipleSelection(SceneManager.Current!.GetRange(last, element));
                    }
                }
                else if (!element.IsEditorSelected)
                {
                    SelectionCollection.Global.AddOverwriteSelection(element);
                }
            }
            if (hovered && ImGuiP.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup(element.FullName);
                if (!element.IsEditorSelected && !ImGui.GetIO().KeyCtrl)
                {
                    SelectionCollection.Global.AddSelection(element);
                }
            }
            if (focused)
            {
                if (element.IsEditorSelected && ImGuiP.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGuiP.IsKeyReleased(ImGuiKey.F) && EditorCameraController.Center != element.Transform.GlobalPosition)
                {
                    EditorCameraController.Center = element.Transform.GlobalPosition;
                }
                else if (element.IsEditorSelected && ImGuiP.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGuiP.IsKeyReleased(ImGuiKey.F) && EditorCameraController.Center == element.Transform.GlobalPosition)
                {
                    EditorCameraController.Center = Vector3.Zero;
                }
                if (element.IsEditorSelected && ImGuiP.IsKeyReleased(ImGuiKey.Delete))
                {
                    SceneManager.Current!.UnsavedChanged = true;
                    SelectionCollection.Global.PurgeSelection();
                }
                if (element.IsEditorSelected && ImGuiP.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGuiP.IsKeyReleased(ImGuiKey.U))
                {
                    SelectionCollection.Global.ClearSelection();
                }
            }
        }

        private static char GetIcon(GameObject element)
        {
            char icon = UwU.Cube;
            if (element is Light)
            {
                icon = UwU.Lightbulb;
            }

            if (element is Camera)
            {
                icon = UwU.Camera;
            }

            if (element is Volume)
            {
                icon = UwU.WandMagicSparkles;
            }

            return icon;
        }

        private static unsafe void ToUTF8(ref UnsafeList<char> str, ref UnsafeList<byte> pOutStr)
        {
            int byteSize = Encoding.UTF8.GetByteCount(str.Data, (int)str.Size);
            pOutStr.Reserve((int)byteSize + 1);
            Encoding.UTF8.GetBytes(str.Data, (int)str.Size, pOutStr.Data, byteSize);
            pOutStr.Resize((int)byteSize);
            pOutStr[pOutStr.Size] = (byte)'\0';
        }

        private unsafe void DrawObjectLabels(Vector2 avail, GameObject element)
        {
            labelBuffer.Clear();
            labelOutBuffer.Clear();

            if (element.HasComponent<ScriptComponent>())
            {
                labelBuffer.PushBack(UwU.Scroll);
            }

            if (element.HasComponent<IDrawable>())
            {
                labelBuffer.PushBack(UwU.DrawPolygon);
            }

            if (element.HasComponent<IActorComponent>())
            {
                labelBuffer.PushBack(UwU.CircleDot);
            }

            if (element.HasComponent<IColliderComponent>())
            {
                labelBuffer.PushBack(UwU.Shapes);
            }

            if (element.HasComponent<IJointComponent>())
            {
                labelBuffer.PushBack(UwU.Link);
            }

            ToUTF8(ref labelBuffer, ref labelOutBuffer);

            float padding = ImGui.CalcTextSize($"{UwU.Eye}").X + 7;

            float width = ImGui.CalcTextSize(labelOutBuffer.Data).X + padding;
            ImGui.SameLine();
            ImGui.SetCursorPosX(avail.X - width);
            ImGui.Text(labelOutBuffer.Data);

            ImGui.SameLine();
            bool visible = element.IsEditorVisible;
            if (visible && ImGui.MenuItem($"{UwU.Eye}"))
            {
                element.IsEditorVisible = false;
            }
            if (!visible && ImGui.MenuItem($"{UwU.EyeSlash}"))
            {
                element.IsEditorVisible = true;
            }
        }

        private void SetLevel(int level, bool isLast)
        {
            if (isLastInLevel.Count <= level)
            {
                isLastInLevel.Add(isLast);
            }
            else
            {
                isLastInLevel[level] = isLast;
            }
        }

        private void DrawTreeLine(ImDrawListPtr drawList, ImRect rect, int level, bool isLast, bool isLevelLower = false)
        {
            for (int i = 1; i < level; i++)
            {
                var lowerLevel = level - i;
                if (isLastInLevel[lowerLevel])
                {
                    continue;
                }
                DrawTreeLine(drawList, rect, lowerLevel, false, true);
            }

            const float lineThickness = 2;
            const float lineWidth = 10;
            float indentSpacing = ImGui.GetStyle().IndentSpacing * (level - 1) + ImGui.GetTreeNodeToLabelSpacing() * 0.5f - lineThickness * 0.5f;
            Vector2 lineMin = new(rect.Min.X + indentSpacing, rect.Min.Y);
            Vector2 lineMax = new(lineMin.X + lineThickness, rect.Max.Y);
            Vector2 lineMidpoint = lineMin + (lineMax - lineMin) * 0.5f;
            Vector2 lineTMin = new(lineMax.X, lineMidpoint.Y - lineThickness * 0.5f);
            Vector2 lineTMax = new(lineMax.X + lineWidth, lineMidpoint.Y + lineThickness * 0.5f);
            if (isLast)
            {
                lineMax.Y = lineTMax.Y; // set vertical line y to horizontal line y to create a L shape
            }
            uint color = GetColorForLevel(level);
            drawList.AddRectFilled(lineMin, lineMax, color);
            if (!isLevelLower)
            {
                drawList.AddRectFilled(lineTMin, lineTMax, color);
            }
        }
    }
}