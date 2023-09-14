namespace VkTesting.Graphics
{
    using HexaEngine.Shaderc;
    using Silk.NET.Vulkan;
    using VkTesting;
    using static VulkanGraphicsDevice;

    public unsafe class VulkanGraphicsPipeline : IDisposable
    {
        private readonly VulkanGraphicsDevice device;
        private readonly VulkanRenderPass renderPass;
        private GraphicsPipelineDesc desc;
        private Pipeline pipeline;
        private PipelineLayout layout;
        private ShaderModule vertexShader;
        private ShaderModule tessControlShader;
        private ShaderModule tessEvaluationShader;
        private ShaderModule geometryShader;
        private ShaderModule fragmentShader;
        private bool disposedValue;

        public VulkanGraphicsPipeline(VulkanGraphicsDevice device, VulkanRenderPass renderPass, GraphicsPipelineDesc desc)
        {
            this.device = device;
            this.renderPass = renderPass;
            this.desc = desc;
            Compile();
        }

        public void Bind(VulkanCommandList commandList)
        {
            vk.CmdBindPipeline(commandList.CommandBuffer, PipelineBindPoint.Graphics, pipeline);
        }

        private void Compile()
        {
            uint count = 0;
            PipelineShaderStageCreateInfo* stages = stackalloc PipelineShaderStageCreateInfo[5];
            DynamicState* dynamicStates = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
            if (desc.VertexShader != null)
            {
                layout = CreateLayout();

                var bytecode = ShaderCompiler.CompileHLSLFile(desc.VertexShader, desc.VertexShaderEntrypoint, ShadercShaderKind.VertexShader);
                vertexShader = CreateShaderModule(bytecode);
                bytecode.Release();

                PipelineShaderStageCreateInfo shaderStageInfo = default;
                shaderStageInfo.SType = StructureType.PipelineShaderStageCreateInfo;
                shaderStageInfo.Stage = ShaderStageFlags.VertexBit;
                shaderStageInfo.Module = vertexShader;
                shaderStageInfo.PName = desc.VertexShaderEntrypoint.ToUTF8();
                stages[count] = shaderStageInfo;
                count++;
            }
            if (desc.TessControlShader != null)
            {
                var bytecode = ShaderCompiler.CompileHLSLFile(desc.TessControlShader, desc.TessControlShaderEntrypoint, ShadercShaderKind.TessControlShader);
                tessControlShader = CreateShaderModule(bytecode);
                bytecode.Release();
                PipelineShaderStageCreateInfo shaderStageInfo = default;
                shaderStageInfo.SType = StructureType.PipelineShaderStageCreateInfo;
                shaderStageInfo.Stage = ShaderStageFlags.TessellationControlBit;
                shaderStageInfo.Module = tessControlShader;
                shaderStageInfo.PName = desc.TessControlShaderEntrypoint.ToUTF8();
                stages[count] = shaderStageInfo;
                count++;
            }
            if (desc.TessEvaluationShader != null)
            {
                var bytecode = ShaderCompiler.CompileHLSLFile(desc.TessEvaluationShader, desc.TessEvaluationShaderEntrypoint, ShadercShaderKind.TessEvaluationShader);
                tessEvaluationShader = CreateShaderModule(bytecode);
                bytecode.Release();
                PipelineShaderStageCreateInfo shaderStageInfo = default;
                shaderStageInfo.SType = StructureType.PipelineShaderStageCreateInfo;
                shaderStageInfo.Stage = ShaderStageFlags.TessellationEvaluationBit;
                shaderStageInfo.Module = tessEvaluationShader;
                shaderStageInfo.PName = desc.TessEvaluationShaderEntrypoint.ToUTF8();
                stages[count] = shaderStageInfo;
                count++;
            }
            if (desc.GeometryShader != null)
            {
                var bytecode = ShaderCompiler.CompileHLSLFile(desc.GeometryShader, desc.GeometryShaderEntrypoint, ShadercShaderKind.GeometryShader);
                geometryShader = CreateShaderModule(bytecode);
                bytecode.Release();
                PipelineShaderStageCreateInfo shaderStageInfo = default;
                shaderStageInfo.SType = StructureType.PipelineShaderStageCreateInfo;
                shaderStageInfo.Stage = ShaderStageFlags.GeometryBit;
                shaderStageInfo.Module = geometryShader;
                shaderStageInfo.PName = desc.GeometryShaderEntrypoint.ToUTF8();
                stages[count] = shaderStageInfo;
                count++;
            }
            if (desc.FragmentShader != null)
            {
                var bytecode = ShaderCompiler.CompileHLSLFile(desc.FragmentShader, desc.FragmentShaderEntrypoint, ShadercShaderKind.FragmentShader);
                fragmentShader = CreateShaderModule(bytecode);
                bytecode.Release();
                PipelineShaderStageCreateInfo shaderStageInfo = default;
                shaderStageInfo.SType = StructureType.PipelineShaderStageCreateInfo;
                shaderStageInfo.Stage = ShaderStageFlags.FragmentBit;
                shaderStageInfo.Module = fragmentShader;
                shaderStageInfo.PName = desc.FragmentShaderEntrypoint.ToUTF8();
                stages[count] = shaderStageInfo;
                count++;
            }

            PipelineDynamicStateCreateInfo dynamicState = default;
            dynamicState.SType = StructureType.PipelineDynamicStateCreateInfo;
            dynamicState.DynamicStateCount = 2;
            dynamicState.PDynamicStates = dynamicStates;

            PipelineVertexInputStateCreateInfo vertexInputInfo = default;
            vertexInputInfo.SType = StructureType.PipelineVertexInputStateCreateInfo;
            vertexInputInfo.VertexBindingDescriptionCount = 0;
            vertexInputInfo.PVertexBindingDescriptions = null; // Optional
            vertexInputInfo.VertexAttributeDescriptionCount = 0;
            vertexInputInfo.PVertexAttributeDescriptions = null; // Optional

            PipelineInputAssemblyStateCreateInfo inputAssembly = default;
            inputAssembly.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
            inputAssembly.Topology = PrimitiveTopology.TriangleList;
            inputAssembly.PrimitiveRestartEnable = false;

            PipelineViewportStateCreateInfo viewportState = default;
            viewportState.SType = StructureType.PipelineViewportStateCreateInfo;
            viewportState.ViewportCount = 1;
            viewportState.ScissorCount = 1;

            PipelineRasterizationStateCreateInfo rasterizer = default;
            rasterizer.SType = StructureType.PipelineRasterizationStateCreateInfo;
            rasterizer.DepthClampEnable = false;
            rasterizer.RasterizerDiscardEnable = false;
            rasterizer.PolygonMode = PolygonMode.Fill;
            rasterizer.LineWidth = 1.0f;
            rasterizer.CullMode = CullModeFlags.BackBit;
            rasterizer.FrontFace = FrontFace.Clockwise;
            rasterizer.DepthBiasEnable = false;
            rasterizer.DepthBiasConstantFactor = 0.0f; // Optional
            rasterizer.DepthBiasClamp = 0.0f; // Optional
            rasterizer.DepthBiasSlopeFactor = 0.0f; // Optional

            PipelineMultisampleStateCreateInfo multisampling = default;
            multisampling.SType = StructureType.PipelineMultisampleStateCreateInfo;
            multisampling.SampleShadingEnable = false;
            multisampling.RasterizationSamples = SampleCountFlags.Count1Bit;
            multisampling.MinSampleShading = 1.0f; // Optional
            multisampling.PSampleMask = null; // Optional
            multisampling.AlphaToCoverageEnable = false; // Optional
            multisampling.AlphaToOneEnable = false; // Optional

            PipelineColorBlendAttachmentState colorBlendAttachment = default;
            colorBlendAttachment.ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
            colorBlendAttachment.BlendEnable = false;
            colorBlendAttachment.SrcColorBlendFactor = BlendFactor.One; // Optional
            colorBlendAttachment.DstColorBlendFactor = BlendFactor.Zero; // Optional
            colorBlendAttachment.ColorBlendOp = BlendOp.Add; // Optional
            colorBlendAttachment.SrcAlphaBlendFactor = BlendFactor.One; // Optional
            colorBlendAttachment.DstAlphaBlendFactor = BlendFactor.Zero; // Optional
            colorBlendAttachment.AlphaBlendOp = BlendOp.Add; // Optional

            PipelineColorBlendStateCreateInfo colorBlending = default;
            colorBlending.SType = StructureType.PipelineColorBlendStateCreateInfo;
            colorBlending.LogicOpEnable = false;
            colorBlending.LogicOp = LogicOp.Copy; // Optional
            colorBlending.AttachmentCount = 1;
            colorBlending.PAttachments = &colorBlendAttachment;
            colorBlending.BlendConstants[0] = 0.0f; // Optional
            colorBlending.BlendConstants[1] = 0.0f; // Optional
            colorBlending.BlendConstants[2] = 0.0f; // Optional
            colorBlending.BlendConstants[3] = 0.0f; // Optional

            GraphicsPipelineCreateInfo pipelineInfo = default;
            pipelineInfo.SType = StructureType.GraphicsPipelineCreateInfo;
            pipelineInfo.StageCount = count;
            pipelineInfo.PStages = stages;
            pipelineInfo.PVertexInputState = &vertexInputInfo;
            pipelineInfo.PInputAssemblyState = &inputAssembly;
            pipelineInfo.PViewportState = &viewportState;
            pipelineInfo.PRasterizationState = &rasterizer;
            pipelineInfo.PMultisampleState = &multisampling;
            pipelineInfo.PDepthStencilState = null; // Optional
            pipelineInfo.PColorBlendState = &colorBlending;
            pipelineInfo.PDynamicState = &dynamicState;
            pipelineInfo.Layout = layout;

            pipelineInfo.RenderPass = renderPass.RenderPass;
            pipelineInfo.Subpass = 0;
            pipelineInfo.BasePipelineHandle = new(0);
            pipelineInfo.BasePipelineIndex = -1;

            var result = vk.CreateGraphicsPipelines(device.Device, new(0), 1, pipelineInfo, null, out pipeline);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            for (int i = 0; i < count; i++)
            {
                Free(stages[i].PName);
            }
        }

        private PipelineLayout CreateLayout()
        {
            PipelineLayoutCreateInfo pipelineLayoutInfo = default;
            pipelineLayoutInfo.SType = StructureType.PipelineLayoutCreateInfo;
            pipelineLayoutInfo.SetLayoutCount = 0; // Optional
            pipelineLayoutInfo.PSetLayouts = null; // Optional
            pipelineLayoutInfo.PushConstantRangeCount = 0; // Optional
            pipelineLayoutInfo.PPushConstantRanges = null; // Optional

            var result = vk.CreatePipelineLayout(device.Device, pipelineLayoutInfo, null, out var pipelineLayout);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            return pipelineLayout;
        }

        private ShaderModule CreateShaderModule(ShaderBlob blob)
        {
            ShaderModuleCreateInfo createInfo = default;
            createInfo.SType = StructureType.ShaderModuleCreateInfo;
            createInfo.CodeSize = blob.Length;
            createInfo.PCode = (uint*)blob.Data;

            var result = vk.CreateShaderModule(device.Device, createInfo, null, out var shaderModule);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            return shaderModule;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vk.DestroyPipeline(device.Device, pipeline, null);
                vk.DestroyPipelineLayout(device.Device, layout, null);
                if (vertexShader.Handle != 0)
                    vk.DestroyShaderModule(device.Device, vertexShader, null);
                if (tessControlShader.Handle != 0)
                    vk.DestroyShaderModule(device.Device, tessControlShader, null);
                if (tessEvaluationShader.Handle != 0)
                    vk.DestroyShaderModule(device.Device, tessEvaluationShader, null);
                if (geometryShader.Handle != 0)
                    vk.DestroyShaderModule(device.Device, geometryShader, null);
                if (fragmentShader.Handle != 0)
                    vk.DestroyShaderModule(device.Device, fragmentShader, null);
                disposedValue = true;
            }
        }

        ~VulkanGraphicsPipeline()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}