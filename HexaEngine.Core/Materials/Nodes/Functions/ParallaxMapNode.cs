namespace HexaEngine.Materials.Nodes.Functions
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.Numerics;

    public enum ParallaxMode
    {
        ParallaxMapping,
        ParallaxSteepMapping,
        ParallaxOcclusionMapping
    }

    public class ParallaxMapNode : FuncCallDeclarationBaseNode
    {
        private ParallaxMode parallaxMode;
        private int maxLayers = 32;
        private int minLayers = 8;

        [JsonConstructor]
        public ParallaxMapNode(int id, bool removable, bool isStatic) : base(id, "Parallax Map", removable, isStatic)
        {
        }

        public ParallaxMapNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            // Define output and input pins with appropriate handling for default values and user input
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, PinType.Float2)); // Output displaced UV coordinates
            InTex = AddOrGetPin(new Pin(editor.GetUniqueId(), "Height", PinShape.QuadFilled, PinKind.Input, PinType.Texture2D, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "UV", PinShape.QuadFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv")); // Default to UV coordinates
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "ViewDir", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.viewDir")); // Default to view direction in tangent space

            // ParallaxScale and ParallaxBias have default values but no defaultExpression
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Scale", PinShape.QuadFilled, PinKind.Input, PinType.Float, new Vector4(0.5f, 0, 0, 0), 1)); // Default scale of 0.05
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Bias", PinShape.QuadFilled, PinKind.Input, PinType.Float, new Vector4(0f, 0, 0, 0), 1)); // Default bias of -0.02

            base.Initialize(editor);
        }

        public ParallaxMode ParallaxMode
        {
            get => parallaxMode;
            set
            {
                OnValueChanging();
                parallaxMode = value;
                OnValueChanged();
            }
        }

        public int MaxLayers
        {
            get => maxLayers;
            set
            {
                OnValueChanging();
                maxLayers = value;
                OnValueChanged();
            }
        }

        public int MinLayers
        {
            get => minLayers;
            set
            {
                OnValueChanging();
                minLayers = value;
                OnValueChanged();
            }
        }

        [JsonIgnore]
        public override PrimitivePin Out { get; protected set; } = null!;

        [JsonIgnore] public Pin InTex { get; protected set; } = null!;

        [JsonIgnore]
        public override string MethodName => "ParallaxMap";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float2); // Output type is a Float2 for the displaced UV coordinates

        private static string GetParallaxMappingBodyFallback()
        {
            return $@"return uv;";
        }

        private static string GetParallaxMappingBody(string texture, string sampler)
        {
            return $@"
            float height =  {texture}.Sample({sampler}, uv).r;
            float2 p = viewDir.xy / viewDir.z * (height * parallaxScale);
            return uv - p;";
        }

        private string GetParallaxSteepMappingBody(string texture, string sampler)
        {
            return @$"
            const float minLayers = {MinLayers};
            const float maxLayers = {MaxLayers};
            float numLayers = lerp(maxLayers, minLayers, max(dot(float3(0.0, 0.0, 1.0), viewDir), 0.0));

            float layerDepth = 1.0 / numLayers;

            float currentLayerDepth = 0.0;

            float2 P = viewDir.xy * parallaxScale;
            float2 deltaTexCoords = P / numLayers;

            float2 currentTexCoords = uv;
            float currentDepthMapValue =  {texture}.Sample({sampler}, uv).r;

            [unroll({MaxLayers})]
            while(currentLayerDepth < currentDepthMapValue)
            {{
                // shift texture coordinates along direction of P
                currentTexCoords -= deltaTexCoords;
                // get depthmap value at current texture coordinates
                currentDepthMapValue = {texture}.Sample({sampler}, currentTexCoords).r;
                // get depth of next layer
                currentLayerDepth += layerDepth;
            }}

            return currentTexCoords;";
        }

        private string GetParallaxOcclusionMappingBody(string texture, string sampler)
        {
            return @$"
            const float minLayers = {MinLayers};
            const float maxLayers = {MaxLayers};
            float numLayers = lerp(maxLayers, minLayers, max(dot(float3(0.0, 0.0, 1.0), viewDir), 0.0));

            float layerDepth = 1.0 / numLayers;

            float currentLayerDepth = 0.0;

            float2 P = viewDir.xy * parallaxScale;
            float2 deltaTexCoords = P / numLayers;

            float2 currentTexCoords = uv;
            float currentDepthMapValue =  {texture}.Sample({sampler}, uv).r;

            [unroll({MaxLayers})]
            while(currentLayerDepth < currentDepthMapValue)
            {{
                // shift texture coordinates along direction of P
                currentTexCoords -= deltaTexCoords;
                // get depthmap value at current texture coordinates
                currentDepthMapValue = {texture}.Sample({sampler}, currentTexCoords).r;
                // get depth of next layer
                currentLayerDepth += layerDepth;
            }}

            // get texture coordinates before collision (reverse operations)
            float2 prevTexCoords = currentTexCoords + deltaTexCoords;

            // get depth after and before collision for linear interpolation
            float afterDepth  = currentDepthMapValue - currentLayerDepth;
            float beforeDepth = {texture}.Sample({sampler}, prevTexCoords).r - currentLayerDepth + layerDepth;

            // interpolation of texture coordinates
            float weight = afterDepth / (afterDepth - beforeDepth);
            float2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

            return finalTexCoords;";
        }

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            if (InTex.Links.Count == 0 || InTex.Links[0].OutputNode is not ITextureNode textureNode)
            {
                table.AddMethod(MethodName, "float2 uv, float3 viewDir, float parallaxScale, float parallaxBias", "float2", GetParallaxMappingBodyFallback());
                return;
            }

            context.GetVariableFirstLink(InTex); // force analyze the texture node first.

            var slotSrv = context.TextureMapping[textureNode];
            var slotSampler = context.SamplerMapping[textureNode];
            var srv = table.GetShaderResourceView(slotSrv);
            var sampler = table.GetSamplerState(slotSampler);

            // The HLSL method for parallax mapping
            string body = ParallaxMode switch
            {
                ParallaxMode.ParallaxMapping => GetParallaxMappingBody(srv.Name, sampler.Name),
                ParallaxMode.ParallaxSteepMapping => GetParallaxSteepMappingBody(srv.Name, sampler.Name),
                ParallaxMode.ParallaxOcclusionMapping => GetParallaxOcclusionMappingBody(srv.Name, sampler.Name),
                _ => GetParallaxMappingBodyFallback()
            };

            // Register the method in the shader, exposing the necessary parameters
            table.AddMethod(MethodName, "float2 uv, float3 viewDir, float parallaxScale, float parallaxBias", "float2", body);
        }
    }
}