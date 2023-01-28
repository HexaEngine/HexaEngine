namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Effects;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Nodes;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;

    public class EffectWindow : ImGuiWindow
    {
        private List<NamedEffect> descriptions = new();
        private List<NamedTexture> textures = new();
        private List<NamedShaderResource> resources = new();
        private List<NamedRenderTarget> targets = new();
        private List<NamedSampler> samplers = new();
        private List<NamedBuffer> buffers = new();
        private NodeEditor editor = new();
        private Func<bool>? createCallback;

        private static readonly string[] formatNames = Enum.GetNames<Format>();
        private static readonly Format[] formats = Enum.GetValues<Format>();

        private static readonly string[] shaderStageNames = Enum.GetNames<ShaderStage>();
        private static readonly ShaderStage[] shaderStages = Enum.GetValues<ShaderStage>();

        private static readonly string[] shaderResourceViewDimensionNames = Enum.GetNames<ShaderResourceViewDimension>();
        private static readonly ShaderResourceViewDimension[] shaderResourceViewDimensions = Enum.GetValues<ShaderResourceViewDimension>();

        private static readonly string[] renderTargetViewDimensionNames = Enum.GetNames<RenderTargetViewDimension>();
        private static readonly RenderTargetViewDimension[] renderTargetViewDimensions = Enum.GetValues<RenderTargetViewDimension>();

        private static readonly string[] filterNames = Enum.GetNames<Filter>();
        private static readonly Filter[] filters = Enum.GetValues<Filter>();

        private static readonly string[] textureAddressModesNames = Enum.GetNames<TextureAddressMode>();
        private static readonly TextureAddressMode[] textureAddressModes = Enum.GetValues<TextureAddressMode>();

        private static readonly string[] comparisonFunctionNames = Enum.GetNames<ComparisonFunction>();
        private static readonly ComparisonFunction[] comparisonFunctions = Enum.GetValues<ComparisonFunction>();

        private struct NamedEffect
        {
            public string Name = string.Empty;
            public EffectDescription Description = new();

            public NamedEffect()
            {
            }
        }

        private struct NamedTexture
        {
            public string Name = string.Empty;
            public TextureDescription Description;
            public bool HasSRV;
            public bool HasRTV;

            public NamedTexture()
            {
            }
        }

        private struct NamedShaderResource
        {
            public string Name = string.Empty;
            public ShaderResourceViewDescription Description;

            public NamedShaderResource()
            {
            }
        }

        private struct NamedRenderTarget
        {
            public string Name = string.Empty;
            public RenderTargetViewDescription Description;

            public NamedRenderTarget()
            {
            }
        }

        private struct NamedSampler
        {
            public string Name = string.Empty;
            public SamplerDescription Description = SamplerDescription.PointClamp;

            public NamedSampler()
            {
            }
        }

        private struct NamedBuffer
        {
            public string Name = string.Empty;
            public BufferDescription Description;

            public NamedBuffer()
            {
            }
        }

        public EffectWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;
            var swap = editor.CreateNode("SwapChain", false);
            swap.CreatePin("Input", PinKind.Input, PinType.RenderTargetView, ImNodesNET.PinShape.QuadFilled);
            var gbuffers = editor.CreateNode("GBuffer", false);
            gbuffers.CreatePin("Color", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Position", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Normal", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Tangent", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Misc0", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Misc1", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Misc2", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
            gbuffers.CreatePin("Misc3", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
        }

        protected override string Name => "Effects";

        public override void DrawWindow(IGraphicsContext context)
        {
            base.DrawWindow(context);
        }

        public unsafe void DisplayMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Add Effect"))
                {
                    newEffect = new();
                    createCallback = CreateEffect;
                    ImGui.OpenPopup("Create", ImGuiPopupFlags.AnyPopupId | ImGuiPopupFlags.AnyPopupLevel | ImGuiPopupFlags.AnyPopup);
                }
                if (ImGui.MenuItem("Add Texture"))
                {
                    newTexture = new();
                    createCallback = CreateTexture;
                    ImGui.OpenPopup("Create", ImGuiPopupFlags.AnyPopupId | ImGuiPopupFlags.AnyPopupLevel | ImGuiPopupFlags.AnyPopup);
                }
                if (ImGui.MenuItem("Add Sampler"))
                {
                    newSampler = new();
                    createCallback = CreateSampler;
                    ImGui.OpenPopup("Create", ImGuiPopupFlags.AnyPopupId | ImGuiPopupFlags.AnyPopupLevel | ImGuiPopupFlags.AnyPopup);
                }

                if (ImGui.BeginPopupModal("Create", null, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.Modal))
                {
                    if (createCallback?.Invoke() ?? false)
                        ImGui.CloseCurrentPopup();

                    ImGui.EndPopup();
                }

                ImGui.EndMenuBar();
            }
        }

        private NamedTexture newTexture;

        public bool CreateTexture()
        {
            ImGui.InputText("Name", ref newTexture.Name, 256);
            ImGui.InputInt("Width", ref newTexture.Description.Width);
            ImGui.InputInt("Height", ref newTexture.Description.Height);

            int index = Array.IndexOf(formats, newTexture.Description.Format);
            if (ImGui.Combo("Format", ref index, formatNames, formatNames.Length))
            {
                newTexture.Description.Format = formats[index];
            }

            ImGui.Checkbox("Has SRV", ref newTexture.HasRTV);
            ImGui.Checkbox("Has RTV", ref newTexture.HasSRV);

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Ok"))
            {
                var node = editor.CreateNode(newTexture.Name);
                if (newTexture.HasRTV)
                {
                    node.CreatePin("RTV", PinKind.Input, PinType.RenderTargetView, ImNodesNET.PinShape.QuadFilled);
                }
                if (newTexture.HasSRV)
                {
                    node.CreatePin("SRV", PinKind.Output, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
                }
                return true;
            }
            return false;
        }

        private NamedEffect newEffect;

        public bool CreateEffect()
        {
            ImGui.InputText("Name", ref newEffect.Name, 256);

            if (ImGui.Button("+##CBV"))
            {
                newEffect.Description.EffectConstants.Add(new() { Name = string.Empty });
            }
            if (ImGui.BeginListBox("CBVs"))
            {
                var bindings = newEffect.Description.EffectConstants;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (i != 0)
                        ImGui.Separator();
                    bool changed = false;
                    EffectConstantBuffer binding = bindings[i];
                    if (ImGui.InputText("Name##" + i, ref binding.Name, 1024))
                    {
                        changed = true;
                    }

                    if (ImGui.InputInt("Slot##" + i, ref binding.Slot))
                    {
                        changed = true;
                    }

                    var stageIndex = Array.IndexOf(shaderStages, binding.Stage);
                    if (ImGui.Combo("Stage##" + i, ref stageIndex, shaderStageNames, shaderStageNames.Length))
                    {
                        binding.Stage = shaderStages[stageIndex];
                        changed = true;
                    }

                    if (changed)
                        bindings[i] = binding;
                }
                ImGui.EndListBox();
            }

            if (ImGui.Button("+##SRV"))
            {
                newEffect.Description.EffectResources.Add(new() { Name = string.Empty });
            }
            if (ImGui.BeginListBox("SRVs"))
            {
                var bindings = newEffect.Description.EffectResources;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (i != 0)
                        ImGui.Separator();
                    bool changed = false;
                    EffectResourceDescription binding = bindings[i];
                    if (ImGui.InputText("Name##" + i, ref binding.Name, 1024))
                    {
                        changed = true;
                    }

                    if (ImGui.InputInt("Slot##" + i, ref binding.Slot))
                    {
                        changed = true;
                    }

                    var stageIndex = Array.IndexOf(shaderStages, binding.Stage);
                    if (ImGui.Combo("Stage##" + i, ref stageIndex, shaderStageNames, shaderStageNames.Length))
                    {
                        binding.Stage = shaderStages[stageIndex];
                        changed = true;
                    }

                    var dimIndex = Array.IndexOf(shaderResourceViewDimensions, binding.Dimension);
                    if (ImGui.Combo("Dimension##" + i, ref dimIndex, shaderResourceViewDimensionNames, shaderResourceViewDimensionNames.Length))
                    {
                        binding.Dimension = shaderResourceViewDimensions[dimIndex];
                        changed = true;
                    }

                    if (changed)
                        bindings[i] = binding;
                }
                ImGui.EndListBox();
            }

            if (ImGui.Button("+##Samplers"))
            {
                newEffect.Description.EffectSamplers.Add(new() { Name = string.Empty });
            }
            if (ImGui.BeginListBox("Samplers"))
            {
                var bindings = newEffect.Description.EffectSamplers;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (i != 0)
                        ImGui.Separator();
                    bool changed = false;
                    var binding = bindings[i];
                    if (ImGui.InputText("Name##" + i, ref binding.Name, 1024))
                    {
                        changed = true;
                    }

                    if (ImGui.InputInt("Slot##" + i, ref binding.Slot))
                    {
                        changed = true;
                    }

                    var stageIndex = Array.IndexOf(shaderStages, binding.Stage);
                    if (ImGui.Combo("Stage##" + i, ref stageIndex, shaderStageNames, shaderStageNames.Length))
                    {
                        binding.Stage = shaderStages[stageIndex];
                        changed = true;
                    }

                    if (changed)
                        bindings[i] = binding;
                }
                ImGui.EndListBox();
            }

            if (ImGui.Button("+##Targets"))
            {
                newEffect.Description.EffectTargets.Add(new() { Name = string.Empty });
            }
            if (ImGui.BeginListBox("Targets"))
            {
                var bindings = newEffect.Description.EffectTargets;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (i != 0)
                        ImGui.Separator();
                    bool changed = false;
                    var binding = bindings[i];
                    if (ImGui.InputText("Name##" + i, ref binding.Name, 1024))
                    {
                        changed = true;
                    }

                    if (ImGui.InputInt("Slot##" + i, ref binding.Slot))
                    {
                        changed = true;
                    }

                    var dimIndex = Array.IndexOf(renderTargetViewDimensions, binding.Dimension);
                    if (ImGui.Combo("Dimension##" + i, ref dimIndex, renderTargetViewDimensionNames, renderTargetViewDimensionNames.Length))
                    {
                        binding.Dimension = renderTargetViewDimensions[dimIndex];
                        changed = true;
                    }

                    if (changed)
                        bindings[i] = binding;
                }
                ImGui.EndListBox();
            }

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Ok"))
            {
                var node = editor.CreateNode(newEffect.Name);
                var desc = newEffect.Description;
                for (int i = 0; i < desc.EffectConstants.Count; i++)
                {
                    var constant = desc.EffectConstants[i];
                    node.CreatePin(constant.Name, PinKind.Input, PinType.ConstantBuffer, ImNodesNET.PinShape.CircleFilled);
                }
                for (int i = 0; i < desc.EffectResources.Count; i++)
                {
                    var resource = desc.EffectResources[i];
                    node.CreatePin(resource.Name, PinKind.Input, PinType.ShaderResourceView, ImNodesNET.PinShape.QuadFilled);
                }
                for (int i = 0; i < desc.EffectSamplers.Count; i++)
                {
                    var sampler = desc.EffectSamplers[i];
                    node.CreatePin(sampler.Name, PinKind.Input, PinType.Sampler, ImNodesNET.PinShape.TriangleFilled);
                }
                for (int i = 0; i < desc.EffectTargets.Count; i++)
                {
                    var target = desc.EffectTargets[i];
                    node.CreatePin(target.Name, PinKind.Output, PinType.RenderTargetView, ImNodesNET.PinShape.QuadFilled);
                }

                return true;
            }
            return false;
        }

        private NamedSampler newSampler;

        public bool CreateSampler()
        {
            ImGui.InputText("Name", ref newSampler.Name, 256);

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Ok"))
            {
                editor.AddNode(new SamplerNode(editor, newSampler.Name, true, false));

                return true;
            }
            return false;
        }

        public override void DrawContent(IGraphicsContext context)
        {
            DisplayMenuBar();
            editor.Draw();
        }
    }
}