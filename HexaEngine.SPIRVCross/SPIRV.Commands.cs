namespace HexaEngine.SPIRVCross
{
    using System;
    using System.Runtime.InteropServices;

    public unsafe partial class SPIRV
    {
        internal const string LibName = "spirv-cross-c-shared";

        [LibraryImport(LibName, EntryPoint = "spvc_get_version")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcGetVersion(uint* major, uint* minor, uint* patch);

        [LibraryImport(LibName, EntryPoint = "spvc_get_commit_revision_and_timestamp")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcGetCommitRevisionAndTimestamp();

        [LibraryImport(LibName, EntryPoint = "spvc_msl_vertex_attribute_init")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcMslVertexAttributeInit(SpvcMslVertexAttribute* attr);

        [LibraryImport(LibName, EntryPoint = "spvc_msl_shader_input_init")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcMslShaderInputInit(SpvcMslShaderInput* input);

        [LibraryImport(LibName, EntryPoint = "spvc_msl_resource_binding_init")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcMslResourceBindingInit(SpvcMslResourceBinding* binding);

        [LibraryImport(LibName, EntryPoint = "spvc_msl_get_aux_buffer_struct_version")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcMslGetAuxBufferStructVersion();

        [LibraryImport(LibName, EntryPoint = "spvc_msl_constexpr_sampler_init")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcMslConstexprSamplerInit(SpvcMslConstexprSampler* sampler);

        [LibraryImport(LibName, EntryPoint = "spvc_msl_sampler_ycbcr_conversion_init")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcMslSamplerYcbcrConversionInit(SpvcMslSamplerYcbcrConversion* conv);

        [LibraryImport(LibName, EntryPoint = "spvc_hlsl_resource_binding_init")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcHlslResourceBindingInit(SpvcHlslResourceBinding* binding);

        [LibraryImport(LibName, EntryPoint = "spvc_context_create")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcContextCreate(SpvcContext* context);

        [LibraryImport(LibName, EntryPoint = "spvc_context_destroy")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcContextDestroy(SpvcContext context);

        [LibraryImport(LibName, EntryPoint = "spvc_context_release_allocations")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcContextReleaseAllocations(SpvcContext context);

        [LibraryImport(LibName, EntryPoint = "spvc_context_get_last_error_string")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcContextGetLastErrorString(SpvcContext context);

        [LibraryImport(LibName, EntryPoint = "spvc_context_set_error_callback")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcContextSetErrorCallback(SpvcContext context, SpvcErrorCallback cb, void* userdata);

        [LibraryImport(LibName, EntryPoint = "spvc_context_parse_spirv")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcContextParseSpirv(SpvcContext context, SpvId* spirv, nuint wordCount, SpvcParsedIr* parsedIr);

        [LibraryImport(LibName, EntryPoint = "spvc_context_create_compiler")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcContextCreateCompiler(SpvcContext context, SpvcBackend backend, SpvcParsedIr parsedIr, SpvcCaptureMode mode, SpvcCompiler* compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_current_id_bound")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerGetCurrentIdBound(SpvcCompiler compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_create_compiler_options")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerCreateCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions* options);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_options_set_bool")]
        public static extern SpvcResult SpvcCompilerOptionsSetBool(SpvcCompilerOptions options, SpvcCompilerOption option, bool value);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_options_set_uint")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerOptionsSetUint(SpvcCompilerOptions options, SpvcCompilerOption option, uint value);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_install_compiler_options")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerInstallCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions options);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_compile")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerCompile(SpvcCompiler compiler, byte* source);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_add_header_line")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerAddHeaderLine(SpvcCompiler compiler, byte* line);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_require_extension")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerRequireExtension(SpvcCompiler compiler, byte* ext);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_flatten_buffer_block")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerFlattenBufferBlock(SpvcCompiler compiler, uint id);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_variable_is_depth_or_compare")]
        public static extern bool SpvcCompilerVariableIsDepthOrCompare(SpvcCompiler compiler, uint id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_hlsl_set_root_constants_layout")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerHlslSetRootConstantsLayout(SpvcCompiler compiler, SpvcHlslRootConstants* constantInfo, nuint count);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_hlsl_add_vertex_attribute_remap")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerHlslAddVertexAttributeRemap(SpvcCompiler compiler, SpvcHlslVertexAttributeRemap* remap, nuint remaps);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_hlsl_remap_num_workgroups_builtin")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerHlslRemapNumWorkgroupsBuiltin(SpvcCompiler compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_hlsl_set_resource_binding_flags")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerHlslSetResourceBindingFlags(SpvcCompiler compiler, uint flags);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_hlsl_add_resource_binding")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerHlslAddResourceBinding(SpvcCompiler compiler, SpvcHlslResourceBinding* binding);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_hlsl_is_resource_used")]
        public static extern bool SpvcCompilerHlslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_is_rasterization_disabled")]
        public static extern bool SpvcCompilerMslIsRasterizationDisabled(SpvcCompiler compiler);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_needs_aux_buffer")]
        public static extern bool SpvcCompilerMslNeedsAuxBuffer(SpvcCompiler compiler);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_needs_swizzle_buffer")]
        public static extern bool SpvcCompilerMslNeedsSwizzleBuffer(SpvcCompiler compiler);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_needs_buffer_size_buffer")]
        public static extern bool SpvcCompilerMslNeedsBufferSizeBuffer(SpvcCompiler compiler);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_needs_output_buffer")]
        public static extern bool SpvcCompilerMslNeedsOutputBuffer(SpvcCompiler compiler);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_needs_patch_output_buffer")]
        public static extern bool SpvcCompilerMslNeedsPatchOutputBuffer(SpvcCompiler compiler);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_needs_input_threadgroup_mem")]
        public static extern bool SpvcCompilerMslNeedsInputThreadgroupMem(SpvcCompiler compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_add_vertex_attribute")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslAddVertexAttribute(SpvcCompiler compiler, SpvcMslVertexAttribute* attrs);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_add_resource_binding")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslAddResourceBinding(SpvcCompiler compiler, SpvcMslResourceBinding* binding);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_add_shader_input")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslAddShaderInput(SpvcCompiler compiler, SpvcMslShaderInput* input);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_add_discrete_descriptor_set")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslAddDiscreteDescriptorSet(SpvcCompiler compiler, uint descSet);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_set_argument_buffer_device_address_space")]
        public static extern SpvcResult SpvcCompilerMslSetArgumentBufferDeviceAddressSpace(SpvcCompiler compiler, uint descSet, bool deviceAddress);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_is_vertex_attribute_used")]
        public static extern bool SpvcCompilerMslIsVertexAttributeUsed(SpvcCompiler compiler, uint location);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_is_shader_input_used")]
        public static extern bool SpvcCompilerMslIsShaderInputUsed(SpvcCompiler compiler, uint location);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_msl_is_resource_used")]
        public static extern bool SpvcCompilerMslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_remap_constexpr_sampler")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslRemapConstexprSampler(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_remap_constexpr_sampler_by_binding")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslRemapConstexprSamplerByBinding(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_remap_constexpr_sampler_ycbcr")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslRemapConstexprSamplerYcbcr(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslRemapConstexprSamplerByBindingYcbcr(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_set_fragment_output_components")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslSetFragmentOutputComponents(SpvcCompiler compiler, uint location, uint components);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_get_automatic_resource_binding")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerMslGetAutomaticResourceBinding(SpvcCompiler compiler, uint id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_get_automatic_resource_binding_secondary")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerMslGetAutomaticResourceBindingSecondary(SpvcCompiler compiler, uint id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_add_dynamic_buffer")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslAddDynamicBuffer(SpvcCompiler compiler, uint descSet, uint binding, uint index);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_add_inline_uniform_block")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslAddInlineUniformBlock(SpvcCompiler compiler, uint descSet, uint binding);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_set_combined_sampler_suffix")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerMslSetCombinedSamplerSuffix(SpvcCompiler compiler, byte* suffix);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_msl_get_combined_sampler_suffix")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerMslGetCombinedSamplerSuffix(SpvcCompiler compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_active_interface_variables")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetActiveInterfaceVariables(SpvcCompiler compiler, SpvcSet* set);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_enabled_interface_variables")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerSetEnabledInterfaceVariables(SpvcCompiler compiler, SpvcSet set);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_create_shader_resources")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerCreateShaderResources(SpvcCompiler compiler, SpvcResources* resources);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_create_shader_resources_for_active_variables")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerCreateShaderResourcesForActiveVariables(SpvcCompiler compiler, SpvcResources* resources, SpvcSet active);

        [LibraryImport(LibName, EntryPoint = "spvc_resources_get_resource_list_for_type")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcResourcesGetResourceListForType(SpvcResources resources, SpvcResourceType type, SpvcReflectedResource* resourceList, nuint* resourceSize);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_decoration")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, uint argument);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_decoration_string")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, byte* argument);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_name")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetName(SpvcCompiler compiler, SpvId id, byte* argument);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_member_decoration")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, uint argument);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_member_decoration_string")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, byte* argument);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_member_name")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetMemberName(SpvcCompiler compiler, uint id, uint memberIndex, byte* argument);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_unset_decoration")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerUnsetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_unset_member_decoration")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerUnsetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_has_decoration")]
        public static extern bool SpvcCompilerHasDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_has_member_decoration")]
        public static extern bool SpvcCompilerHasMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_name")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerGetName(SpvcCompiler compiler, SpvId id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_decoration")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerGetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_decoration_string")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerGetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_member_decoration")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerGetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_member_decoration_string")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerGetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_member_name")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerGetMemberName(SpvcCompiler compiler, uint id, uint memberIndex);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_entry_points")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetEntryPoints(SpvcCompiler compiler, SpvcEntryPoint* entryPoints, nuint* numEntryPoints);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_entry_point")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerSetEntryPoint(SpvcCompiler compiler, byte* name, SpvExecutionModel model);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_rename_entry_point")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerRenameEntryPoint(SpvcCompiler compiler, byte* oldName, byte* newName, SpvExecutionModel model);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_cleansed_entry_point_name")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerGetCleansedEntryPointName(SpvcCompiler compiler, byte* name, SpvExecutionModel model);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_execution_mode")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_unset_execution_mode")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerUnsetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_set_execution_mode_with_arguments")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcCompilerSetExecutionModeWithArguments(SpvcCompiler compiler, SpvExecutionMode mode, uint arg0, uint arg1, uint arg2);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_execution_modes")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetExecutionModes(SpvcCompiler compiler, SpvExecutionMode* modes, nuint* numModes);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_execution_mode_argument")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerGetExecutionModeArgument(SpvcCompiler compiler, SpvExecutionMode mode);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_execution_mode_argument_by_index")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerGetExecutionModeArgumentByIndex(SpvcCompiler compiler, SpvExecutionMode mode, uint index);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_execution_model")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvExecutionModel SpvcCompilerGetExecutionModel(SpvcCompiler compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_type_handle")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcType SpvcCompilerGetTypeHandle(SpvcCompiler compiler, uint id);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_base_type_id")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetBaseTypeId(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_basetype")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcBasetype SpvcTypeGetBasetype(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_bit_width")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetBitWidth(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_vector_size")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetVectorSize(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_columns")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetColumns(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_num_array_dimensions")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetNumArrayDimensions(SpvcType type);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_type_array_dimension_is_literal")]
        public static extern bool SpvcTypeArrayDimensionIsLiteral(SpvcType type, uint dimension);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_array_dimension")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvId SpvcTypeGetArrayDimension(SpvcType type, uint dimension);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_num_member_types")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetNumMemberTypes(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_member_type")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetMemberType(SpvcType type, uint index);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_storage_class")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvStorageClass SpvcTypeGetStorageClass(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_image_sampled_type")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcTypeGetImageSampledType(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_image_dimension")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvDim SpvcTypeGetImageDimension(SpvcType type);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_type_get_image_is_depth")]
        public static extern bool SpvcTypeGetImageIsDepth(SpvcType type);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_type_get_image_arrayed")]
        public static extern bool SpvcTypeGetImageArrayed(SpvcType type);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_type_get_image_multisampled")]
        public static extern bool SpvcTypeGetImageMultisampled(SpvcType type);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_type_get_image_is_storage")]
        public static extern bool SpvcTypeGetImageIsStorage(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_image_storage_format")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvImageFormat SpvcTypeGetImageStorageFormat(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_type_get_image_access_qualifier")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvAccessQualifier SpvcTypeGetImageAccessQualifier(SpvcType type);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_declared_struct_size")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetDeclaredStructSize(SpvcCompiler compiler, SpvcType structType, nuint* size);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_declared_struct_size_runtime_array")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetDeclaredStructSizeRuntimeArray(SpvcCompiler compiler, SpvcType structType, nuint arraySize, nuint* size);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_declared_struct_member_size")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetDeclaredStructMemberSize(SpvcCompiler compiler, SpvcType type, uint index, nuint* size);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_type_struct_member_offset")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerTypeStructMemberOffset(SpvcCompiler compiler, SpvcType type, uint index, uint* offset);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_type_struct_member_array_stride")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerTypeStructMemberArrayStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_type_struct_member_matrix_stride")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerTypeStructMemberMatrixStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_build_dummy_sampler_for_combined_images")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerBuildDummySamplerForCombinedImages(SpvcCompiler compiler, uint* id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_build_combined_image_samplers")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerBuildCombinedImageSamplers(SpvcCompiler compiler);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_combined_image_samplers")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetCombinedImageSamplers(SpvcCompiler compiler, SpvcCombinedImageSampler* samplers, nuint* numSamplers);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_specialization_constants")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* constants, nuint* numConstants);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_constant_handle")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcConstant SpvcCompilerGetConstantHandle(SpvcCompiler compiler, uint id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_work_group_size_specialization_constants")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcCompilerGetWorkGroupSizeSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* x, SpvcSpecializationConstant* y, SpvcSpecializationConstant* z);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_active_buffer_ranges")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetActiveBufferRanges(SpvcCompiler compiler, uint id, SpvcBufferRange* ranges, nuint* numRanges);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_fp16")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial float SpvcConstantGetScalarFp16(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_fp32")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial float SpvcConstantGetScalarFp32(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_fp64")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial double SpvcConstantGetScalarFp64(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_u32")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcConstantGetScalarU32(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_i32")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int SpvcConstantGetScalarI32(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_u16")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcConstantGetScalarU16(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_i16")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int SpvcConstantGetScalarI16(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_u8")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcConstantGetScalarU8(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_scalar_i8")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int SpvcConstantGetScalarI8(SpvcConstant constant, uint column, uint row);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_subconstants")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial void SpvcConstantGetSubconstants(SpvcConstant constant, uint* constituents, nuint* count);

        [LibraryImport(LibName, EntryPoint = "spvc_constant_get_type")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial uint SpvcConstantGetType(SpvcConstant constant);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_get_binary_offset_for_decoration")]
        public static extern bool SpvcCompilerGetBinaryOffsetForDecoration(SpvcCompiler compiler, uint id, SpvDecoration decoration, uint* wordOffset);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_buffer_is_hlsl_counter_buffer")]
        public static extern bool SpvcCompilerBufferIsHlslCounterBuffer(SpvcCompiler compiler, uint id);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "spvc_compiler_buffer_get_hlsl_counter_buffer")]
        public static extern bool SpvcCompilerBufferGetHlslCounterBuffer(SpvcCompiler compiler, uint id, uint* counterId);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_declared_capabilities")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetDeclaredCapabilities(SpvcCompiler compiler, SpvCapability* capabilities, nuint* numCapabilities);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_declared_extensions")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetDeclaredExtensions(SpvcCompiler compiler, byte* extensions, nuint* numExtensions);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_remapped_declared_block_name")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial byte* SpvcCompilerGetRemappedDeclaredBlockName(SpvcCompiler compiler, uint id);

        [LibraryImport(LibName, EntryPoint = "spvc_compiler_get_buffer_block_decorations")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial SpvcResult SpvcCompilerGetBufferBlockDecorations(SpvcCompiler compiler, uint id, SpvDecoration* decorations, nuint* numDecorations);
    }
}