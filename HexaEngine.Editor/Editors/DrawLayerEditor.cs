namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Graphics;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;
    using System.Reflection;

    public class DrawLayerPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly Type propType;

        public DrawLayerPropertyEditor(string name, PropertyInfo property)
        {
            Name = name;
            Property = property;
            guiName = new(name);
            propType = property.PropertyType;
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            if (instance is not GameObject gameObject)
            {
                return false;
            }

            if (value is not DrawLayerCollection drawLayers)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text(guiName.Name);

            bool changed = false;
            var scene = gameObject.GetScene();
            var sceneDrawLayers = scene.DrawLayerManager.DrawLayers;
            int x = 0;
            const int maxItemsOnRow = 4;

            ImGui.BeginGroup();
            for (int i = 0; i < sceneDrawLayers.Count; i++, x++)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0));
                DrawLayer drawLayer = sceneDrawLayers[i];
                bool enabled = drawLayers.Contains(drawLayer);
                if (x > 0 && x < maxItemsOnRow)
                {
                    ImGui.SameLine();
                }
                else if (x >= maxItemsOnRow)
                {
                    x = 0;
                }
                if (ImGui.Checkbox(drawLayer.DisplayName.Id, ref enabled))
                {
                    if (enabled)
                    {
                        drawLayers.Add(drawLayer);
                    }
                    else
                    {
                        drawLayers.Remove(drawLayer);
                    }
                    changed = true;
                }
                ImGui.PopStyleVar();
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Rename"))
                    {
                    }

                    if (ImGui.MenuItem("Delete"))
                    {
                        MessageBox.Show("Warning", "Are you sure you want to delete the draw layer this action cannot be undone!", null, (mb, user) =>
                        {
                            scene.DrawLayerManager.RemoveLayer(drawLayer);
                        }, MessageBoxType.YesNo);
                    }

                    ImGui.EndPopup();
                }

                TooltipHelper.Tooltip(drawLayer.DisplayName);
            }
            ImGui.EndGroup();

            if (ImGui.SmallButton("+"))
            {
                CreateDrawLayerPopup popup = new(scene.DrawLayerManager);
                PopupManager.Show(popup);
            }

            return changed;
        }

        public class RenameDrawLayerPopup : Modal
        {
            private readonly DrawLayer drawLayer;
            private string newName;

            public RenameDrawLayerPopup(DrawLayer drawLayer)
            {
                this.drawLayer = drawLayer;
                newName = drawLayer.Name;
            }

            public override string Name { get; } = "Rename Draw Layer";

            protected override ImGuiWindowFlags Flags { get; }

            public override void Reset()
            {
            }

            protected override void DrawContent()
            {
                ImGui.InputText("##0", ref newName, 1024);
                if (ImGui.Button("Cancel"))
                {
                    Close();
                }
                ImGui.SameLine();
                if (ImGui.Button("Ok"))
                {
                    drawLayer.Name = newName;
                    Close();
                }
            }
        }

        public class CreateDrawLayerPopup : Modal
        {
            private readonly DrawLayerManager manager;
            private string drawLayerName = "New Draw Layer";

            public CreateDrawLayerPopup(DrawLayerManager manager)
            {
                this.manager = manager;
            }

            public override string Name { get; } = "New Draw Layer";

            protected override ImGuiWindowFlags Flags { get; }

            public override void Reset()
            {
                drawLayerName = "New Draw Layer";
            }

            protected override void DrawContent()
            {
                ImGui.InputText("##0", ref drawLayerName, 1024);
                if (ImGui.Button("Cancel"))
                {
                    Close();
                }
                ImGui.SameLine();
                if (ImGui.Button("Ok"))
                {
                    manager.AddLayer(drawLayerName);
                    Close();
                }
            }
        }
    }
}