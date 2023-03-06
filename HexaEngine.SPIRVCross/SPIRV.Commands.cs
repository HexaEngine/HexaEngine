namespace HexaEngine.SPIRVCross
{
    using System;
    using System.Runtime.InteropServices;

    public unsafe partial class SPIRV
    {
        internal static IntPtr sNativeLibrary = LoadNativeLibrary();

        internal static T LoadFunction<T>(string name) => LibraryLoader.LoadFunction<T>(sNativeLibrary, name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcGetVersion(uint* major, uint* minor, uint* patch);

        private static readonly PFNSpvcGetVersion spvcGetVersion = LoadFunction<PFNSpvcGetVersion>("spvc_get_version");

        public static void SpvcGetVersion(uint* major, uint* minor, uint* patch)
        {
            spvcGetVersion(major, minor, patch);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcGetCommitRevisionAndTimestamp();

        private static readonly PFNSpvcGetCommitRevisionAndTimestamp spvcGetCommitRevisionAndTimestamp = LoadFunction<PFNSpvcGetCommitRevisionAndTimestamp>("spvc_get_commit_revision_and_timestamp");

        public static byte* SpvcGetCommitRevisionAndTimestamp()
        {
            return spvcGetCommitRevisionAndTimestamp();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcMslVertexAttributeInit(SpvcMslVertexAttribute* attr);

        private static readonly PFNSpvcMslVertexAttributeInit spvcMslVertexAttributeInit = LoadFunction<PFNSpvcMslVertexAttributeInit>("spvc_msl_vertex_attribute_init");

        public static void SpvcMslVertexAttributeInit(SpvcMslVertexAttribute* attr)
        {
            spvcMslVertexAttributeInit(attr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcMslShaderInputInit(SpvcMslShaderInput* input);

        private static readonly PFNSpvcMslShaderInputInit spvcMslShaderInputInit = LoadFunction<PFNSpvcMslShaderInputInit>("spvc_msl_shader_input_init");

        public static void SpvcMslShaderInputInit(SpvcMslShaderInput* input)
        {
            spvcMslShaderInputInit(input);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcMslResourceBindingInit(SpvcMslResourceBinding* binding);

        private static readonly PFNSpvcMslResourceBindingInit spvcMslResourceBindingInit = LoadFunction<PFNSpvcMslResourceBindingInit>("spvc_msl_resource_binding_init");

        public static void SpvcMslResourceBindingInit(SpvcMslResourceBinding* binding)
        {
            spvcMslResourceBindingInit(binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcMslGetAuxBufferStructVersion();

        private static readonly PFNSpvcMslGetAuxBufferStructVersion spvcMslGetAuxBufferStructVersion = LoadFunction<PFNSpvcMslGetAuxBufferStructVersion>("spvc_msl_get_aux_buffer_struct_version");

        public static uint SpvcMslGetAuxBufferStructVersion()
        {
            return spvcMslGetAuxBufferStructVersion();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcMslConstexprSamplerInit(SpvcMslConstexprSampler* sampler);

        private static readonly PFNSpvcMslConstexprSamplerInit spvcMslConstexprSamplerInit = LoadFunction<PFNSpvcMslConstexprSamplerInit>("spvc_msl_constexpr_sampler_init");

        public static void SpvcMslConstexprSamplerInit(SpvcMslConstexprSampler* sampler)
        {
            spvcMslConstexprSamplerInit(sampler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcMslSamplerYcbcrConversionInit(SpvcMslSamplerYcbcrConversion* conv);

        private static readonly PFNSpvcMslSamplerYcbcrConversionInit spvcMslSamplerYcbcrConversionInit = LoadFunction<PFNSpvcMslSamplerYcbcrConversionInit>("spvc_msl_sampler_ycbcr_conversion_init");

        public static void SpvcMslSamplerYcbcrConversionInit(SpvcMslSamplerYcbcrConversion* conv)
        {
            spvcMslSamplerYcbcrConversionInit(conv);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcHlslResourceBindingInit(SpvcHlslResourceBinding* binding);

        private static readonly PFNSpvcHlslResourceBindingInit spvcHlslResourceBindingInit = LoadFunction<PFNSpvcHlslResourceBindingInit>("spvc_hlsl_resource_binding_init");

        public static void SpvcHlslResourceBindingInit(SpvcHlslResourceBinding* binding)
        {
            spvcHlslResourceBindingInit(binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcContextCreate(SpvcContext* context);

        private static readonly PFNSpvcContextCreate spvcContextCreate = LoadFunction<PFNSpvcContextCreate>("spvc_context_create");

        public static SpvcResult SpvcContextCreate(SpvcContext* context)
        {
            return spvcContextCreate(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcContextDestroy(SpvcContext context);

        private static readonly PFNSpvcContextDestroy spvcContextDestroy = LoadFunction<PFNSpvcContextDestroy>("spvc_context_destroy");

        public static void SpvcContextDestroy(SpvcContext context)
        {
            spvcContextDestroy(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcContextReleaseAllocations(SpvcContext context);

        private static readonly PFNSpvcContextReleaseAllocations spvcContextReleaseAllocations = LoadFunction<PFNSpvcContextReleaseAllocations>("spvc_context_release_allocations");

        public static void SpvcContextReleaseAllocations(SpvcContext context)
        {
            spvcContextReleaseAllocations(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcContextGetLastErrorString(SpvcContext context);

        private static readonly PFNSpvcContextGetLastErrorString spvcContextGetLastErrorString = LoadFunction<PFNSpvcContextGetLastErrorString>("spvc_context_get_last_error_string");

        public static byte* SpvcContextGetLastErrorString(SpvcContext context)
        {
            return spvcContextGetLastErrorString(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcContextSetErrorCallback(SpvcContext context, SpvcErrorCallback cb, void* userdata);

        private static readonly PFNSpvcContextSetErrorCallback spvcContextSetErrorCallback = LoadFunction<PFNSpvcContextSetErrorCallback>("spvc_context_set_error_callback");

        public static void SpvcContextSetErrorCallback(SpvcContext context, SpvcErrorCallback cb, void* userdata)
        {
            spvcContextSetErrorCallback(context, cb, userdata);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcContextParseSpirv(SpvcContext context, SpvId* spirv, nuint wordCount, SpvcParsedIr* parsedIr);

        private static readonly PFNSpvcContextParseSpirv spvcContextParseSpirv = LoadFunction<PFNSpvcContextParseSpirv>("spvc_context_parse_spirv");

        public static SpvcResult SpvcContextParseSpirv(SpvcContext context, SpvId* spirv, nuint wordCount, SpvcParsedIr* parsedIr)
        {
            return spvcContextParseSpirv(context, spirv, wordCount, parsedIr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcContextCreateCompiler(SpvcContext context, SpvcBackend backend, SpvcParsedIr parsedIr, SpvcCaptureMode mode, SpvcCompiler* compiler);

        private static readonly PFNSpvcContextCreateCompiler spvcContextCreateCompiler = LoadFunction<PFNSpvcContextCreateCompiler>("spvc_context_create_compiler");

        public static SpvcResult SpvcContextCreateCompiler(SpvcContext context, SpvcBackend backend, SpvcParsedIr parsedIr, SpvcCaptureMode mode, SpvcCompiler* compiler)
        {
            return spvcContextCreateCompiler(context, backend, parsedIr, mode, compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerGetCurrentIdBound(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerGetCurrentIdBound spvcCompilerGetCurrentIdBound = LoadFunction<PFNSpvcCompilerGetCurrentIdBound>("spvc_compiler_get_current_id_bound");

        public static uint SpvcCompilerGetCurrentIdBound(SpvcCompiler compiler)
        {
            return spvcCompilerGetCurrentIdBound(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerCreateCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions* options);

        private static readonly PFNSpvcCompilerCreateCompilerOptions spvcCompilerCreateCompilerOptions = LoadFunction<PFNSpvcCompilerCreateCompilerOptions>("spvc_compiler_create_compiler_options");

        public static SpvcResult SpvcCompilerCreateCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions* options)
        {
            return spvcCompilerCreateCompilerOptions(compiler, options);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerOptionsSetBool(SpvcCompilerOptions options, SpvcCompilerOption option, bool value);

        private static readonly PFNSpvcCompilerOptionsSetBool spvcCompilerOptionsSetBool = LoadFunction<PFNSpvcCompilerOptionsSetBool>("spvc_compiler_options_set_bool");

        public static SpvcResult SpvcCompilerOptionsSetBool(SpvcCompilerOptions options, SpvcCompilerOption option, bool value)
        {
            return spvcCompilerOptionsSetBool(options, option, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerOptionsSetUint(SpvcCompilerOptions options, SpvcCompilerOption option, uint value);

        private static readonly PFNSpvcCompilerOptionsSetUint spvcCompilerOptionsSetUint = LoadFunction<PFNSpvcCompilerOptionsSetUint>("spvc_compiler_options_set_uint");

        public static SpvcResult SpvcCompilerOptionsSetUint(SpvcCompilerOptions options, SpvcCompilerOption option, uint value)
        {
            return spvcCompilerOptionsSetUint(options, option, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerInstallCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions options);

        private static readonly PFNSpvcCompilerInstallCompilerOptions spvcCompilerInstallCompilerOptions = LoadFunction<PFNSpvcCompilerInstallCompilerOptions>("spvc_compiler_install_compiler_options");

        public static SpvcResult SpvcCompilerInstallCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions options)
        {
            return spvcCompilerInstallCompilerOptions(compiler, options);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerCompile(SpvcCompiler compiler, byte* source);

        private static readonly PFNSpvcCompilerCompile spvcCompilerCompile = LoadFunction<PFNSpvcCompilerCompile>("spvc_compiler_compile");

        public static SpvcResult SpvcCompilerCompile(SpvcCompiler compiler, byte* source)
        {
            return spvcCompilerCompile(compiler, source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerAddHeaderLine(SpvcCompiler compiler, byte* line);

        private static readonly PFNSpvcCompilerAddHeaderLine spvcCompilerAddHeaderLine = LoadFunction<PFNSpvcCompilerAddHeaderLine>("spvc_compiler_add_header_line");

        public static SpvcResult SpvcCompilerAddHeaderLine(SpvcCompiler compiler, byte* line)
        {
            return spvcCompilerAddHeaderLine(compiler, line);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerRequireExtension(SpvcCompiler compiler, byte* ext);

        private static readonly PFNSpvcCompilerRequireExtension spvcCompilerRequireExtension = LoadFunction<PFNSpvcCompilerRequireExtension>("spvc_compiler_require_extension");

        public static SpvcResult SpvcCompilerRequireExtension(SpvcCompiler compiler, byte* ext)
        {
            return spvcCompilerRequireExtension(compiler, ext);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerFlattenBufferBlock(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerFlattenBufferBlock spvcCompilerFlattenBufferBlock = LoadFunction<PFNSpvcCompilerFlattenBufferBlock>("spvc_compiler_flatten_buffer_block");

        public static SpvcResult SpvcCompilerFlattenBufferBlock(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerFlattenBufferBlock(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerVariableIsDepthOrCompare(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerVariableIsDepthOrCompare spvcCompilerVariableIsDepthOrCompare = LoadFunction<PFNSpvcCompilerVariableIsDepthOrCompare>("spvc_compiler_variable_is_depth_or_compare");

        public static bool SpvcCompilerVariableIsDepthOrCompare(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerVariableIsDepthOrCompare(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerHlslSetRootConstantsLayout(SpvcCompiler compiler, SpvcHlslRootConstants* constantInfo, nuint count);

        private static readonly PFNSpvcCompilerHlslSetRootConstantsLayout spvcCompilerHlslSetRootConstantsLayout = LoadFunction<PFNSpvcCompilerHlslSetRootConstantsLayout>("spvc_compiler_hlsl_set_root_constants_layout");

        public static SpvcResult SpvcCompilerHlslSetRootConstantsLayout(SpvcCompiler compiler, SpvcHlslRootConstants* constantInfo, nuint count)
        {
            return spvcCompilerHlslSetRootConstantsLayout(compiler, constantInfo, count);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerHlslAddVertexAttributeRemap(SpvcCompiler compiler, SpvcHlslVertexAttributeRemap* remap, nuint remaps);

        private static readonly PFNSpvcCompilerHlslAddVertexAttributeRemap spvcCompilerHlslAddVertexAttributeRemap = LoadFunction<PFNSpvcCompilerHlslAddVertexAttributeRemap>("spvc_compiler_hlsl_add_vertex_attribute_remap");

        public static SpvcResult SpvcCompilerHlslAddVertexAttributeRemap(SpvcCompiler compiler, SpvcHlslVertexAttributeRemap* remap, nuint remaps)
        {
            return spvcCompilerHlslAddVertexAttributeRemap(compiler, remap, remaps);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerHlslRemapNumWorkgroupsBuiltin(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerHlslRemapNumWorkgroupsBuiltin spvcCompilerHlslRemapNumWorkgroupsBuiltin = LoadFunction<PFNSpvcCompilerHlslRemapNumWorkgroupsBuiltin>("spvc_compiler_hlsl_remap_num_workgroups_builtin");

        public static uint SpvcCompilerHlslRemapNumWorkgroupsBuiltin(SpvcCompiler compiler)
        {
            return spvcCompilerHlslRemapNumWorkgroupsBuiltin(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerHlslSetResourceBindingFlags(SpvcCompiler compiler, uint flags);

        private static readonly PFNSpvcCompilerHlslSetResourceBindingFlags spvcCompilerHlslSetResourceBindingFlags = LoadFunction<PFNSpvcCompilerHlslSetResourceBindingFlags>("spvc_compiler_hlsl_set_resource_binding_flags");

        public static SpvcResult SpvcCompilerHlslSetResourceBindingFlags(SpvcCompiler compiler, uint flags)
        {
            return spvcCompilerHlslSetResourceBindingFlags(compiler, flags);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerHlslAddResourceBinding(SpvcCompiler compiler, SpvcHlslResourceBinding* binding);

        private static readonly PFNSpvcCompilerHlslAddResourceBinding spvcCompilerHlslAddResourceBinding = LoadFunction<PFNSpvcCompilerHlslAddResourceBinding>("spvc_compiler_hlsl_add_resource_binding");

        public static SpvcResult SpvcCompilerHlslAddResourceBinding(SpvcCompiler compiler, SpvcHlslResourceBinding* binding)
        {
            return spvcCompilerHlslAddResourceBinding(compiler, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerHlslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding);

        private static readonly PFNSpvcCompilerHlslIsResourceUsed spvcCompilerHlslIsResourceUsed = LoadFunction<PFNSpvcCompilerHlslIsResourceUsed>("spvc_compiler_hlsl_is_resource_used");

        public static bool SpvcCompilerHlslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding)
        {
            return spvcCompilerHlslIsResourceUsed(compiler, model, set, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslIsRasterizationDisabled(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslIsRasterizationDisabled spvcCompilerMslIsRasterizationDisabled = LoadFunction<PFNSpvcCompilerMslIsRasterizationDisabled>("spvc_compiler_msl_is_rasterization_disabled");

        public static bool SpvcCompilerMslIsRasterizationDisabled(SpvcCompiler compiler)
        {
            return spvcCompilerMslIsRasterizationDisabled(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslNeedsAuxBuffer(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslNeedsAuxBuffer spvcCompilerMslNeedsAuxBuffer = LoadFunction<PFNSpvcCompilerMslNeedsAuxBuffer>("spvc_compiler_msl_needs_aux_buffer");

        public static bool SpvcCompilerMslNeedsAuxBuffer(SpvcCompiler compiler)
        {
            return spvcCompilerMslNeedsAuxBuffer(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslNeedsSwizzleBuffer(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslNeedsSwizzleBuffer spvcCompilerMslNeedsSwizzleBuffer = LoadFunction<PFNSpvcCompilerMslNeedsSwizzleBuffer>("spvc_compiler_msl_needs_swizzle_buffer");

        public static bool SpvcCompilerMslNeedsSwizzleBuffer(SpvcCompiler compiler)
        {
            return spvcCompilerMslNeedsSwizzleBuffer(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslNeedsBufferSizeBuffer(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslNeedsBufferSizeBuffer spvcCompilerMslNeedsBufferSizeBuffer = LoadFunction<PFNSpvcCompilerMslNeedsBufferSizeBuffer>("spvc_compiler_msl_needs_buffer_size_buffer");

        public static bool SpvcCompilerMslNeedsBufferSizeBuffer(SpvcCompiler compiler)
        {
            return spvcCompilerMslNeedsBufferSizeBuffer(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslNeedsOutputBuffer(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslNeedsOutputBuffer spvcCompilerMslNeedsOutputBuffer = LoadFunction<PFNSpvcCompilerMslNeedsOutputBuffer>("spvc_compiler_msl_needs_output_buffer");

        public static bool SpvcCompilerMslNeedsOutputBuffer(SpvcCompiler compiler)
        {
            return spvcCompilerMslNeedsOutputBuffer(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslNeedsPatchOutputBuffer(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslNeedsPatchOutputBuffer spvcCompilerMslNeedsPatchOutputBuffer = LoadFunction<PFNSpvcCompilerMslNeedsPatchOutputBuffer>("spvc_compiler_msl_needs_patch_output_buffer");

        public static bool SpvcCompilerMslNeedsPatchOutputBuffer(SpvcCompiler compiler)
        {
            return spvcCompilerMslNeedsPatchOutputBuffer(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslNeedsInputThreadgroupMem(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslNeedsInputThreadgroupMem spvcCompilerMslNeedsInputThreadgroupMem = LoadFunction<PFNSpvcCompilerMslNeedsInputThreadgroupMem>("spvc_compiler_msl_needs_input_threadgroup_mem");

        public static bool SpvcCompilerMslNeedsInputThreadgroupMem(SpvcCompiler compiler)
        {
            return spvcCompilerMslNeedsInputThreadgroupMem(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslAddVertexAttribute(SpvcCompiler compiler, SpvcMslVertexAttribute* attrs);

        private static readonly PFNSpvcCompilerMslAddVertexAttribute spvcCompilerMslAddVertexAttribute = LoadFunction<PFNSpvcCompilerMslAddVertexAttribute>("spvc_compiler_msl_add_vertex_attribute");

        public static SpvcResult SpvcCompilerMslAddVertexAttribute(SpvcCompiler compiler, SpvcMslVertexAttribute* attrs)
        {
            return spvcCompilerMslAddVertexAttribute(compiler, attrs);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslAddResourceBinding(SpvcCompiler compiler, SpvcMslResourceBinding* binding);

        private static readonly PFNSpvcCompilerMslAddResourceBinding spvcCompilerMslAddResourceBinding = LoadFunction<PFNSpvcCompilerMslAddResourceBinding>("spvc_compiler_msl_add_resource_binding");

        public static SpvcResult SpvcCompilerMslAddResourceBinding(SpvcCompiler compiler, SpvcMslResourceBinding* binding)
        {
            return spvcCompilerMslAddResourceBinding(compiler, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslAddShaderInput(SpvcCompiler compiler, SpvcMslShaderInput* input);

        private static readonly PFNSpvcCompilerMslAddShaderInput spvcCompilerMslAddShaderInput = LoadFunction<PFNSpvcCompilerMslAddShaderInput>("spvc_compiler_msl_add_shader_input");

        public static SpvcResult SpvcCompilerMslAddShaderInput(SpvcCompiler compiler, SpvcMslShaderInput* input)
        {
            return spvcCompilerMslAddShaderInput(compiler, input);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslAddDiscreteDescriptorSet(SpvcCompiler compiler, uint descSet);

        private static readonly PFNSpvcCompilerMslAddDiscreteDescriptorSet spvcCompilerMslAddDiscreteDescriptorSet = LoadFunction<PFNSpvcCompilerMslAddDiscreteDescriptorSet>("spvc_compiler_msl_add_discrete_descriptor_set");

        public static SpvcResult SpvcCompilerMslAddDiscreteDescriptorSet(SpvcCompiler compiler, uint descSet)
        {
            return spvcCompilerMslAddDiscreteDescriptorSet(compiler, descSet);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslSetArgumentBufferDeviceAddressSpace(SpvcCompiler compiler, uint descSet, bool deviceAddress);

        private static readonly PFNSpvcCompilerMslSetArgumentBufferDeviceAddressSpace spvcCompilerMslSetArgumentBufferDeviceAddressSpace = LoadFunction<PFNSpvcCompilerMslSetArgumentBufferDeviceAddressSpace>("spvc_compiler_msl_set_argument_buffer_device_address_space");

        public static SpvcResult SpvcCompilerMslSetArgumentBufferDeviceAddressSpace(SpvcCompiler compiler, uint descSet, bool deviceAddress)
        {
            return spvcCompilerMslSetArgumentBufferDeviceAddressSpace(compiler, descSet, deviceAddress);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslIsVertexAttributeUsed(SpvcCompiler compiler, uint location);

        private static readonly PFNSpvcCompilerMslIsVertexAttributeUsed spvcCompilerMslIsVertexAttributeUsed = LoadFunction<PFNSpvcCompilerMslIsVertexAttributeUsed>("spvc_compiler_msl_is_vertex_attribute_used");

        public static bool SpvcCompilerMslIsVertexAttributeUsed(SpvcCompiler compiler, uint location)
        {
            return spvcCompilerMslIsVertexAttributeUsed(compiler, location);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslIsShaderInputUsed(SpvcCompiler compiler, uint location);

        private static readonly PFNSpvcCompilerMslIsShaderInputUsed spvcCompilerMslIsShaderInputUsed = LoadFunction<PFNSpvcCompilerMslIsShaderInputUsed>("spvc_compiler_msl_is_shader_input_used");

        public static bool SpvcCompilerMslIsShaderInputUsed(SpvcCompiler compiler, uint location)
        {
            return spvcCompilerMslIsShaderInputUsed(compiler, location);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerMslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding);

        private static readonly PFNSpvcCompilerMslIsResourceUsed spvcCompilerMslIsResourceUsed = LoadFunction<PFNSpvcCompilerMslIsResourceUsed>("spvc_compiler_msl_is_resource_used");

        public static bool SpvcCompilerMslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding)
        {
            return spvcCompilerMslIsResourceUsed(compiler, model, set, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslRemapConstexprSampler(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler);

        private static readonly PFNSpvcCompilerMslRemapConstexprSampler spvcCompilerMslRemapConstexprSampler = LoadFunction<PFNSpvcCompilerMslRemapConstexprSampler>("spvc_compiler_msl_remap_constexpr_sampler");

        public static SpvcResult SpvcCompilerMslRemapConstexprSampler(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler)
        {
            return spvcCompilerMslRemapConstexprSampler(compiler, id, sampler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslRemapConstexprSamplerByBinding(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler);

        private static readonly PFNSpvcCompilerMslRemapConstexprSamplerByBinding spvcCompilerMslRemapConstexprSamplerByBinding = LoadFunction<PFNSpvcCompilerMslRemapConstexprSamplerByBinding>("spvc_compiler_msl_remap_constexpr_sampler_by_binding");

        public static SpvcResult SpvcCompilerMslRemapConstexprSamplerByBinding(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler)
        {
            return spvcCompilerMslRemapConstexprSamplerByBinding(compiler, descSet, binding, sampler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslRemapConstexprSamplerYcbcr(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv);

        private static readonly PFNSpvcCompilerMslRemapConstexprSamplerYcbcr spvcCompilerMslRemapConstexprSamplerYcbcr = LoadFunction<PFNSpvcCompilerMslRemapConstexprSamplerYcbcr>("spvc_compiler_msl_remap_constexpr_sampler_ycbcr");

        public static SpvcResult SpvcCompilerMslRemapConstexprSamplerYcbcr(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv)
        {
            return spvcCompilerMslRemapConstexprSamplerYcbcr(compiler, id, sampler, conv);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslRemapConstexprSamplerByBindingYcbcr(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv);

        private static readonly PFNSpvcCompilerMslRemapConstexprSamplerByBindingYcbcr spvcCompilerMslRemapConstexprSamplerByBindingYcbcr = LoadFunction<PFNSpvcCompilerMslRemapConstexprSamplerByBindingYcbcr>("spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr");

        public static SpvcResult SpvcCompilerMslRemapConstexprSamplerByBindingYcbcr(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv)
        {
            return spvcCompilerMslRemapConstexprSamplerByBindingYcbcr(compiler, descSet, binding, sampler, conv);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslSetFragmentOutputComponents(SpvcCompiler compiler, uint location, uint components);

        private static readonly PFNSpvcCompilerMslSetFragmentOutputComponents spvcCompilerMslSetFragmentOutputComponents = LoadFunction<PFNSpvcCompilerMslSetFragmentOutputComponents>("spvc_compiler_msl_set_fragment_output_components");

        public static SpvcResult SpvcCompilerMslSetFragmentOutputComponents(SpvcCompiler compiler, uint location, uint components)
        {
            return spvcCompilerMslSetFragmentOutputComponents(compiler, location, components);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerMslGetAutomaticResourceBinding(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerMslGetAutomaticResourceBinding spvcCompilerMslGetAutomaticResourceBinding = LoadFunction<PFNSpvcCompilerMslGetAutomaticResourceBinding>("spvc_compiler_msl_get_automatic_resource_binding");

        public static uint SpvcCompilerMslGetAutomaticResourceBinding(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerMslGetAutomaticResourceBinding(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerMslGetAutomaticResourceBindingSecondary(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerMslGetAutomaticResourceBindingSecondary spvcCompilerMslGetAutomaticResourceBindingSecondary = LoadFunction<PFNSpvcCompilerMslGetAutomaticResourceBindingSecondary>("spvc_compiler_msl_get_automatic_resource_binding_secondary");

        public static uint SpvcCompilerMslGetAutomaticResourceBindingSecondary(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerMslGetAutomaticResourceBindingSecondary(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslAddDynamicBuffer(SpvcCompiler compiler, uint descSet, uint binding, uint index);

        private static readonly PFNSpvcCompilerMslAddDynamicBuffer spvcCompilerMslAddDynamicBuffer = LoadFunction<PFNSpvcCompilerMslAddDynamicBuffer>("spvc_compiler_msl_add_dynamic_buffer");

        public static SpvcResult SpvcCompilerMslAddDynamicBuffer(SpvcCompiler compiler, uint descSet, uint binding, uint index)
        {
            return spvcCompilerMslAddDynamicBuffer(compiler, descSet, binding, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslAddInlineUniformBlock(SpvcCompiler compiler, uint descSet, uint binding);

        private static readonly PFNSpvcCompilerMslAddInlineUniformBlock spvcCompilerMslAddInlineUniformBlock = LoadFunction<PFNSpvcCompilerMslAddInlineUniformBlock>("spvc_compiler_msl_add_inline_uniform_block");

        public static SpvcResult SpvcCompilerMslAddInlineUniformBlock(SpvcCompiler compiler, uint descSet, uint binding)
        {
            return spvcCompilerMslAddInlineUniformBlock(compiler, descSet, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerMslSetCombinedSamplerSuffix(SpvcCompiler compiler, byte* suffix);

        private static readonly PFNSpvcCompilerMslSetCombinedSamplerSuffix spvcCompilerMslSetCombinedSamplerSuffix = LoadFunction<PFNSpvcCompilerMslSetCombinedSamplerSuffix>("spvc_compiler_msl_set_combined_sampler_suffix");

        public static SpvcResult SpvcCompilerMslSetCombinedSamplerSuffix(SpvcCompiler compiler, byte* suffix)
        {
            return spvcCompilerMslSetCombinedSamplerSuffix(compiler, suffix);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerMslGetCombinedSamplerSuffix(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerMslGetCombinedSamplerSuffix spvcCompilerMslGetCombinedSamplerSuffix = LoadFunction<PFNSpvcCompilerMslGetCombinedSamplerSuffix>("spvc_compiler_msl_get_combined_sampler_suffix");

        public static byte* SpvcCompilerMslGetCombinedSamplerSuffix(SpvcCompiler compiler)
        {
            return spvcCompilerMslGetCombinedSamplerSuffix(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetActiveInterfaceVariables(SpvcCompiler compiler, SpvcSet* set);

        private static readonly PFNSpvcCompilerGetActiveInterfaceVariables spvcCompilerGetActiveInterfaceVariables = LoadFunction<PFNSpvcCompilerGetActiveInterfaceVariables>("spvc_compiler_get_active_interface_variables");

        public static SpvcResult SpvcCompilerGetActiveInterfaceVariables(SpvcCompiler compiler, SpvcSet* set)
        {
            return spvcCompilerGetActiveInterfaceVariables(compiler, set);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerSetEnabledInterfaceVariables(SpvcCompiler compiler, SpvcSet set);

        private static readonly PFNSpvcCompilerSetEnabledInterfaceVariables spvcCompilerSetEnabledInterfaceVariables = LoadFunction<PFNSpvcCompilerSetEnabledInterfaceVariables>("spvc_compiler_set_enabled_interface_variables");

        public static SpvcResult SpvcCompilerSetEnabledInterfaceVariables(SpvcCompiler compiler, SpvcSet set)
        {
            return spvcCompilerSetEnabledInterfaceVariables(compiler, set);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerCreateShaderResources(SpvcCompiler compiler, SpvcResources* resources);

        private static readonly PFNSpvcCompilerCreateShaderResources spvcCompilerCreateShaderResources = LoadFunction<PFNSpvcCompilerCreateShaderResources>("spvc_compiler_create_shader_resources");

        public static SpvcResult SpvcCompilerCreateShaderResources(SpvcCompiler compiler, SpvcResources* resources)
        {
            return spvcCompilerCreateShaderResources(compiler, resources);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerCreateShaderResourcesForActiveVariables(SpvcCompiler compiler, SpvcResources* resources, SpvcSet active);

        private static readonly PFNSpvcCompilerCreateShaderResourcesForActiveVariables spvcCompilerCreateShaderResourcesForActiveVariables = LoadFunction<PFNSpvcCompilerCreateShaderResourcesForActiveVariables>("spvc_compiler_create_shader_resources_for_active_variables");

        public static SpvcResult SpvcCompilerCreateShaderResourcesForActiveVariables(SpvcCompiler compiler, SpvcResources* resources, SpvcSet active)
        {
            return spvcCompilerCreateShaderResourcesForActiveVariables(compiler, resources, active);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcResourcesGetResourceListForType(SpvcResources resources, SpvcResourceType type, SpvcReflectedResource* resourceList, nuint* resourceSize);

        private static readonly PFNSpvcResourcesGetResourceListForType spvcResourcesGetResourceListForType = LoadFunction<PFNSpvcResourcesGetResourceListForType>("spvc_resources_get_resource_list_for_type");

        public static SpvcResult SpvcResourcesGetResourceListForType(SpvcResources resources, SpvcResourceType type, SpvcReflectedResource* resourceList, nuint* resourceSize)
        {
            return spvcResourcesGetResourceListForType(resources, type, resourceList, resourceSize);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, uint argument);

        private static readonly PFNSpvcCompilerSetDecoration spvcCompilerSetDecoration = LoadFunction<PFNSpvcCompilerSetDecoration>("spvc_compiler_set_decoration");

        public static void SpvcCompilerSetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, uint argument)
        {
            spvcCompilerSetDecoration(compiler, id, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, byte* argument);

        private static readonly PFNSpvcCompilerSetDecorationString spvcCompilerSetDecorationString = LoadFunction<PFNSpvcCompilerSetDecorationString>("spvc_compiler_set_decoration_string");

        public static void SpvcCompilerSetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, byte* argument)
        {
            spvcCompilerSetDecorationString(compiler, id, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetName(SpvcCompiler compiler, SpvId id, byte* argument);

        private static readonly PFNSpvcCompilerSetName spvcCompilerSetName = LoadFunction<PFNSpvcCompilerSetName>("spvc_compiler_set_name");

        public static void SpvcCompilerSetName(SpvcCompiler compiler, SpvId id, byte* argument)
        {
            spvcCompilerSetName(compiler, id, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, uint argument);

        private static readonly PFNSpvcCompilerSetMemberDecoration spvcCompilerSetMemberDecoration = LoadFunction<PFNSpvcCompilerSetMemberDecoration>("spvc_compiler_set_member_decoration");

        public static void SpvcCompilerSetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, uint argument)
        {
            spvcCompilerSetMemberDecoration(compiler, id, memberIndex, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, byte* argument);

        private static readonly PFNSpvcCompilerSetMemberDecorationString spvcCompilerSetMemberDecorationString = LoadFunction<PFNSpvcCompilerSetMemberDecorationString>("spvc_compiler_set_member_decoration_string");

        public static void SpvcCompilerSetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, byte* argument)
        {
            spvcCompilerSetMemberDecorationString(compiler, id, memberIndex, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetMemberName(SpvcCompiler compiler, uint id, uint memberIndex, byte* argument);

        private static readonly PFNSpvcCompilerSetMemberName spvcCompilerSetMemberName = LoadFunction<PFNSpvcCompilerSetMemberName>("spvc_compiler_set_member_name");

        public static void SpvcCompilerSetMemberName(SpvcCompiler compiler, uint id, uint memberIndex, byte* argument)
        {
            spvcCompilerSetMemberName(compiler, id, memberIndex, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerUnsetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerUnsetDecoration spvcCompilerUnsetDecoration = LoadFunction<PFNSpvcCompilerUnsetDecoration>("spvc_compiler_unset_decoration");

        public static void SpvcCompilerUnsetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            spvcCompilerUnsetDecoration(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerUnsetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerUnsetMemberDecoration spvcCompilerUnsetMemberDecoration = LoadFunction<PFNSpvcCompilerUnsetMemberDecoration>("spvc_compiler_unset_member_decoration");

        public static void SpvcCompilerUnsetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            spvcCompilerUnsetMemberDecoration(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerHasDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerHasDecoration spvcCompilerHasDecoration = LoadFunction<PFNSpvcCompilerHasDecoration>("spvc_compiler_has_decoration");

        public static bool SpvcCompilerHasDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            return spvcCompilerHasDecoration(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerHasMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerHasMemberDecoration spvcCompilerHasMemberDecoration = LoadFunction<PFNSpvcCompilerHasMemberDecoration>("spvc_compiler_has_member_decoration");

        public static bool SpvcCompilerHasMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            return spvcCompilerHasMemberDecoration(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerGetName(SpvcCompiler compiler, SpvId id);

        private static readonly PFNSpvcCompilerGetName spvcCompilerGetName = LoadFunction<PFNSpvcCompilerGetName>("spvc_compiler_get_name");

        public static byte* SpvcCompilerGetName(SpvcCompiler compiler, SpvId id)
        {
            return spvcCompilerGetName(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerGetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerGetDecoration spvcCompilerGetDecoration = LoadFunction<PFNSpvcCompilerGetDecoration>("spvc_compiler_get_decoration");

        public static uint SpvcCompilerGetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            return spvcCompilerGetDecoration(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerGetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerGetDecorationString spvcCompilerGetDecorationString = LoadFunction<PFNSpvcCompilerGetDecorationString>("spvc_compiler_get_decoration_string");

        public static byte* SpvcCompilerGetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            return spvcCompilerGetDecorationString(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerGetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerGetMemberDecoration spvcCompilerGetMemberDecoration = LoadFunction<PFNSpvcCompilerGetMemberDecoration>("spvc_compiler_get_member_decoration");

        public static uint SpvcCompilerGetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            return spvcCompilerGetMemberDecoration(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerGetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration);

        private static readonly PFNSpvcCompilerGetMemberDecorationString spvcCompilerGetMemberDecorationString = LoadFunction<PFNSpvcCompilerGetMemberDecorationString>("spvc_compiler_get_member_decoration_string");

        public static byte* SpvcCompilerGetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            return spvcCompilerGetMemberDecorationString(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerGetMemberName(SpvcCompiler compiler, uint id, uint memberIndex);

        private static readonly PFNSpvcCompilerGetMemberName spvcCompilerGetMemberName = LoadFunction<PFNSpvcCompilerGetMemberName>("spvc_compiler_get_member_name");

        public static byte* SpvcCompilerGetMemberName(SpvcCompiler compiler, uint id, uint memberIndex)
        {
            return spvcCompilerGetMemberName(compiler, id, memberIndex);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetEntryPoints(SpvcCompiler compiler, SpvcEntryPoint* entryPoints, nuint* numEntryPoints);

        private static readonly PFNSpvcCompilerGetEntryPoints spvcCompilerGetEntryPoints = LoadFunction<PFNSpvcCompilerGetEntryPoints>("spvc_compiler_get_entry_points");

        public static SpvcResult SpvcCompilerGetEntryPoints(SpvcCompiler compiler, SpvcEntryPoint* entryPoints, nuint* numEntryPoints)
        {
            return spvcCompilerGetEntryPoints(compiler, entryPoints, numEntryPoints);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerSetEntryPoint(SpvcCompiler compiler, byte* name, SpvExecutionModel model);

        private static readonly PFNSpvcCompilerSetEntryPoint spvcCompilerSetEntryPoint = LoadFunction<PFNSpvcCompilerSetEntryPoint>("spvc_compiler_set_entry_point");

        public static SpvcResult SpvcCompilerSetEntryPoint(SpvcCompiler compiler, byte* name, SpvExecutionModel model)
        {
            return spvcCompilerSetEntryPoint(compiler, name, model);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerRenameEntryPoint(SpvcCompiler compiler, byte* oldName, byte* newName, SpvExecutionModel model);

        private static readonly PFNSpvcCompilerRenameEntryPoint spvcCompilerRenameEntryPoint = LoadFunction<PFNSpvcCompilerRenameEntryPoint>("spvc_compiler_rename_entry_point");

        public static SpvcResult SpvcCompilerRenameEntryPoint(SpvcCompiler compiler, byte* oldName, byte* newName, SpvExecutionModel model)
        {
            return spvcCompilerRenameEntryPoint(compiler, oldName, newName, model);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerGetCleansedEntryPointName(SpvcCompiler compiler, byte* name, SpvExecutionModel model);

        private static readonly PFNSpvcCompilerGetCleansedEntryPointName spvcCompilerGetCleansedEntryPointName = LoadFunction<PFNSpvcCompilerGetCleansedEntryPointName>("spvc_compiler_get_cleansed_entry_point_name");

        public static byte* SpvcCompilerGetCleansedEntryPointName(SpvcCompiler compiler, byte* name, SpvExecutionModel model)
        {
            return spvcCompilerGetCleansedEntryPointName(compiler, name, model);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode);

        private static readonly PFNSpvcCompilerSetExecutionMode spvcCompilerSetExecutionMode = LoadFunction<PFNSpvcCompilerSetExecutionMode>("spvc_compiler_set_execution_mode");

        public static void SpvcCompilerSetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode)
        {
            spvcCompilerSetExecutionMode(compiler, mode);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerUnsetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode);

        private static readonly PFNSpvcCompilerUnsetExecutionMode spvcCompilerUnsetExecutionMode = LoadFunction<PFNSpvcCompilerUnsetExecutionMode>("spvc_compiler_unset_execution_mode");

        public static void SpvcCompilerUnsetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode)
        {
            spvcCompilerUnsetExecutionMode(compiler, mode);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcCompilerSetExecutionModeWithArguments(SpvcCompiler compiler, SpvExecutionMode mode, uint arg0, uint arg1, uint arg2);

        private static readonly PFNSpvcCompilerSetExecutionModeWithArguments spvcCompilerSetExecutionModeWithArguments = LoadFunction<PFNSpvcCompilerSetExecutionModeWithArguments>("spvc_compiler_set_execution_mode_with_arguments");

        public static void SpvcCompilerSetExecutionModeWithArguments(SpvcCompiler compiler, SpvExecutionMode mode, uint arg0, uint arg1, uint arg2)
        {
            spvcCompilerSetExecutionModeWithArguments(compiler, mode, arg0, arg1, arg2);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetExecutionModes(SpvcCompiler compiler, SpvExecutionMode* modes, nuint* numModes);

        private static readonly PFNSpvcCompilerGetExecutionModes spvcCompilerGetExecutionModes = LoadFunction<PFNSpvcCompilerGetExecutionModes>("spvc_compiler_get_execution_modes");

        public static SpvcResult SpvcCompilerGetExecutionModes(SpvcCompiler compiler, SpvExecutionMode* modes, nuint* numModes)
        {
            return spvcCompilerGetExecutionModes(compiler, modes, numModes);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerGetExecutionModeArgument(SpvcCompiler compiler, SpvExecutionMode mode);

        private static readonly PFNSpvcCompilerGetExecutionModeArgument spvcCompilerGetExecutionModeArgument = LoadFunction<PFNSpvcCompilerGetExecutionModeArgument>("spvc_compiler_get_execution_mode_argument");

        public static uint SpvcCompilerGetExecutionModeArgument(SpvcCompiler compiler, SpvExecutionMode mode)
        {
            return spvcCompilerGetExecutionModeArgument(compiler, mode);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerGetExecutionModeArgumentByIndex(SpvcCompiler compiler, SpvExecutionMode mode, uint index);

        private static readonly PFNSpvcCompilerGetExecutionModeArgumentByIndex spvcCompilerGetExecutionModeArgumentByIndex = LoadFunction<PFNSpvcCompilerGetExecutionModeArgumentByIndex>("spvc_compiler_get_execution_mode_argument_by_index");

        public static uint SpvcCompilerGetExecutionModeArgumentByIndex(SpvcCompiler compiler, SpvExecutionMode mode, uint index)
        {
            return spvcCompilerGetExecutionModeArgumentByIndex(compiler, mode, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvExecutionModel PFNSpvcCompilerGetExecutionModel(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerGetExecutionModel spvcCompilerGetExecutionModel = LoadFunction<PFNSpvcCompilerGetExecutionModel>("spvc_compiler_get_execution_model");

        public static SpvExecutionModel SpvcCompilerGetExecutionModel(SpvcCompiler compiler)
        {
            return spvcCompilerGetExecutionModel(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcType PFNSpvcCompilerGetTypeHandle(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerGetTypeHandle spvcCompilerGetTypeHandle = LoadFunction<PFNSpvcCompilerGetTypeHandle>("spvc_compiler_get_type_handle");

        public static SpvcType SpvcCompilerGetTypeHandle(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerGetTypeHandle(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetBaseTypeId(SpvcType type);

        private static readonly PFNSpvcTypeGetBaseTypeId spvcTypeGetBaseTypeId = LoadFunction<PFNSpvcTypeGetBaseTypeId>("spvc_type_get_base_type_id");

        public static uint SpvcTypeGetBaseTypeId(SpvcType type)
        {
            return spvcTypeGetBaseTypeId(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcBasetype PFNSpvcTypeGetBasetype(SpvcType type);

        private static readonly PFNSpvcTypeGetBasetype spvcTypeGetBasetype = LoadFunction<PFNSpvcTypeGetBasetype>("spvc_type_get_basetype");

        public static SpvcBasetype SpvcTypeGetBasetype(SpvcType type)
        {
            return spvcTypeGetBasetype(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetBitWidth(SpvcType type);

        private static readonly PFNSpvcTypeGetBitWidth spvcTypeGetBitWidth = LoadFunction<PFNSpvcTypeGetBitWidth>("spvc_type_get_bit_width");

        public static uint SpvcTypeGetBitWidth(SpvcType type)
        {
            return spvcTypeGetBitWidth(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetVectorSize(SpvcType type);

        private static readonly PFNSpvcTypeGetVectorSize spvcTypeGetVectorSize = LoadFunction<PFNSpvcTypeGetVectorSize>("spvc_type_get_vector_size");

        public static uint SpvcTypeGetVectorSize(SpvcType type)
        {
            return spvcTypeGetVectorSize(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetColumns(SpvcType type);

        private static readonly PFNSpvcTypeGetColumns spvcTypeGetColumns = LoadFunction<PFNSpvcTypeGetColumns>("spvc_type_get_columns");

        public static uint SpvcTypeGetColumns(SpvcType type)
        {
            return spvcTypeGetColumns(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetNumArrayDimensions(SpvcType type);

        private static readonly PFNSpvcTypeGetNumArrayDimensions spvcTypeGetNumArrayDimensions = LoadFunction<PFNSpvcTypeGetNumArrayDimensions>("spvc_type_get_num_array_dimensions");

        public static uint SpvcTypeGetNumArrayDimensions(SpvcType type)
        {
            return spvcTypeGetNumArrayDimensions(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcTypeArrayDimensionIsLiteral(SpvcType type, uint dimension);

        private static readonly PFNSpvcTypeArrayDimensionIsLiteral spvcTypeArrayDimensionIsLiteral = LoadFunction<PFNSpvcTypeArrayDimensionIsLiteral>("spvc_type_array_dimension_is_literal");

        public static bool SpvcTypeArrayDimensionIsLiteral(SpvcType type, uint dimension)
        {
            return spvcTypeArrayDimensionIsLiteral(type, dimension);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvId PFNSpvcTypeGetArrayDimension(SpvcType type, uint dimension);

        private static readonly PFNSpvcTypeGetArrayDimension spvcTypeGetArrayDimension = LoadFunction<PFNSpvcTypeGetArrayDimension>("spvc_type_get_array_dimension");

        public static SpvId SpvcTypeGetArrayDimension(SpvcType type, uint dimension)
        {
            return spvcTypeGetArrayDimension(type, dimension);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetNumMemberTypes(SpvcType type);

        private static readonly PFNSpvcTypeGetNumMemberTypes spvcTypeGetNumMemberTypes = LoadFunction<PFNSpvcTypeGetNumMemberTypes>("spvc_type_get_num_member_types");

        public static uint SpvcTypeGetNumMemberTypes(SpvcType type)
        {
            return spvcTypeGetNumMemberTypes(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetMemberType(SpvcType type, uint index);

        private static readonly PFNSpvcTypeGetMemberType spvcTypeGetMemberType = LoadFunction<PFNSpvcTypeGetMemberType>("spvc_type_get_member_type");

        public static uint SpvcTypeGetMemberType(SpvcType type, uint index)
        {
            return spvcTypeGetMemberType(type, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvStorageClass PFNSpvcTypeGetStorageClass(SpvcType type);

        private static readonly PFNSpvcTypeGetStorageClass spvcTypeGetStorageClass = LoadFunction<PFNSpvcTypeGetStorageClass>("spvc_type_get_storage_class");

        public static SpvStorageClass SpvcTypeGetStorageClass(SpvcType type)
        {
            return spvcTypeGetStorageClass(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcTypeGetImageSampledType(SpvcType type);

        private static readonly PFNSpvcTypeGetImageSampledType spvcTypeGetImageSampledType = LoadFunction<PFNSpvcTypeGetImageSampledType>("spvc_type_get_image_sampled_type");

        public static uint SpvcTypeGetImageSampledType(SpvcType type)
        {
            return spvcTypeGetImageSampledType(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvDim PFNSpvcTypeGetImageDimension(SpvcType type);

        private static readonly PFNSpvcTypeGetImageDimension spvcTypeGetImageDimension = LoadFunction<PFNSpvcTypeGetImageDimension>("spvc_type_get_image_dimension");

        public static SpvDim SpvcTypeGetImageDimension(SpvcType type)
        {
            return spvcTypeGetImageDimension(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcTypeGetImageIsDepth(SpvcType type);

        private static readonly PFNSpvcTypeGetImageIsDepth spvcTypeGetImageIsDepth = LoadFunction<PFNSpvcTypeGetImageIsDepth>("spvc_type_get_image_is_depth");

        public static bool SpvcTypeGetImageIsDepth(SpvcType type)
        {
            return spvcTypeGetImageIsDepth(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcTypeGetImageArrayed(SpvcType type);

        private static readonly PFNSpvcTypeGetImageArrayed spvcTypeGetImageArrayed = LoadFunction<PFNSpvcTypeGetImageArrayed>("spvc_type_get_image_arrayed");

        public static bool SpvcTypeGetImageArrayed(SpvcType type)
        {
            return spvcTypeGetImageArrayed(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcTypeGetImageMultisampled(SpvcType type);

        private static readonly PFNSpvcTypeGetImageMultisampled spvcTypeGetImageMultisampled = LoadFunction<PFNSpvcTypeGetImageMultisampled>("spvc_type_get_image_multisampled");

        public static bool SpvcTypeGetImageMultisampled(SpvcType type)
        {
            return spvcTypeGetImageMultisampled(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcTypeGetImageIsStorage(SpvcType type);

        private static readonly PFNSpvcTypeGetImageIsStorage spvcTypeGetImageIsStorage = LoadFunction<PFNSpvcTypeGetImageIsStorage>("spvc_type_get_image_is_storage");

        public static bool SpvcTypeGetImageIsStorage(SpvcType type)
        {
            return spvcTypeGetImageIsStorage(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvImageFormat PFNSpvcTypeGetImageStorageFormat(SpvcType type);

        private static readonly PFNSpvcTypeGetImageStorageFormat spvcTypeGetImageStorageFormat = LoadFunction<PFNSpvcTypeGetImageStorageFormat>("spvc_type_get_image_storage_format");

        public static SpvImageFormat SpvcTypeGetImageStorageFormat(SpvcType type)
        {
            return spvcTypeGetImageStorageFormat(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvAccessQualifier PFNSpvcTypeGetImageAccessQualifier(SpvcType type);

        private static readonly PFNSpvcTypeGetImageAccessQualifier spvcTypeGetImageAccessQualifier = LoadFunction<PFNSpvcTypeGetImageAccessQualifier>("spvc_type_get_image_access_qualifier");

        public static SpvAccessQualifier SpvcTypeGetImageAccessQualifier(SpvcType type)
        {
            return spvcTypeGetImageAccessQualifier(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetDeclaredStructSize(SpvcCompiler compiler, SpvcType structType, nuint* size);

        private static readonly PFNSpvcCompilerGetDeclaredStructSize spvcCompilerGetDeclaredStructSize = LoadFunction<PFNSpvcCompilerGetDeclaredStructSize>("spvc_compiler_get_declared_struct_size");

        public static SpvcResult SpvcCompilerGetDeclaredStructSize(SpvcCompiler compiler, SpvcType structType, nuint* size)
        {
            return spvcCompilerGetDeclaredStructSize(compiler, structType, size);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetDeclaredStructSizeRuntimeArray(SpvcCompiler compiler, SpvcType structType, nuint arraySize, nuint* size);

        private static readonly PFNSpvcCompilerGetDeclaredStructSizeRuntimeArray spvcCompilerGetDeclaredStructSizeRuntimeArray = LoadFunction<PFNSpvcCompilerGetDeclaredStructSizeRuntimeArray>("spvc_compiler_get_declared_struct_size_runtime_array");

        public static SpvcResult SpvcCompilerGetDeclaredStructSizeRuntimeArray(SpvcCompiler compiler, SpvcType structType, nuint arraySize, nuint* size)
        {
            return spvcCompilerGetDeclaredStructSizeRuntimeArray(compiler, structType, arraySize, size);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetDeclaredStructMemberSize(SpvcCompiler compiler, SpvcType type, uint index, nuint* size);

        private static readonly PFNSpvcCompilerGetDeclaredStructMemberSize spvcCompilerGetDeclaredStructMemberSize = LoadFunction<PFNSpvcCompilerGetDeclaredStructMemberSize>("spvc_compiler_get_declared_struct_member_size");

        public static SpvcResult SpvcCompilerGetDeclaredStructMemberSize(SpvcCompiler compiler, SpvcType type, uint index, nuint* size)
        {
            return spvcCompilerGetDeclaredStructMemberSize(compiler, type, index, size);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerTypeStructMemberOffset(SpvcCompiler compiler, SpvcType type, uint index, uint* offset);

        private static readonly PFNSpvcCompilerTypeStructMemberOffset spvcCompilerTypeStructMemberOffset = LoadFunction<PFNSpvcCompilerTypeStructMemberOffset>("spvc_compiler_type_struct_member_offset");

        public static SpvcResult SpvcCompilerTypeStructMemberOffset(SpvcCompiler compiler, SpvcType type, uint index, uint* offset)
        {
            return spvcCompilerTypeStructMemberOffset(compiler, type, index, offset);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerTypeStructMemberArrayStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride);

        private static readonly PFNSpvcCompilerTypeStructMemberArrayStride spvcCompilerTypeStructMemberArrayStride = LoadFunction<PFNSpvcCompilerTypeStructMemberArrayStride>("spvc_compiler_type_struct_member_array_stride");

        public static SpvcResult SpvcCompilerTypeStructMemberArrayStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride)
        {
            return spvcCompilerTypeStructMemberArrayStride(compiler, type, index, stride);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerTypeStructMemberMatrixStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride);

        private static readonly PFNSpvcCompilerTypeStructMemberMatrixStride spvcCompilerTypeStructMemberMatrixStride = LoadFunction<PFNSpvcCompilerTypeStructMemberMatrixStride>("spvc_compiler_type_struct_member_matrix_stride");

        public static SpvcResult SpvcCompilerTypeStructMemberMatrixStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride)
        {
            return spvcCompilerTypeStructMemberMatrixStride(compiler, type, index, stride);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerBuildDummySamplerForCombinedImages(SpvcCompiler compiler, uint* id);

        private static readonly PFNSpvcCompilerBuildDummySamplerForCombinedImages spvcCompilerBuildDummySamplerForCombinedImages = LoadFunction<PFNSpvcCompilerBuildDummySamplerForCombinedImages>("spvc_compiler_build_dummy_sampler_for_combined_images");

        public static SpvcResult SpvcCompilerBuildDummySamplerForCombinedImages(SpvcCompiler compiler, uint* id)
        {
            return spvcCompilerBuildDummySamplerForCombinedImages(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerBuildCombinedImageSamplers(SpvcCompiler compiler);

        private static readonly PFNSpvcCompilerBuildCombinedImageSamplers spvcCompilerBuildCombinedImageSamplers = LoadFunction<PFNSpvcCompilerBuildCombinedImageSamplers>("spvc_compiler_build_combined_image_samplers");

        public static SpvcResult SpvcCompilerBuildCombinedImageSamplers(SpvcCompiler compiler)
        {
            return spvcCompilerBuildCombinedImageSamplers(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetCombinedImageSamplers(SpvcCompiler compiler, SpvcCombinedImageSampler* samplers, nuint* numSamplers);

        private static readonly PFNSpvcCompilerGetCombinedImageSamplers spvcCompilerGetCombinedImageSamplers = LoadFunction<PFNSpvcCompilerGetCombinedImageSamplers>("spvc_compiler_get_combined_image_samplers");

        public static SpvcResult SpvcCompilerGetCombinedImageSamplers(SpvcCompiler compiler, SpvcCombinedImageSampler* samplers, nuint* numSamplers)
        {
            return spvcCompilerGetCombinedImageSamplers(compiler, samplers, numSamplers);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* constants, nuint* numConstants);

        private static readonly PFNSpvcCompilerGetSpecializationConstants spvcCompilerGetSpecializationConstants = LoadFunction<PFNSpvcCompilerGetSpecializationConstants>("spvc_compiler_get_specialization_constants");

        public static SpvcResult SpvcCompilerGetSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* constants, nuint* numConstants)
        {
            return spvcCompilerGetSpecializationConstants(compiler, constants, numConstants);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcConstant PFNSpvcCompilerGetConstantHandle(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerGetConstantHandle spvcCompilerGetConstantHandle = LoadFunction<PFNSpvcCompilerGetConstantHandle>("spvc_compiler_get_constant_handle");

        public static SpvcConstant SpvcCompilerGetConstantHandle(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerGetConstantHandle(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcCompilerGetWorkGroupSizeSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* x, SpvcSpecializationConstant* y, SpvcSpecializationConstant* z);

        private static readonly PFNSpvcCompilerGetWorkGroupSizeSpecializationConstants spvcCompilerGetWorkGroupSizeSpecializationConstants = LoadFunction<PFNSpvcCompilerGetWorkGroupSizeSpecializationConstants>("spvc_compiler_get_work_group_size_specialization_constants");

        public static uint SpvcCompilerGetWorkGroupSizeSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* x, SpvcSpecializationConstant* y, SpvcSpecializationConstant* z)
        {
            return spvcCompilerGetWorkGroupSizeSpecializationConstants(compiler, x, y, z);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetActiveBufferRanges(SpvcCompiler compiler, uint id, SpvcBufferRange* ranges, nuint* numRanges);

        private static readonly PFNSpvcCompilerGetActiveBufferRanges spvcCompilerGetActiveBufferRanges = LoadFunction<PFNSpvcCompilerGetActiveBufferRanges>("spvc_compiler_get_active_buffer_ranges");

        public static SpvcResult SpvcCompilerGetActiveBufferRanges(SpvcCompiler compiler, uint id, SpvcBufferRange* ranges, nuint* numRanges)
        {
            return spvcCompilerGetActiveBufferRanges(compiler, id, ranges, numRanges);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float PFNSpvcConstantGetScalarFp16(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarFp16 spvcConstantGetScalarFp16 = LoadFunction<PFNSpvcConstantGetScalarFp16>(nameof(spvcConstantGetScalarFp16));

        public static float SpvcConstantGetScalarFp16(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarFp16(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float PFNSpvcConstantGetScalarFp32(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarFp32 spvcConstantGetScalarFp32 = LoadFunction<PFNSpvcConstantGetScalarFp32>(nameof(spvcConstantGetScalarFp32));

        public static float SpvcConstantGetScalarFp32(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarFp32(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate double PFNSpvcConstantGetScalarFp64(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarFp64 spvcConstantGetScalarFp64 = LoadFunction<PFNSpvcConstantGetScalarFp64>(nameof(spvcConstantGetScalarFp64));

        public static double SpvcConstantGetScalarFp64(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarFp64(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcConstantGetScalarU32(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarU32 spvcConstantGetScalarU32 = LoadFunction<PFNSpvcConstantGetScalarU32>(nameof(spvcConstantGetScalarU32));

        public static uint SpvcConstantGetScalarU32(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarU32(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PFNSpvcConstantGetScalarI32(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarI32 spvcConstantGetScalarI32 = LoadFunction<PFNSpvcConstantGetScalarI32>(nameof(spvcConstantGetScalarI32));

        public static int SpvcConstantGetScalarI32(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarI32(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcConstantGetScalarU16(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarU16 spvcConstantGetScalarU16 = LoadFunction<PFNSpvcConstantGetScalarU16>(nameof(spvcConstantGetScalarU16));

        public static uint SpvcConstantGetScalarU16(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarU16(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PFNSpvcConstantGetScalarI16(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarI16 spvcConstantGetScalarI16 = LoadFunction<PFNSpvcConstantGetScalarI16>(nameof(spvcConstantGetScalarI16));

        public static int SpvcConstantGetScalarI16(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarI16(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcConstantGetScalarU8(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarU8 spvcConstantGetScalarU8 = LoadFunction<PFNSpvcConstantGetScalarU8>(nameof(spvcConstantGetScalarU8));

        public static uint SpvcConstantGetScalarU8(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarU8(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PFNSpvcConstantGetScalarI8(SpvcConstant constant, uint column, uint row);

        private static readonly PFNSpvcConstantGetScalarI8 spvcConstantGetScalarI8 = LoadFunction<PFNSpvcConstantGetScalarI8>(nameof(spvcConstantGetScalarI8));

        public static int SpvcConstantGetScalarI8(SpvcConstant constant, uint column, uint row)
        {
            return spvcConstantGetScalarI8(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNSpvcConstantGetSubconstants(SpvcConstant constant, uint* constituents, nuint* count);

        private static readonly PFNSpvcConstantGetSubconstants spvcConstantGetSubconstants = LoadFunction<PFNSpvcConstantGetSubconstants>("spvc_constant_get_subconstants");

        public static void SpvcConstantGetSubconstants(SpvcConstant constant, uint* constituents, nuint* count)
        {
            spvcConstantGetSubconstants(constant, constituents, count);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFNSpvcConstantGetType(SpvcConstant constant);

        private static readonly PFNSpvcConstantGetType spvcConstantGetType = LoadFunction<PFNSpvcConstantGetType>("spvc_constant_get_type");

        public static uint SpvcConstantGetType(SpvcConstant constant)
        {
            return spvcConstantGetType(constant);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerGetBinaryOffsetForDecoration(SpvcCompiler compiler, uint id, SpvDecoration decoration, uint* wordOffset);

        private static readonly PFNSpvcCompilerGetBinaryOffsetForDecoration spvcCompilerGetBinaryOffsetForDecoration = LoadFunction<PFNSpvcCompilerGetBinaryOffsetForDecoration>("spvc_compiler_get_binary_offset_for_decoration");

        public static bool SpvcCompilerGetBinaryOffsetForDecoration(SpvcCompiler compiler, uint id, SpvDecoration decoration, uint* wordOffset)
        {
            return spvcCompilerGetBinaryOffsetForDecoration(compiler, id, decoration, wordOffset);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerBufferIsHlslCounterBuffer(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerBufferIsHlslCounterBuffer spvcCompilerBufferIsHlslCounterBuffer = LoadFunction<PFNSpvcCompilerBufferIsHlslCounterBuffer>("spvc_compiler_buffer_is_hlsl_counter_buffer");

        public static bool SpvcCompilerBufferIsHlslCounterBuffer(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerBufferIsHlslCounterBuffer(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFNSpvcCompilerBufferGetHlslCounterBuffer(SpvcCompiler compiler, uint id, uint* counterId);

        private static readonly PFNSpvcCompilerBufferGetHlslCounterBuffer spvcCompilerBufferGetHlslCounterBuffer = LoadFunction<PFNSpvcCompilerBufferGetHlslCounterBuffer>("spvc_compiler_buffer_get_hlsl_counter_buffer");

        public static bool SpvcCompilerBufferGetHlslCounterBuffer(SpvcCompiler compiler, uint id, uint* counterId)
        {
            return spvcCompilerBufferGetHlslCounterBuffer(compiler, id, counterId);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetDeclaredCapabilities(SpvcCompiler compiler, SpvCapability* capabilities, nuint* numCapabilities);

        private static readonly PFNSpvcCompilerGetDeclaredCapabilities spvcCompilerGetDeclaredCapabilities = LoadFunction<PFNSpvcCompilerGetDeclaredCapabilities>("spvc_compiler_get_declared_capabilities");

        public static SpvcResult SpvcCompilerGetDeclaredCapabilities(SpvcCompiler compiler, SpvCapability* capabilities, nuint* numCapabilities)
        {
            return spvcCompilerGetDeclaredCapabilities(compiler, capabilities, numCapabilities);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetDeclaredExtensions(SpvcCompiler compiler, byte* extensions, nuint* numExtensions);

        private static readonly PFNSpvcCompilerGetDeclaredExtensions spvcCompilerGetDeclaredExtensions = LoadFunction<PFNSpvcCompilerGetDeclaredExtensions>("spvc_compiler_get_declared_extensions");

        public static SpvcResult SpvcCompilerGetDeclaredExtensions(SpvcCompiler compiler, byte* extensions, nuint* numExtensions)
        {
            return spvcCompilerGetDeclaredExtensions(compiler, extensions, numExtensions);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFNSpvcCompilerGetRemappedDeclaredBlockName(SpvcCompiler compiler, uint id);

        private static readonly PFNSpvcCompilerGetRemappedDeclaredBlockName spvcCompilerGetRemappedDeclaredBlockName = LoadFunction<PFNSpvcCompilerGetRemappedDeclaredBlockName>("spvc_compiler_get_remapped_declared_block_name");

        public static byte* SpvcCompilerGetRemappedDeclaredBlockName(SpvcCompiler compiler, uint id)
        {
            return spvcCompilerGetRemappedDeclaredBlockName(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFNSpvcCompilerGetBufferBlockDecorations(SpvcCompiler compiler, uint id, SpvDecoration* decorations, nuint* numDecorations);

        private static readonly PFNSpvcCompilerGetBufferBlockDecorations spvcCompilerGetBufferBlockDecorations = LoadFunction<PFNSpvcCompilerGetBufferBlockDecorations>("spvc_compiler_get_buffer_block_decorations");

        public static SpvcResult SpvcCompilerGetBufferBlockDecorations(SpvcCompiler compiler, uint id, SpvDecoration* decorations, nuint* numDecorations)
        {
            return spvcCompilerGetBufferBlockDecorations(compiler, id, decorations, numDecorations);
        }
    }
}