using System;
using System.Runtime.InteropServices;

namespace SPIRVCross
{
    public unsafe partial class SPIRV
    {
        internal static IntPtr s_NativeLibrary = LoadNativeLibrary();

        internal static T LoadFunction<T>(string name) => LibraryLoader.LoadFunction<T>(s_NativeLibrary, name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_get_version(uint* major, uint* minor, uint* patch);

        private static readonly PFN_spvc_get_version spvc_get_version_ = LoadFunction<PFN_spvc_get_version>("spvc_get_version");

        public static void SpvcGetVersion(uint* major, uint* minor, uint* patch)
        {
            spvc_get_version_(major, minor, patch);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_get_commit_revision_and_timestamp();

        private static readonly PFN_spvc_get_commit_revision_and_timestamp spvc_get_commit_revision_and_timestamp_ = LoadFunction<PFN_spvc_get_commit_revision_and_timestamp>("spvc_get_commit_revision_and_timestamp");

        public static byte* SpvcGetCommitRevisionAndTimestamp()
        {
            return spvc_get_commit_revision_and_timestamp_();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_msl_vertex_attribute_init(SpvcMslVertexAttribute* attr);

        private static readonly PFN_spvc_msl_vertex_attribute_init spvc_msl_vertex_attribute_init_ = LoadFunction<PFN_spvc_msl_vertex_attribute_init>("spvc_msl_vertex_attribute_init");

        public static void SpvcMslVertexAttributeInit(SpvcMslVertexAttribute* attr)
        {
            spvc_msl_vertex_attribute_init_(attr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_msl_shader_input_init(SpvcMslShaderInput* input);

        private static readonly PFN_spvc_msl_shader_input_init spvc_msl_shader_input_init_ = LoadFunction<PFN_spvc_msl_shader_input_init>("spvc_msl_shader_input_init");

        public static void spvcMslShaderInputInit(SpvcMslShaderInput* input)
        {
            spvc_msl_shader_input_init_(input);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_msl_resource_binding_init(SpvcMslResourceBinding* binding);

        private static readonly PFN_spvc_msl_resource_binding_init spvc_msl_resource_binding_init_ = LoadFunction<PFN_spvc_msl_resource_binding_init>("spvc_msl_resource_binding_init");

        public static void spvcMslResourceBindingInit(SpvcMslResourceBinding* binding)
        {
            spvc_msl_resource_binding_init_(binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_msl_get_aux_buffer_struct_version();

        private static readonly PFN_spvc_msl_get_aux_buffer_struct_version spvc_msl_get_aux_buffer_struct_version_ = LoadFunction<PFN_spvc_msl_get_aux_buffer_struct_version>("spvc_msl_get_aux_buffer_struct_version");

        public static uint spvcMslGetAuxBufferStructVersion()
        {
            return spvc_msl_get_aux_buffer_struct_version_();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_msl_constexpr_sampler_init(SpvcMslConstexprSampler* sampler);

        private static readonly PFN_spvc_msl_constexpr_sampler_init spvc_msl_constexpr_sampler_init_ = LoadFunction<PFN_spvc_msl_constexpr_sampler_init>("spvc_msl_constexpr_sampler_init");

        public static void spvcMslConstexprSamplerInit(SpvcMslConstexprSampler* sampler)
        {
            spvc_msl_constexpr_sampler_init_(sampler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_msl_sampler_ycbcr_conversion_init(SpvcMslSamplerYcbcrConversion* conv);

        private static readonly PFN_spvc_msl_sampler_ycbcr_conversion_init spvc_msl_sampler_ycbcr_conversion_init_ = LoadFunction<PFN_spvc_msl_sampler_ycbcr_conversion_init>("spvc_msl_sampler_ycbcr_conversion_init");

        public static void spvcMslSamplerYcbcrConversionInit(SpvcMslSamplerYcbcrConversion* conv)
        {
            spvc_msl_sampler_ycbcr_conversion_init_(conv);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_hlsl_resource_binding_init(SpvcHlslResourceBinding* binding);

        private static readonly PFN_spvc_hlsl_resource_binding_init spvc_hlsl_resource_binding_init_ = LoadFunction<PFN_spvc_hlsl_resource_binding_init>("spvc_hlsl_resource_binding_init");

        public static void spvcHlslResourceBindingInit(SpvcHlslResourceBinding* binding)
        {
            spvc_hlsl_resource_binding_init_(binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_context_create(SpvcContext* context);

        private static readonly PFN_spvc_context_create spvc_context_create_ = LoadFunction<PFN_spvc_context_create>("spvc_context_create");

        public static SpvcResult SpvcContextCreate(SpvcContext* context)
        {
            return spvc_context_create_(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_context_destroy(SpvcContext context);

        private static readonly PFN_spvc_context_destroy spvc_context_destroy_ = LoadFunction<PFN_spvc_context_destroy>("spvc_context_destroy");

        public static void SpvcContextDestroy(SpvcContext context)
        {
            spvc_context_destroy_(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_context_release_allocations(SpvcContext context);

        private static readonly PFN_spvc_context_release_allocations spvc_context_release_allocations_ = LoadFunction<PFN_spvc_context_release_allocations>("spvc_context_release_allocations");

        public static void spvcContextReleaseAllocations(SpvcContext context)
        {
            spvc_context_release_allocations_(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_context_get_last_error_string(SpvcContext context);

        private static readonly PFN_spvc_context_get_last_error_string spvc_context_get_last_error_string_ = LoadFunction<PFN_spvc_context_get_last_error_string>("spvc_context_get_last_error_string");

        public static byte* spvcContextGetLastErrorString(SpvcContext context)
        {
            return spvc_context_get_last_error_string_(context);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_context_set_error_callback(SpvcContext context, SpvcErrorCallback cb, void* userdata);

        private static readonly PFN_spvc_context_set_error_callback spvc_context_set_error_callback_ = LoadFunction<PFN_spvc_context_set_error_callback>("spvc_context_set_error_callback");

        public static void SpvcContextSetErrorCallback(SpvcContext context, SpvcErrorCallback cb, void* userdata)
        {
            spvc_context_set_error_callback_(context, cb, userdata);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_context_parse_spirv(SpvcContext context, SpvId* spirv, nuint word_count, SpvcParsedIr* parsed_ir);

        private static readonly PFN_spvc_context_parse_spirv spvc_context_parse_spirv_ = LoadFunction<PFN_spvc_context_parse_spirv>("spvc_context_parse_spirv");

        public static SpvcResult SpvcContextParseSpirv(SpvcContext context, SpvId* spirv, nuint wordCount, SpvcParsedIr* parsedIr)
        {
            return spvc_context_parse_spirv_(context, spirv, wordCount, parsedIr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_context_create_compiler(SpvcContext context, SpvcBackend backend, SpvcParsedIr parsed_ir, SpvcCaptureMode mode, SpvcCompiler* compiler);

        private static readonly PFN_spvc_context_create_compiler spvc_context_create_compiler_ = LoadFunction<PFN_spvc_context_create_compiler>("spvc_context_create_compiler");

        public static SpvcResult SpvcContextCreateCompiler(SpvcContext context, SpvcBackend backend, SpvcParsedIr parsedIr, SpvcCaptureMode mode, SpvcCompiler* compiler)
        {
            return spvc_context_create_compiler_(context, backend, parsedIr, mode, compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_get_current_id_bound(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_get_current_id_bound spvc_compiler_get_current_id_bound_ = LoadFunction<PFN_spvc_compiler_get_current_id_bound>("spvc_compiler_get_current_id_bound");

        public static uint spvcCompilerGetCurrentIdBound(SpvcCompiler compiler)
        {
            return spvc_compiler_get_current_id_bound_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_create_compiler_options(SpvcCompiler compiler, SpvcCompilerOptions* options);

        private static readonly PFN_spvc_compiler_create_compiler_options spvc_compiler_create_compiler_options_ = LoadFunction<PFN_spvc_compiler_create_compiler_options>("spvc_compiler_create_compiler_options");

        public static SpvcResult SpvcCompilerCreateCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions* options)
        {
            return spvc_compiler_create_compiler_options_(compiler, options);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_options_set_bool(SpvcCompilerOptions options, SpvcCompilerOption option, bool value);

        private static readonly PFN_spvc_compiler_options_set_bool spvc_compiler_options_set_bool_ = LoadFunction<PFN_spvc_compiler_options_set_bool>("spvc_compiler_options_set_bool");

        public static SpvcResult SpvcCompilerOptionsSetBool(SpvcCompilerOptions options, SpvcCompilerOption option, bool value)
        {
            return spvc_compiler_options_set_bool_(options, option, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_options_set_uint(SpvcCompilerOptions options, SpvcCompilerOption option, uint value);

        private static readonly PFN_spvc_compiler_options_set_uint spvc_compiler_options_set_uint_ = LoadFunction<PFN_spvc_compiler_options_set_uint>("spvc_compiler_options_set_uint");

        public static SpvcResult SpvcCompilerOptionsSetUint(SpvcCompilerOptions options, SpvcCompilerOption option, uint value)
        {
            return spvc_compiler_options_set_uint_(options, option, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_install_compiler_options(SpvcCompiler compiler, SpvcCompilerOptions options);

        private static readonly PFN_spvc_compiler_install_compiler_options spvc_compiler_install_compiler_options_ = LoadFunction<PFN_spvc_compiler_install_compiler_options>("spvc_compiler_install_compiler_options");

        public static SpvcResult SpvcCompilerInstallCompilerOptions(SpvcCompiler compiler, SpvcCompilerOptions options)
        {
            return spvc_compiler_install_compiler_options_(compiler, options);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_compile(SpvcCompiler compiler, byte* source);

        private static readonly PFN_spvc_compiler_compile spvc_compiler_compile_ = LoadFunction<PFN_spvc_compiler_compile>("spvc_compiler_compile");

        public static SpvcResult SpvcCompilerCompile(SpvcCompiler compiler, byte* source)
        {
            return spvc_compiler_compile_(compiler, source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_add_header_line(SpvcCompiler compiler, byte* line);

        private static readonly PFN_spvc_compiler_add_header_line spvc_compiler_add_header_line_ = LoadFunction<PFN_spvc_compiler_add_header_line>("spvc_compiler_add_header_line");

        public static SpvcResult spvcCompilerAddHeaderLine(SpvcCompiler compiler, byte* line)
        {
            return spvc_compiler_add_header_line_(compiler, line);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_require_extension(SpvcCompiler compiler, byte* ext);

        private static readonly PFN_spvc_compiler_require_extension spvc_compiler_require_extension_ = LoadFunction<PFN_spvc_compiler_require_extension>("spvc_compiler_require_extension");

        public static SpvcResult spvcCompilerRequireExtension(SpvcCompiler compiler, byte* ext)
        {
            return spvc_compiler_require_extension_(compiler, ext);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_flatten_buffer_block(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_flatten_buffer_block spvc_compiler_flatten_buffer_block_ = LoadFunction<PFN_spvc_compiler_flatten_buffer_block>("spvc_compiler_flatten_buffer_block");

        public static SpvcResult spvcCompilerFlattenBufferBlock(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_flatten_buffer_block_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_variable_is_depth_or_compare(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_variable_is_depth_or_compare spvc_compiler_variable_is_depth_or_compare_ = LoadFunction<PFN_spvc_compiler_variable_is_depth_or_compare>("spvc_compiler_variable_is_depth_or_compare");

        public static bool spvcCompilerVariableIsDepthOrCompare(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_variable_is_depth_or_compare_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_hlsl_set_root_constants_layout(SpvcCompiler compiler, SpvcHlslRootConstants* constant_info, nuint count);

        private static readonly PFN_spvc_compiler_hlsl_set_root_constants_layout spvc_compiler_hlsl_set_root_constants_layout_ = LoadFunction<PFN_spvc_compiler_hlsl_set_root_constants_layout>("spvc_compiler_hlsl_set_root_constants_layout");

        public static SpvcResult spvcCompilerHlslSetRootConstantsLayout(SpvcCompiler compiler, SpvcHlslRootConstants* constantInfo, nuint count)
        {
            return spvc_compiler_hlsl_set_root_constants_layout_(compiler, constantInfo, count);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_hlsl_add_vertex_attribute_remap(SpvcCompiler compiler, SpvcHlslVertexAttributeRemap* remap, nuint remaps);

        private static readonly PFN_spvc_compiler_hlsl_add_vertex_attribute_remap spvc_compiler_hlsl_add_vertex_attribute_remap_ = LoadFunction<PFN_spvc_compiler_hlsl_add_vertex_attribute_remap>("spvc_compiler_hlsl_add_vertex_attribute_remap");

        public static SpvcResult spvcCompilerHlslAddVertexAttributeRemap(SpvcCompiler compiler, SpvcHlslVertexAttributeRemap* remap, nuint remaps)
        {
            return spvc_compiler_hlsl_add_vertex_attribute_remap_(compiler, remap, remaps);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_hlsl_remap_num_workgroups_builtin(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_hlsl_remap_num_workgroups_builtin spvc_compiler_hlsl_remap_num_workgroups_builtin_ = LoadFunction<PFN_spvc_compiler_hlsl_remap_num_workgroups_builtin>("spvc_compiler_hlsl_remap_num_workgroups_builtin");

        public static uint spvcCompilerHlslRemapNumWorkgroupsBuiltin(SpvcCompiler compiler)
        {
            return spvc_compiler_hlsl_remap_num_workgroups_builtin_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_hlsl_set_resource_binding_flags(SpvcCompiler compiler, uint flags);

        private static readonly PFN_spvc_compiler_hlsl_set_resource_binding_flags spvc_compiler_hlsl_set_resource_binding_flags_ = LoadFunction<PFN_spvc_compiler_hlsl_set_resource_binding_flags>("spvc_compiler_hlsl_set_resource_binding_flags");

        public static SpvcResult spvcCompilerHlslSetResourceBindingFlags(SpvcCompiler compiler, uint flags)
        {
            return spvc_compiler_hlsl_set_resource_binding_flags_(compiler, flags);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_hlsl_add_resource_binding(SpvcCompiler compiler, SpvcHlslResourceBinding* binding);

        private static readonly PFN_spvc_compiler_hlsl_add_resource_binding spvc_compiler_hlsl_add_resource_binding_ = LoadFunction<PFN_spvc_compiler_hlsl_add_resource_binding>("spvc_compiler_hlsl_add_resource_binding");

        public static SpvcResult spvcCompilerHlslAddResourceBinding(SpvcCompiler compiler, SpvcHlslResourceBinding* binding)
        {
            return spvc_compiler_hlsl_add_resource_binding_(compiler, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_hlsl_is_resource_used(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding);

        private static readonly PFN_spvc_compiler_hlsl_is_resource_used spvc_compiler_hlsl_is_resource_used_ = LoadFunction<PFN_spvc_compiler_hlsl_is_resource_used>("spvc_compiler_hlsl_is_resource_used");

        public static bool spvcCompilerHlslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding)
        {
            return spvc_compiler_hlsl_is_resource_used_(compiler, model, set, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_is_rasterization_disabled(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_is_rasterization_disabled spvc_compiler_msl_is_rasterization_disabled_ = LoadFunction<PFN_spvc_compiler_msl_is_rasterization_disabled>("spvc_compiler_msl_is_rasterization_disabled");

        public static bool spvcCompilerMslIsRasterizationDisabled(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_is_rasterization_disabled_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_needs_aux_buffer(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_needs_aux_buffer spvc_compiler_msl_needs_aux_buffer_ = LoadFunction<PFN_spvc_compiler_msl_needs_aux_buffer>("spvc_compiler_msl_needs_aux_buffer");

        public static bool spvcCompilerMslNeedsAuxBuffer(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_needs_aux_buffer_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_needs_swizzle_buffer(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_needs_swizzle_buffer spvc_compiler_msl_needs_swizzle_buffer_ = LoadFunction<PFN_spvc_compiler_msl_needs_swizzle_buffer>("spvc_compiler_msl_needs_swizzle_buffer");

        public static bool spvcCompilerMslNeedsSwizzleBuffer(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_needs_swizzle_buffer_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_needs_buffer_size_buffer(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_needs_buffer_size_buffer spvc_compiler_msl_needs_buffer_size_buffer_ = LoadFunction<PFN_spvc_compiler_msl_needs_buffer_size_buffer>("spvc_compiler_msl_needs_buffer_size_buffer");

        public static bool spvcCompilerMslNeedsBufferSizeBuffer(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_needs_buffer_size_buffer_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_needs_output_buffer(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_needs_output_buffer spvc_compiler_msl_needs_output_buffer_ = LoadFunction<PFN_spvc_compiler_msl_needs_output_buffer>("spvc_compiler_msl_needs_output_buffer");

        public static bool spvcCompilerMslNeedsOutputBuffer(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_needs_output_buffer_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_needs_patch_output_buffer(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_needs_patch_output_buffer spvc_compiler_msl_needs_patch_output_buffer_ = LoadFunction<PFN_spvc_compiler_msl_needs_patch_output_buffer>("spvc_compiler_msl_needs_patch_output_buffer");

        public static bool spvcCompilerMslNeedsPatchOutputBuffer(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_needs_patch_output_buffer_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_needs_input_threadgroup_mem(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_needs_input_threadgroup_mem spvc_compiler_msl_needs_input_threadgroup_mem_ = LoadFunction<PFN_spvc_compiler_msl_needs_input_threadgroup_mem>("spvc_compiler_msl_needs_input_threadgroup_mem");

        public static bool spvcCompilerMslNeedsInputThreadgroupMem(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_needs_input_threadgroup_mem_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_add_vertex_attribute(SpvcCompiler compiler, SpvcMslVertexAttribute* attrs);

        private static readonly PFN_spvc_compiler_msl_add_vertex_attribute spvc_compiler_msl_add_vertex_attribute_ = LoadFunction<PFN_spvc_compiler_msl_add_vertex_attribute>("spvc_compiler_msl_add_vertex_attribute");

        public static SpvcResult spvcCompilerMslAddVertexAttribute(SpvcCompiler compiler, SpvcMslVertexAttribute* attrs)
        {
            return spvc_compiler_msl_add_vertex_attribute_(compiler, attrs);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_add_resource_binding(SpvcCompiler compiler, SpvcMslResourceBinding* binding);

        private static readonly PFN_spvc_compiler_msl_add_resource_binding spvc_compiler_msl_add_resource_binding_ = LoadFunction<PFN_spvc_compiler_msl_add_resource_binding>("spvc_compiler_msl_add_resource_binding");

        public static SpvcResult spvcCompilerMslAddResourceBinding(SpvcCompiler compiler, SpvcMslResourceBinding* binding)
        {
            return spvc_compiler_msl_add_resource_binding_(compiler, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_add_shader_input(SpvcCompiler compiler, SpvcMslShaderInput* input);

        private static readonly PFN_spvc_compiler_msl_add_shader_input spvc_compiler_msl_add_shader_input_ = LoadFunction<PFN_spvc_compiler_msl_add_shader_input>("spvc_compiler_msl_add_shader_input");

        public static SpvcResult spvcCompilerMslAddShaderInput(SpvcCompiler compiler, SpvcMslShaderInput* input)
        {
            return spvc_compiler_msl_add_shader_input_(compiler, input);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_add_discrete_descriptor_set(SpvcCompiler compiler, uint desc_set);

        private static readonly PFN_spvc_compiler_msl_add_discrete_descriptor_set spvc_compiler_msl_add_discrete_descriptor_set_ = LoadFunction<PFN_spvc_compiler_msl_add_discrete_descriptor_set>("spvc_compiler_msl_add_discrete_descriptor_set");

        public static SpvcResult spvcCompilerMslAddDiscreteDescriptorSet(SpvcCompiler compiler, uint descSet)
        {
            return spvc_compiler_msl_add_discrete_descriptor_set_(compiler, descSet);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_set_argument_buffer_device_address_space(SpvcCompiler compiler, uint desc_set, bool device_address);

        private static readonly PFN_spvc_compiler_msl_set_argument_buffer_device_address_space spvc_compiler_msl_set_argument_buffer_device_address_space_ = LoadFunction<PFN_spvc_compiler_msl_set_argument_buffer_device_address_space>("spvc_compiler_msl_set_argument_buffer_device_address_space");

        public static SpvcResult spvcCompilerMslSetArgumentBufferDeviceAddressSpace(SpvcCompiler compiler, uint descSet, bool deviceAddress)
        {
            return spvc_compiler_msl_set_argument_buffer_device_address_space_(compiler, descSet, deviceAddress);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_is_vertex_attribute_used(SpvcCompiler compiler, uint location);

        private static readonly PFN_spvc_compiler_msl_is_vertex_attribute_used spvc_compiler_msl_is_vertex_attribute_used_ = LoadFunction<PFN_spvc_compiler_msl_is_vertex_attribute_used>("spvc_compiler_msl_is_vertex_attribute_used");

        public static bool spvcCompilerMslIsVertexAttributeUsed(SpvcCompiler compiler, uint location)
        {
            return spvc_compiler_msl_is_vertex_attribute_used_(compiler, location);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_is_shader_input_used(SpvcCompiler compiler, uint location);

        private static readonly PFN_spvc_compiler_msl_is_shader_input_used spvc_compiler_msl_is_shader_input_used_ = LoadFunction<PFN_spvc_compiler_msl_is_shader_input_used>("spvc_compiler_msl_is_shader_input_used");

        public static bool spvcCompilerMslIsShaderInputUsed(SpvcCompiler compiler, uint location)
        {
            return spvc_compiler_msl_is_shader_input_used_(compiler, location);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_msl_is_resource_used(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding);

        private static readonly PFN_spvc_compiler_msl_is_resource_used spvc_compiler_msl_is_resource_used_ = LoadFunction<PFN_spvc_compiler_msl_is_resource_used>("spvc_compiler_msl_is_resource_used");

        public static bool spvcCompilerMslIsResourceUsed(SpvcCompiler compiler, SpvExecutionModel model, uint set, uint binding)
        {
            return spvc_compiler_msl_is_resource_used_(compiler, model, set, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_remap_constexpr_sampler(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler);

        private static readonly PFN_spvc_compiler_msl_remap_constexpr_sampler spvc_compiler_msl_remap_constexpr_sampler_ = LoadFunction<PFN_spvc_compiler_msl_remap_constexpr_sampler>("spvc_compiler_msl_remap_constexpr_sampler");

        public static SpvcResult spvcCompilerMslRemapConstexprSampler(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler)
        {
            return spvc_compiler_msl_remap_constexpr_sampler_(compiler, id, sampler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_remap_constexpr_sampler_by_binding(SpvcCompiler compiler, uint desc_set, uint binding, SpvcMslConstexprSampler* sampler);

        private static readonly PFN_spvc_compiler_msl_remap_constexpr_sampler_by_binding spvc_compiler_msl_remap_constexpr_sampler_by_binding_ = LoadFunction<PFN_spvc_compiler_msl_remap_constexpr_sampler_by_binding>("spvc_compiler_msl_remap_constexpr_sampler_by_binding");

        public static SpvcResult spvcCompilerMslRemapConstexprSamplerByBinding(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler)
        {
            return spvc_compiler_msl_remap_constexpr_sampler_by_binding_(compiler, descSet, binding, sampler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_remap_constexpr_sampler_ycbcr(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv);

        private static readonly PFN_spvc_compiler_msl_remap_constexpr_sampler_ycbcr spvc_compiler_msl_remap_constexpr_sampler_ycbcr_ = LoadFunction<PFN_spvc_compiler_msl_remap_constexpr_sampler_ycbcr>("spvc_compiler_msl_remap_constexpr_sampler_ycbcr");

        public static SpvcResult spvcCompilerMslRemapConstexprSamplerYcbcr(SpvcCompiler compiler, uint id, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv)
        {
            return spvc_compiler_msl_remap_constexpr_sampler_ycbcr_(compiler, id, sampler, conv);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr(SpvcCompiler compiler, uint desc_set, uint binding, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv);

        private static readonly PFN_spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr_ = LoadFunction<PFN_spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr>("spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr");

        public static SpvcResult spvcCompilerMslRemapConstexprSamplerByBindingYcbcr(SpvcCompiler compiler, uint descSet, uint binding, SpvcMslConstexprSampler* sampler, SpvcMslSamplerYcbcrConversion* conv)
        {
            return spvc_compiler_msl_remap_constexpr_sampler_by_binding_ycbcr_(compiler, descSet, binding, sampler, conv);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_set_fragment_output_components(SpvcCompiler compiler, uint location, uint components);

        private static readonly PFN_spvc_compiler_msl_set_fragment_output_components spvc_compiler_msl_set_fragment_output_components_ = LoadFunction<PFN_spvc_compiler_msl_set_fragment_output_components>("spvc_compiler_msl_set_fragment_output_components");

        public static SpvcResult spvcCompilerMslSetFragmentOutputComponents(SpvcCompiler compiler, uint location, uint components)
        {
            return spvc_compiler_msl_set_fragment_output_components_(compiler, location, components);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_msl_get_automatic_resource_binding(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_msl_get_automatic_resource_binding spvc_compiler_msl_get_automatic_resource_binding_ = LoadFunction<PFN_spvc_compiler_msl_get_automatic_resource_binding>("spvc_compiler_msl_get_automatic_resource_binding");

        public static uint spvcCompilerMslGetAutomaticResourceBinding(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_msl_get_automatic_resource_binding_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_msl_get_automatic_resource_binding_secondary(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_msl_get_automatic_resource_binding_secondary spvc_compiler_msl_get_automatic_resource_binding_secondary_ = LoadFunction<PFN_spvc_compiler_msl_get_automatic_resource_binding_secondary>("spvc_compiler_msl_get_automatic_resource_binding_secondary");

        public static uint spvcCompilerMslGetAutomaticResourceBindingSecondary(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_msl_get_automatic_resource_binding_secondary_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_add_dynamic_buffer(SpvcCompiler compiler, uint desc_set, uint binding, uint index);

        private static readonly PFN_spvc_compiler_msl_add_dynamic_buffer spvc_compiler_msl_add_dynamic_buffer_ = LoadFunction<PFN_spvc_compiler_msl_add_dynamic_buffer>("spvc_compiler_msl_add_dynamic_buffer");

        public static SpvcResult spvcCompilerMslAddDynamicBuffer(SpvcCompiler compiler, uint descSet, uint binding, uint index)
        {
            return spvc_compiler_msl_add_dynamic_buffer_(compiler, descSet, binding, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_add_inline_uniform_block(SpvcCompiler compiler, uint desc_set, uint binding);

        private static readonly PFN_spvc_compiler_msl_add_inline_uniform_block spvc_compiler_msl_add_inline_uniform_block_ = LoadFunction<PFN_spvc_compiler_msl_add_inline_uniform_block>("spvc_compiler_msl_add_inline_uniform_block");

        public static SpvcResult spvcCompilerMslAddInlineUniformBlock(SpvcCompiler compiler, uint descSet, uint binding)
        {
            return spvc_compiler_msl_add_inline_uniform_block_(compiler, descSet, binding);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_msl_set_combined_sampler_suffix(SpvcCompiler compiler, byte* suffix);

        private static readonly PFN_spvc_compiler_msl_set_combined_sampler_suffix spvc_compiler_msl_set_combined_sampler_suffix_ = LoadFunction<PFN_spvc_compiler_msl_set_combined_sampler_suffix>("spvc_compiler_msl_set_combined_sampler_suffix");

        public static SpvcResult spvcCompilerMslSetCombinedSamplerSuffix(SpvcCompiler compiler, byte* suffix)
        {
            return spvc_compiler_msl_set_combined_sampler_suffix_(compiler, suffix);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_msl_get_combined_sampler_suffix(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_msl_get_combined_sampler_suffix spvc_compiler_msl_get_combined_sampler_suffix_ = LoadFunction<PFN_spvc_compiler_msl_get_combined_sampler_suffix>("spvc_compiler_msl_get_combined_sampler_suffix");

        public static byte* spvcCompilerMslGetCombinedSamplerSuffix(SpvcCompiler compiler)
        {
            return spvc_compiler_msl_get_combined_sampler_suffix_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_active_interface_variables(SpvcCompiler compiler, SpvcSet* set);

        private static readonly PFN_spvc_compiler_get_active_interface_variables spvc_compiler_get_active_interface_variables_ = LoadFunction<PFN_spvc_compiler_get_active_interface_variables>("spvc_compiler_get_active_interface_variables");

        public static SpvcResult spvcCompilerGetActiveInterfaceVariables(SpvcCompiler compiler, SpvcSet* set)
        {
            return spvc_compiler_get_active_interface_variables_(compiler, set);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_set_enabled_interface_variables(SpvcCompiler compiler, SpvcSet set);

        private static readonly PFN_spvc_compiler_set_enabled_interface_variables spvc_compiler_set_enabled_interface_variables_ = LoadFunction<PFN_spvc_compiler_set_enabled_interface_variables>("spvc_compiler_set_enabled_interface_variables");

        public static SpvcResult spvcCompilerSetEnabledInterfaceVariables(SpvcCompiler compiler, SpvcSet set)
        {
            return spvc_compiler_set_enabled_interface_variables_(compiler, set);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_create_shader_resources(SpvcCompiler compiler, SpvcResources* resources);

        private static readonly PFN_spvc_compiler_create_shader_resources spvc_compiler_create_shader_resources_ = LoadFunction<PFN_spvc_compiler_create_shader_resources>("spvc_compiler_create_shader_resources");

        public static SpvcResult SpvcCompilerCreateShaderResources(SpvcCompiler compiler, SpvcResources* resources)
        {
            return spvc_compiler_create_shader_resources_(compiler, resources);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_create_shader_resources_for_active_variables(SpvcCompiler compiler, SpvcResources* resources, SpvcSet active);

        private static readonly PFN_spvc_compiler_create_shader_resources_for_active_variables spvc_compiler_create_shader_resources_for_active_variables_ = LoadFunction<PFN_spvc_compiler_create_shader_resources_for_active_variables>("spvc_compiler_create_shader_resources_for_active_variables");

        public static SpvcResult spvcCompilerCreateShaderResourcesForActiveVariables(SpvcCompiler compiler, SpvcResources* resources, SpvcSet active)
        {
            return spvc_compiler_create_shader_resources_for_active_variables_(compiler, resources, active);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_resources_get_resource_list_for_type(SpvcResources resources, SpvcResourceType type, SpvcReflectedResource* resource_list, nuint* resource_size);

        private static readonly PFN_spvc_resources_get_resource_list_for_type spvc_resources_get_resource_list_for_type_ = LoadFunction<PFN_spvc_resources_get_resource_list_for_type>("spvc_resources_get_resource_list_for_type");

        public static SpvcResult SpvcResourcesGetResourceListForType(SpvcResources resources, SpvcResourceType type, SpvcReflectedResource* resourceList, nuint* resourceSize)
        {
            return spvc_resources_get_resource_list_for_type_(resources, type, resourceList, resourceSize);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_decoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, uint argument);

        private static readonly PFN_spvc_compiler_set_decoration spvc_compiler_set_decoration_ = LoadFunction<PFN_spvc_compiler_set_decoration>("spvc_compiler_set_decoration");

        public static void spvcCompilerSetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, uint argument)
        {
            spvc_compiler_set_decoration_(compiler, id, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_decoration_string(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, byte* argument);

        private static readonly PFN_spvc_compiler_set_decoration_string spvc_compiler_set_decoration_string_ = LoadFunction<PFN_spvc_compiler_set_decoration_string>("spvc_compiler_set_decoration_string");

        public static void spvcCompilerSetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration, byte* argument)
        {
            spvc_compiler_set_decoration_string_(compiler, id, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_name(SpvcCompiler compiler, SpvId id, byte* argument);

        private static readonly PFN_spvc_compiler_set_name spvc_compiler_set_name_ = LoadFunction<PFN_spvc_compiler_set_name>("spvc_compiler_set_name");

        public static void spvcCompilerSetName(SpvcCompiler compiler, SpvId id, byte* argument)
        {
            spvc_compiler_set_name_(compiler, id, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_member_decoration(SpvcCompiler compiler, uint id, uint member_index, SpvDecoration decoration, uint argument);

        private static readonly PFN_spvc_compiler_set_member_decoration spvc_compiler_set_member_decoration_ = LoadFunction<PFN_spvc_compiler_set_member_decoration>("spvc_compiler_set_member_decoration");

        public static void spvcCompilerSetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, uint argument)
        {
            spvc_compiler_set_member_decoration_(compiler, id, memberIndex, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_member_decoration_string(SpvcCompiler compiler, uint id, uint member_index, SpvDecoration decoration, byte* argument);

        private static readonly PFN_spvc_compiler_set_member_decoration_string spvc_compiler_set_member_decoration_string_ = LoadFunction<PFN_spvc_compiler_set_member_decoration_string>("spvc_compiler_set_member_decoration_string");

        public static void spvcCompilerSetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration, byte* argument)
        {
            spvc_compiler_set_member_decoration_string_(compiler, id, memberIndex, decoration, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_member_name(SpvcCompiler compiler, uint id, uint member_index, byte* argument);

        private static readonly PFN_spvc_compiler_set_member_name spvc_compiler_set_member_name_ = LoadFunction<PFN_spvc_compiler_set_member_name>("spvc_compiler_set_member_name");

        public static void spvcCompilerSetMemberName(SpvcCompiler compiler, uint id, uint memberIndex, byte* argument)
        {
            spvc_compiler_set_member_name_(compiler, id, memberIndex, argument);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_unset_decoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_unset_decoration spvc_compiler_unset_decoration_ = LoadFunction<PFN_spvc_compiler_unset_decoration>("spvc_compiler_unset_decoration");

        public static void spvcCompilerUnsetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            spvc_compiler_unset_decoration_(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_unset_member_decoration(SpvcCompiler compiler, uint id, uint member_index, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_unset_member_decoration spvc_compiler_unset_member_decoration_ = LoadFunction<PFN_spvc_compiler_unset_member_decoration>("spvc_compiler_unset_member_decoration");

        public static void spvcCompilerUnsetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            spvc_compiler_unset_member_decoration_(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_has_decoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_has_decoration spvc_compiler_has_decoration_ = LoadFunction<PFN_spvc_compiler_has_decoration>("spvc_compiler_has_decoration");

        public static bool spvcCompilerHasDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            return spvc_compiler_has_decoration_(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_has_member_decoration(SpvcCompiler compiler, uint id, uint member_index, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_has_member_decoration spvc_compiler_has_member_decoration_ = LoadFunction<PFN_spvc_compiler_has_member_decoration>("spvc_compiler_has_member_decoration");

        public static bool spvcCompilerHasMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            return spvc_compiler_has_member_decoration_(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_get_name(SpvcCompiler compiler, SpvId id);

        private static readonly PFN_spvc_compiler_get_name spvc_compiler_get_name_ = LoadFunction<PFN_spvc_compiler_get_name>("spvc_compiler_get_name");

        public static byte* spvcCompilerGetName(SpvcCompiler compiler, SpvId id)
        {
            return spvc_compiler_get_name_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_get_decoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_get_decoration spvc_compiler_get_decoration_ = LoadFunction<PFN_spvc_compiler_get_decoration>("spvc_compiler_get_decoration");

        public static uint SpvcCompilerGetDecoration(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            return spvc_compiler_get_decoration_(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_get_decoration_string(SpvcCompiler compiler, SpvId id, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_get_decoration_string spvc_compiler_get_decoration_string_ = LoadFunction<PFN_spvc_compiler_get_decoration_string>("spvc_compiler_get_decoration_string");

        public static byte* spvcCompilerGetDecorationString(SpvcCompiler compiler, SpvId id, SpvDecoration decoration)
        {
            return spvc_compiler_get_decoration_string_(compiler, id, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_get_member_decoration(SpvcCompiler compiler, uint id, uint member_index, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_get_member_decoration spvc_compiler_get_member_decoration_ = LoadFunction<PFN_spvc_compiler_get_member_decoration>("spvc_compiler_get_member_decoration");

        public static uint spvcCompilerGetMemberDecoration(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            return spvc_compiler_get_member_decoration_(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_get_member_decoration_string(SpvcCompiler compiler, uint id, uint member_index, SpvDecoration decoration);

        private static readonly PFN_spvc_compiler_get_member_decoration_string spvc_compiler_get_member_decoration_string_ = LoadFunction<PFN_spvc_compiler_get_member_decoration_string>("spvc_compiler_get_member_decoration_string");

        public static byte* spvcCompilerGetMemberDecorationString(SpvcCompiler compiler, uint id, uint memberIndex, SpvDecoration decoration)
        {
            return spvc_compiler_get_member_decoration_string_(compiler, id, memberIndex, decoration);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_get_member_name(SpvcCompiler compiler, uint id, uint member_index);

        private static readonly PFN_spvc_compiler_get_member_name spvc_compiler_get_member_name_ = LoadFunction<PFN_spvc_compiler_get_member_name>("spvc_compiler_get_member_name");

        public static byte* spvcCompilerGetMemberName(SpvcCompiler compiler, uint id, uint memberIndex)
        {
            return spvc_compiler_get_member_name_(compiler, id, memberIndex);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_entry_points(SpvcCompiler compiler, SpvcEntryPoint* entry_points, nuint* num_entry_points);

        private static readonly PFN_spvc_compiler_get_entry_points spvc_compiler_get_entry_points_ = LoadFunction<PFN_spvc_compiler_get_entry_points>("spvc_compiler_get_entry_points");

        public static SpvcResult spvcCompilerGetEntryPoints(SpvcCompiler compiler, SpvcEntryPoint* entryPoints, nuint* numEntryPoints)
        {
            return spvc_compiler_get_entry_points_(compiler, entryPoints, numEntryPoints);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_set_entry_point(SpvcCompiler compiler, byte* name, SpvExecutionModel model);

        private static readonly PFN_spvc_compiler_set_entry_point spvc_compiler_set_entry_point_ = LoadFunction<PFN_spvc_compiler_set_entry_point>("spvc_compiler_set_entry_point");

        public static SpvcResult spvcCompilerSetEntryPoint(SpvcCompiler compiler, byte* name, SpvExecutionModel model)
        {
            return spvc_compiler_set_entry_point_(compiler, name, model);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_rename_entry_point(SpvcCompiler compiler, byte* old_name, byte* new_name, SpvExecutionModel model);

        private static readonly PFN_spvc_compiler_rename_entry_point spvc_compiler_rename_entry_point_ = LoadFunction<PFN_spvc_compiler_rename_entry_point>("spvc_compiler_rename_entry_point");

        public static SpvcResult spvcCompilerRenameEntryPoint(SpvcCompiler compiler, byte* oldName, byte* newName, SpvExecutionModel model)
        {
            return spvc_compiler_rename_entry_point_(compiler, oldName, newName, model);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_get_cleansed_entry_point_name(SpvcCompiler compiler, byte* name, SpvExecutionModel model);

        private static readonly PFN_spvc_compiler_get_cleansed_entry_point_name spvc_compiler_get_cleansed_entry_point_name_ = LoadFunction<PFN_spvc_compiler_get_cleansed_entry_point_name>("spvc_compiler_get_cleansed_entry_point_name");

        public static byte* spvcCompilerGetCleansedEntryPointName(SpvcCompiler compiler, byte* name, SpvExecutionModel model)
        {
            return spvc_compiler_get_cleansed_entry_point_name_(compiler, name, model);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_execution_mode(SpvcCompiler compiler, SpvExecutionMode mode);

        private static readonly PFN_spvc_compiler_set_execution_mode spvc_compiler_set_execution_mode_ = LoadFunction<PFN_spvc_compiler_set_execution_mode>("spvc_compiler_set_execution_mode");

        public static void spvcCompilerSetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode)
        {
            spvc_compiler_set_execution_mode_(compiler, mode);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_unset_execution_mode(SpvcCompiler compiler, SpvExecutionMode mode);

        private static readonly PFN_spvc_compiler_unset_execution_mode spvc_compiler_unset_execution_mode_ = LoadFunction<PFN_spvc_compiler_unset_execution_mode>("spvc_compiler_unset_execution_mode");

        public static void spvcCompilerUnsetExecutionMode(SpvcCompiler compiler, SpvExecutionMode mode)
        {
            spvc_compiler_unset_execution_mode_(compiler, mode);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_compiler_set_execution_mode_with_arguments(SpvcCompiler compiler, SpvExecutionMode mode, uint arg0, uint arg1, uint arg2);

        private static readonly PFN_spvc_compiler_set_execution_mode_with_arguments spvc_compiler_set_execution_mode_with_arguments_ = LoadFunction<PFN_spvc_compiler_set_execution_mode_with_arguments>("spvc_compiler_set_execution_mode_with_arguments");

        public static void spvcCompilerSetExecutionModeWithArguments(SpvcCompiler compiler, SpvExecutionMode mode, uint arg0, uint arg1, uint arg2)
        {
            spvc_compiler_set_execution_mode_with_arguments_(compiler, mode, arg0, arg1, arg2);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_execution_modes(SpvcCompiler compiler, SpvExecutionMode* modes, nuint* num_modes);

        private static readonly PFN_spvc_compiler_get_execution_modes spvc_compiler_get_execution_modes_ = LoadFunction<PFN_spvc_compiler_get_execution_modes>("spvc_compiler_get_execution_modes");

        public static SpvcResult spvcCompilerGetExecutionModes(SpvcCompiler compiler, SpvExecutionMode* modes, nuint* numModes)
        {
            return spvc_compiler_get_execution_modes_(compiler, modes, numModes);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_get_execution_mode_argument(SpvcCompiler compiler, SpvExecutionMode mode);

        private static readonly PFN_spvc_compiler_get_execution_mode_argument spvc_compiler_get_execution_mode_argument_ = LoadFunction<PFN_spvc_compiler_get_execution_mode_argument>("spvc_compiler_get_execution_mode_argument");

        public static uint spvcCompilerGetExecutionModeArgument(SpvcCompiler compiler, SpvExecutionMode mode)
        {
            return spvc_compiler_get_execution_mode_argument_(compiler, mode);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_get_execution_mode_argument_by_index(SpvcCompiler compiler, SpvExecutionMode mode, uint index);

        private static readonly PFN_spvc_compiler_get_execution_mode_argument_by_index spvc_compiler_get_execution_mode_argument_by_index_ = LoadFunction<PFN_spvc_compiler_get_execution_mode_argument_by_index>("spvc_compiler_get_execution_mode_argument_by_index");

        public static uint spvcCompilerGetExecutionModeArgumentByIndex(SpvcCompiler compiler, SpvExecutionMode mode, uint index)
        {
            return spvc_compiler_get_execution_mode_argument_by_index_(compiler, mode, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvExecutionModel PFN_spvc_compiler_get_execution_model(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_get_execution_model spvc_compiler_get_execution_model_ = LoadFunction<PFN_spvc_compiler_get_execution_model>("spvc_compiler_get_execution_model");

        public static SpvExecutionModel SpvcCompilerGetExecutionModel(SpvcCompiler compiler)
        {
            return spvc_compiler_get_execution_model_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcType PFN_spvc_compiler_get_type_handle(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_get_type_handle spvc_compiler_get_type_handle_ = LoadFunction<PFN_spvc_compiler_get_type_handle>("spvc_compiler_get_type_handle");

        public static SpvcType SpvcCompilerGetTypeHandle(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_get_type_handle_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_base_type_id(SpvcType type);

        private static readonly PFN_spvc_type_get_base_type_id spvc_type_get_base_type_id_ = LoadFunction<PFN_spvc_type_get_base_type_id>("spvc_type_get_base_type_id");

        public static uint spvcTypeGetBaseTypeId(SpvcType type)
        {
            return spvc_type_get_base_type_id_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcBasetype PFN_spvc_type_get_basetype(SpvcType type);

        private static readonly PFN_spvc_type_get_basetype spvc_type_get_basetype_ = LoadFunction<PFN_spvc_type_get_basetype>("spvc_type_get_basetype");

        public static SpvcBasetype spvcTypeGetBasetype(SpvcType type)
        {
            return spvc_type_get_basetype_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_bit_width(SpvcType type);

        private static readonly PFN_spvc_type_get_bit_width spvc_type_get_bit_width_ = LoadFunction<PFN_spvc_type_get_bit_width>("spvc_type_get_bit_width");

        public static uint spvcTypeGetBitWidth(SpvcType type)
        {
            return spvc_type_get_bit_width_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_vector_size(SpvcType type);

        private static readonly PFN_spvc_type_get_vector_size spvc_type_get_vector_size_ = LoadFunction<PFN_spvc_type_get_vector_size>("spvc_type_get_vector_size");

        public static uint spvcTypeGetVectorSize(SpvcType type)
        {
            return spvc_type_get_vector_size_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_columns(SpvcType type);

        private static readonly PFN_spvc_type_get_columns spvc_type_get_columns_ = LoadFunction<PFN_spvc_type_get_columns>("spvc_type_get_columns");

        public static uint spvcTypeGetColumns(SpvcType type)
        {
            return spvc_type_get_columns_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_num_array_dimensions(SpvcType type);

        private static readonly PFN_spvc_type_get_num_array_dimensions spvc_type_get_num_array_dimensions_ = LoadFunction<PFN_spvc_type_get_num_array_dimensions>("spvc_type_get_num_array_dimensions");

        public static uint spvcTypeGetNumArrayDimensions(SpvcType type)
        {
            return spvc_type_get_num_array_dimensions_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_type_array_dimension_is_literal(SpvcType type, uint dimension);

        private static readonly PFN_spvc_type_array_dimension_is_literal spvc_type_array_dimension_is_literal_ = LoadFunction<PFN_spvc_type_array_dimension_is_literal>("spvc_type_array_dimension_is_literal");

        public static bool spvcTypeArrayDimensionIsLiteral(SpvcType type, uint dimension)
        {
            return spvc_type_array_dimension_is_literal_(type, dimension);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvId PFN_spvc_type_get_array_dimension(SpvcType type, uint dimension);

        private static readonly PFN_spvc_type_get_array_dimension spvc_type_get_array_dimension_ = LoadFunction<PFN_spvc_type_get_array_dimension>("spvc_type_get_array_dimension");

        public static SpvId spvcTypeGetArrayDimension(SpvcType type, uint dimension)
        {
            return spvc_type_get_array_dimension_(type, dimension);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_num_member_types(SpvcType type);

        private static readonly PFN_spvc_type_get_num_member_types spvc_type_get_num_member_types_ = LoadFunction<PFN_spvc_type_get_num_member_types>("spvc_type_get_num_member_types");

        public static uint spvcTypeGetNumMemberTypes(SpvcType type)
        {
            return spvc_type_get_num_member_types_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_member_type(SpvcType type, uint index);

        private static readonly PFN_spvc_type_get_member_type spvc_type_get_member_type_ = LoadFunction<PFN_spvc_type_get_member_type>("spvc_type_get_member_type");

        public static uint spvcTypeGetMemberType(SpvcType type, uint index)
        {
            return spvc_type_get_member_type_(type, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvStorageClass PFN_spvc_type_get_storage_class(SpvcType type);

        private static readonly PFN_spvc_type_get_storage_class spvc_type_get_storage_class_ = LoadFunction<PFN_spvc_type_get_storage_class>("spvc_type_get_storage_class");

        public static SpvStorageClass spvcTypeGetStorageClass(SpvcType type)
        {
            return spvc_type_get_storage_class_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_type_get_image_sampled_type(SpvcType type);

        private static readonly PFN_spvc_type_get_image_sampled_type spvc_type_get_image_sampled_type_ = LoadFunction<PFN_spvc_type_get_image_sampled_type>("spvc_type_get_image_sampled_type");

        public static uint spvcTypeGetImageSampledType(SpvcType type)
        {
            return spvc_type_get_image_sampled_type_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvDim PFN_spvc_type_get_image_dimension(SpvcType type);

        private static readonly PFN_spvc_type_get_image_dimension spvc_type_get_image_dimension_ = LoadFunction<PFN_spvc_type_get_image_dimension>("spvc_type_get_image_dimension");

        public static SpvDim spvcTypeGetImageDimension(SpvcType type)
        {
            return spvc_type_get_image_dimension_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_type_get_image_is_depth(SpvcType type);

        private static readonly PFN_spvc_type_get_image_is_depth spvc_type_get_image_is_depth_ = LoadFunction<PFN_spvc_type_get_image_is_depth>("spvc_type_get_image_is_depth");

        public static bool spvcTypeGetImageIsDepth(SpvcType type)
        {
            return spvc_type_get_image_is_depth_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_type_get_image_arrayed(SpvcType type);

        private static readonly PFN_spvc_type_get_image_arrayed spvc_type_get_image_arrayed_ = LoadFunction<PFN_spvc_type_get_image_arrayed>("spvc_type_get_image_arrayed");

        public static bool spvcTypeGetImageArrayed(SpvcType type)
        {
            return spvc_type_get_image_arrayed_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_type_get_image_multisampled(SpvcType type);

        private static readonly PFN_spvc_type_get_image_multisampled spvc_type_get_image_multisampled_ = LoadFunction<PFN_spvc_type_get_image_multisampled>("spvc_type_get_image_multisampled");

        public static bool spvcTypeGetImageMultisampled(SpvcType type)
        {
            return spvc_type_get_image_multisampled_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_type_get_image_is_storage(SpvcType type);

        private static readonly PFN_spvc_type_get_image_is_storage spvc_type_get_image_is_storage_ = LoadFunction<PFN_spvc_type_get_image_is_storage>("spvc_type_get_image_is_storage");

        public static bool spvcTypeGetImageIsStorage(SpvcType type)
        {
            return spvc_type_get_image_is_storage_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvImageFormat PFN_spvc_type_get_image_storage_format(SpvcType type);

        private static readonly PFN_spvc_type_get_image_storage_format spvc_type_get_image_storage_format_ = LoadFunction<PFN_spvc_type_get_image_storage_format>("spvc_type_get_image_storage_format");

        public static SpvImageFormat spvcTypeGetImageStorageFormat(SpvcType type)
        {
            return spvc_type_get_image_storage_format_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvAccessQualifier PFN_spvc_type_get_image_access_qualifier(SpvcType type);

        private static readonly PFN_spvc_type_get_image_access_qualifier spvc_type_get_image_access_qualifier_ = LoadFunction<PFN_spvc_type_get_image_access_qualifier>("spvc_type_get_image_access_qualifier");

        public static SpvAccessQualifier spvcTypeGetImageAccessQualifier(SpvcType type)
        {
            return spvc_type_get_image_access_qualifier_(type);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_declared_struct_size(SpvcCompiler compiler, SpvcType struct_type, nuint* size);

        private static readonly PFN_spvc_compiler_get_declared_struct_size spvc_compiler_get_declared_struct_size_ = LoadFunction<PFN_spvc_compiler_get_declared_struct_size>("spvc_compiler_get_declared_struct_size");

        public static SpvcResult SpvcCompilerGetDeclaredStructSize(SpvcCompiler compiler, SpvcType structType, nuint* size)
        {
            return spvc_compiler_get_declared_struct_size_(compiler, structType, size);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_declared_struct_size_runtime_array(SpvcCompiler compiler, SpvcType struct_type, nuint array_size, nuint* size);

        private static readonly PFN_spvc_compiler_get_declared_struct_size_runtime_array spvc_compiler_get_declared_struct_size_runtime_array_ = LoadFunction<PFN_spvc_compiler_get_declared_struct_size_runtime_array>("spvc_compiler_get_declared_struct_size_runtime_array");

        public static SpvcResult spvcCompilerGetDeclaredStructSizeRuntimeArray(SpvcCompiler compiler, SpvcType structType, nuint arraySize, nuint* size)
        {
            return spvc_compiler_get_declared_struct_size_runtime_array_(compiler, structType, arraySize, size);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_declared_struct_member_size(SpvcCompiler compiler, SpvcType type, uint index, nuint* size);

        private static readonly PFN_spvc_compiler_get_declared_struct_member_size spvc_compiler_get_declared_struct_member_size_ = LoadFunction<PFN_spvc_compiler_get_declared_struct_member_size>("spvc_compiler_get_declared_struct_member_size");

        public static SpvcResult spvcCompilerGetDeclaredStructMemberSize(SpvcCompiler compiler, SpvcType type, uint index, nuint* size)
        {
            return spvc_compiler_get_declared_struct_member_size_(compiler, type, index, size);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_type_struct_member_offset(SpvcCompiler compiler, SpvcType type, uint index, uint* offset);

        private static readonly PFN_spvc_compiler_type_struct_member_offset spvc_compiler_type_struct_member_offset_ = LoadFunction<PFN_spvc_compiler_type_struct_member_offset>("spvc_compiler_type_struct_member_offset");

        public static SpvcResult spvcCompilerTypeStructMemberOffset(SpvcCompiler compiler, SpvcType type, uint index, uint* offset)
        {
            return spvc_compiler_type_struct_member_offset_(compiler, type, index, offset);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_type_struct_member_array_stride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride);

        private static readonly PFN_spvc_compiler_type_struct_member_array_stride spvc_compiler_type_struct_member_array_stride_ = LoadFunction<PFN_spvc_compiler_type_struct_member_array_stride>("spvc_compiler_type_struct_member_array_stride");

        public static SpvcResult spvcCompilerTypeStructMemberArrayStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride)
        {
            return spvc_compiler_type_struct_member_array_stride_(compiler, type, index, stride);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_type_struct_member_matrix_stride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride);

        private static readonly PFN_spvc_compiler_type_struct_member_matrix_stride spvc_compiler_type_struct_member_matrix_stride_ = LoadFunction<PFN_spvc_compiler_type_struct_member_matrix_stride>("spvc_compiler_type_struct_member_matrix_stride");

        public static SpvcResult spvcCompilerTypeStructMemberMatrixStride(SpvcCompiler compiler, SpvcType type, uint index, uint* stride)
        {
            return spvc_compiler_type_struct_member_matrix_stride_(compiler, type, index, stride);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_build_dummy_sampler_for_combined_images(SpvcCompiler compiler, uint* id);

        private static readonly PFN_spvc_compiler_build_dummy_sampler_for_combined_images spvc_compiler_build_dummy_sampler_for_combined_images_ = LoadFunction<PFN_spvc_compiler_build_dummy_sampler_for_combined_images>("spvc_compiler_build_dummy_sampler_for_combined_images");

        public static SpvcResult spvcCompilerBuildDummySamplerForCombinedImages(SpvcCompiler compiler, uint* id)
        {
            return spvc_compiler_build_dummy_sampler_for_combined_images_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_build_combined_image_samplers(SpvcCompiler compiler);

        private static readonly PFN_spvc_compiler_build_combined_image_samplers spvc_compiler_build_combined_image_samplers_ = LoadFunction<PFN_spvc_compiler_build_combined_image_samplers>("spvc_compiler_build_combined_image_samplers");

        public static SpvcResult spvcCompilerBuildCombinedImageSamplers(SpvcCompiler compiler)
        {
            return spvc_compiler_build_combined_image_samplers_(compiler);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_combined_image_samplers(SpvcCompiler compiler, SpvcCombinedImageSampler* samplers, nuint* num_samplers);

        private static readonly PFN_spvc_compiler_get_combined_image_samplers spvc_compiler_get_combined_image_samplers_ = LoadFunction<PFN_spvc_compiler_get_combined_image_samplers>("spvc_compiler_get_combined_image_samplers");

        public static SpvcResult spvcCompilerGetCombinedImageSamplers(SpvcCompiler compiler, SpvcCombinedImageSampler* samplers, nuint* numSamplers)
        {
            return spvc_compiler_get_combined_image_samplers_(compiler, samplers, numSamplers);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_specialization_constants(SpvcCompiler compiler, SpvcSpecializationConstant* constants, nuint* num_constants);

        private static readonly PFN_spvc_compiler_get_specialization_constants spvc_compiler_get_specialization_constants_ = LoadFunction<PFN_spvc_compiler_get_specialization_constants>("spvc_compiler_get_specialization_constants");

        public static SpvcResult spvcCompilerGetSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* constants, nuint* numConstants)
        {
            return spvc_compiler_get_specialization_constants_(compiler, constants, numConstants);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcConstant PFN_spvc_compiler_get_constant_handle(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_get_constant_handle spvc_compiler_get_constant_handle_ = LoadFunction<PFN_spvc_compiler_get_constant_handle>("spvc_compiler_get_constant_handle");

        public static SpvcConstant spvcCompilerGetConstantHandle(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_get_constant_handle_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_compiler_get_work_group_size_specialization_constants(SpvcCompiler compiler, SpvcSpecializationConstant* x, SpvcSpecializationConstant* y, SpvcSpecializationConstant* z);

        private static readonly PFN_spvc_compiler_get_work_group_size_specialization_constants spvc_compiler_get_work_group_size_specialization_constants_ = LoadFunction<PFN_spvc_compiler_get_work_group_size_specialization_constants>("spvc_compiler_get_work_group_size_specialization_constants");

        public static uint spvcCompilerGetWorkGroupSizeSpecializationConstants(SpvcCompiler compiler, SpvcSpecializationConstant* x, SpvcSpecializationConstant* y, SpvcSpecializationConstant* z)
        {
            return spvc_compiler_get_work_group_size_specialization_constants_(compiler, x, y, z);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_active_buffer_ranges(SpvcCompiler compiler, uint id, SpvcBufferRange* ranges, nuint* num_ranges);

        private static readonly PFN_spvc_compiler_get_active_buffer_ranges spvc_compiler_get_active_buffer_ranges_ = LoadFunction<PFN_spvc_compiler_get_active_buffer_ranges>("spvc_compiler_get_active_buffer_ranges");

        public static SpvcResult spvcCompilerGetActiveBufferRanges(SpvcCompiler compiler, uint id, SpvcBufferRange* ranges, nuint* numRanges)
        {
            return spvc_compiler_get_active_buffer_ranges_(compiler, id, ranges, numRanges);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float PFN_spvc_constant_get_scalar_fp16(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_fp16 spvc_constant_get_scalar_fp16_ = LoadFunction<PFN_spvc_constant_get_scalar_fp16>(nameof(spvcConstantGetScalarFp16));

        public static float spvcConstantGetScalarFp16(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_fp16_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float PFN_spvc_constant_get_scalar_fp32(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_fp32 spvc_constant_get_scalar_fp32_ = LoadFunction<PFN_spvc_constant_get_scalar_fp32>(nameof(spvcConstantGetScalarFp32));

        public static float spvcConstantGetScalarFp32(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_fp32_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate double PFN_spvc_constant_get_scalar_fp64(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_fp64 spvc_constant_get_scalar_fp64_ = LoadFunction<PFN_spvc_constant_get_scalar_fp64>(nameof(spvcConstantGetScalarFp64));

        public static double spvcConstantGetScalarFp64(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_fp64_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_constant_get_scalar_u32(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_u32 spvc_constant_get_scalar_u32_ = LoadFunction<PFN_spvc_constant_get_scalar_u32>(nameof(spvcConstantGetScalarU32));

        public static uint spvcConstantGetScalarU32(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_u32_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PFN_spvc_constant_get_scalar_i32(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_i32 spvc_constant_get_scalar_i32_ = LoadFunction<PFN_spvc_constant_get_scalar_i32>(nameof(spvcConstantGetScalarI32));

        public static int spvcConstantGetScalarI32(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_i32_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_constant_get_scalar_u16(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_u16 spvc_constant_get_scalar_u16_ = LoadFunction<PFN_spvc_constant_get_scalar_u16>(nameof(spvcConstantGetScalarU16));

        public static uint spvcConstantGetScalarU16(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_u16_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PFN_spvc_constant_get_scalar_i16(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_i16 spvc_constant_get_scalar_i16_ = LoadFunction<PFN_spvc_constant_get_scalar_i16>(nameof(spvcConstantGetScalarI16));

        public static int spvcConstantGetScalarI16(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_i16_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_constant_get_scalar_u8(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_u8 spvc_constant_get_scalar_u8_ = LoadFunction<PFN_spvc_constant_get_scalar_u8>(nameof(spvcConstantGetScalarU8));

        public static uint spvcConstantGetScalarU8(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_u8_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PFN_spvc_constant_get_scalar_i8(SpvcConstant constant, uint column, uint row);

        private static readonly PFN_spvc_constant_get_scalar_i8 spvc_constant_get_scalar_i8_ = LoadFunction<PFN_spvc_constant_get_scalar_i8>(nameof(spvcConstantGetScalarI8));

        public static int spvcConstantGetScalarI8(SpvcConstant constant, uint column, uint row)
        {
            return spvc_constant_get_scalar_i8_(constant, column, row);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFN_spvc_constant_get_subconstants(SpvcConstant constant, uint* constituents, nuint* count);

        private static readonly PFN_spvc_constant_get_subconstants spvc_constant_get_subconstants_ = LoadFunction<PFN_spvc_constant_get_subconstants>("spvc_constant_get_subconstants");

        public static void spvcConstantGetSubconstants(SpvcConstant constant, uint* constituents, nuint* count)
        {
            spvc_constant_get_subconstants_(constant, constituents, count);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint PFN_spvc_constant_get_type(SpvcConstant constant);

        private static readonly PFN_spvc_constant_get_type spvc_constant_get_type_ = LoadFunction<PFN_spvc_constant_get_type>("spvc_constant_get_type");

        public static uint spvcConstantGetType(SpvcConstant constant)
        {
            return spvc_constant_get_type_(constant);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_get_binary_offset_for_decoration(SpvcCompiler compiler, uint id, SpvDecoration decoration, uint* word_offset);

        private static readonly PFN_spvc_compiler_get_binary_offset_for_decoration spvc_compiler_get_binary_offset_for_decoration_ = LoadFunction<PFN_spvc_compiler_get_binary_offset_for_decoration>("spvc_compiler_get_binary_offset_for_decoration");

        public static bool spvcCompilerGetBinaryOffsetForDecoration(SpvcCompiler compiler, uint id, SpvDecoration decoration, uint* wordOffset)
        {
            return spvc_compiler_get_binary_offset_for_decoration_(compiler, id, decoration, wordOffset);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_buffer_is_hlsl_counter_buffer(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_buffer_is_hlsl_counter_buffer spvc_compiler_buffer_is_hlsl_counter_buffer_ = LoadFunction<PFN_spvc_compiler_buffer_is_hlsl_counter_buffer>("spvc_compiler_buffer_is_hlsl_counter_buffer");

        public static bool spvcCompilerBufferIsHlslCounterBuffer(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_buffer_is_hlsl_counter_buffer_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool PFN_spvc_compiler_buffer_get_hlsl_counter_buffer(SpvcCompiler compiler, uint id, uint* counter_id);

        private static readonly PFN_spvc_compiler_buffer_get_hlsl_counter_buffer spvc_compiler_buffer_get_hlsl_counter_buffer_ = LoadFunction<PFN_spvc_compiler_buffer_get_hlsl_counter_buffer>("spvc_compiler_buffer_get_hlsl_counter_buffer");

        public static bool spvcCompilerBufferGetHlslCounterBuffer(SpvcCompiler compiler, uint id, uint* counterId)
        {
            return spvc_compiler_buffer_get_hlsl_counter_buffer_(compiler, id, counterId);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_declared_capabilities(SpvcCompiler compiler, SpvCapability* capabilities, nuint* num_capabilities);

        private static readonly PFN_spvc_compiler_get_declared_capabilities spvc_compiler_get_declared_capabilities_ = LoadFunction<PFN_spvc_compiler_get_declared_capabilities>("spvc_compiler_get_declared_capabilities");

        public static SpvcResult spvcCompilerGetDeclaredCapabilities(SpvcCompiler compiler, SpvCapability* capabilities, nuint* numCapabilities)
        {
            return spvc_compiler_get_declared_capabilities_(compiler, capabilities, numCapabilities);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_declared_extensions(SpvcCompiler compiler, byte* extensions, nuint* num_extensions);

        private static readonly PFN_spvc_compiler_get_declared_extensions spvc_compiler_get_declared_extensions_ = LoadFunction<PFN_spvc_compiler_get_declared_extensions>("spvc_compiler_get_declared_extensions");

        public static SpvcResult spvcCompilerGetDeclaredExtensions(SpvcCompiler compiler, byte* extensions, nuint* numExtensions)
        {
            return spvc_compiler_get_declared_extensions_(compiler, extensions, numExtensions);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* PFN_spvc_compiler_get_remapped_declared_block_name(SpvcCompiler compiler, uint id);

        private static readonly PFN_spvc_compiler_get_remapped_declared_block_name spvc_compiler_get_remapped_declared_block_name_ = LoadFunction<PFN_spvc_compiler_get_remapped_declared_block_name>("spvc_compiler_get_remapped_declared_block_name");

        public static byte* spvcCompilerGetRemappedDeclaredBlockName(SpvcCompiler compiler, uint id)
        {
            return spvc_compiler_get_remapped_declared_block_name_(compiler, id);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SpvcResult PFN_spvc_compiler_get_buffer_block_decorations(SpvcCompiler compiler, uint id, SpvDecoration* decorations, nuint* num_decorations);

        private static readonly PFN_spvc_compiler_get_buffer_block_decorations spvc_compiler_get_buffer_block_decorations_ = LoadFunction<PFN_spvc_compiler_get_buffer_block_decorations>("spvc_compiler_get_buffer_block_decorations");

        public static SpvcResult spvcCompilerGetBufferBlockDecorations(SpvcCompiler compiler, uint id, SpvDecoration* decorations, nuint* numDecorations)
        {
            return spvc_compiler_get_buffer_block_decorations_(compiler, id, decorations, numDecorations);
        }
    }
}