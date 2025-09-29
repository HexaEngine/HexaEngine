using HexaEngine.Core.Materials.Nodes;
using HexaEngine.Core.Materials.Nodes.Functions;
using HexaEngine.Core.Materials.Nodes.Intrinsics;
using HexaEngine.Core.Materials.Nodes.Intrinsics.Conditional;
using HexaEngine.Core.Materials.Nodes.Intrinsics.Misc;
using HexaEngine.Core.Materials.Nodes.Intrinsics.Trigonometry;
using HexaEngine.Core.Materials.Nodes.Intrinsics.Vector;
using HexaEngine.Core.Materials.Nodes.Noise;
using HexaEngine.Materials;
using HexaEngine.Materials.Nodes;
using HexaEngine.Materials.Nodes.Buildin;
using HexaEngine.Materials.Nodes.Functions;
using HexaEngine.Materials.Nodes.Operations;
using HexaEngine.Materials.Nodes.Textures;
using System.Diagnostics.CodeAnalysis;

namespace HexaEngine.Core.Materials
{
    public static class MaterialNodeRegistry
    {
        private readonly static Dictionary<MaterialNodeType, List<MaterialNodeFactory>> nodeTypeToFactory = [];
        private readonly static Dictionary<Type, MaterialNodeFactory> typeToFactory = [];
        private readonly static List<MaterialNodeFactory> globalFactories = [];

        static MaterialNodeRegistry()
        {
            Register<ConstantNode>(MaterialNodeType.Constant, "Constant");
            Register<PropertyNode>(MaterialNodeType.Constant, "Property");
            Register<SwizzleVectorNode>(MaterialNodeType.Constant, "Swizzle Vector");
            Register<SplitNode>(MaterialNodeType.Constant, "Vector to Components");
            Register<PackNode>(MaterialNodeType.Constant, "Components to Vector");
            Register<CamPosNode>(MaterialNodeType.Constant, "Camera Pos");

            Register<GenericNoiseNode>(MaterialNodeType.Noise, "Generic Noise");

            Register<NormalMapNode>(MaterialNodeType.Method, "Normal Map");
            Register<ParallaxMapNode>(MaterialNodeType.Method, "Parallax Map");
            Register<RotateUVNode>(MaterialNodeType.Method, "Rotate UV");
            Register<FlipUVNode>(MaterialNodeType.Method, "Flip UVs");

            Register<TextureFileNode>(MaterialNodeType.Texture, "Texture File");

            Register<AbsNode>(MaterialNodeType.Intrinsic, "Abs");
            Register<AcosNode>(MaterialNodeType.Intrinsic, "Acos");
            Register<AllNode>(MaterialNodeType.Intrinsic, "All");
            Register<AnyNode>(MaterialNodeType.Intrinsic, "Any");
            Register<AsDoubleNode>(MaterialNodeType.Intrinsic, "Asdouble");
            Register<AsFloatNode>(MaterialNodeType.Intrinsic, "Asfloat");
            Register<AsinNode>(MaterialNodeType.Intrinsic, "Asin");
            Register<AsIntNode>(MaterialNodeType.Intrinsic, "Asint");
            Register<AsUIntNode>(MaterialNodeType.Intrinsic, "Asuint");
            Register<AtanNode>(MaterialNodeType.Intrinsic, "Atan");
            Register<Atan2Node>(MaterialNodeType.Intrinsic, "Atan2");
            Register<CeilingNode>(MaterialNodeType.Intrinsic, "Ceiling");
            Register<ClampNode>(MaterialNodeType.Intrinsic, "Clamp");
            Register<ClipNode>(MaterialNodeType.Intrinsic, "Clip");
            Register<CosNode>(MaterialNodeType.Intrinsic, "Cos");
            Register<CoshNode>(MaterialNodeType.Intrinsic, "Cosh");
            Register<CrossNode>(MaterialNodeType.Intrinsic, "Cross");
            Register<DDXNode>(MaterialNodeType.Intrinsic, "DDX");
            Register<DDXCoarseNode>(MaterialNodeType.Intrinsic, "DDXCoarse");
            Register<DDXFineNode>(MaterialNodeType.Intrinsic, "DDXFine");
            Register<DDYNode>(MaterialNodeType.Intrinsic, "DDY");
            Register<DDYCoarseNode>(MaterialNodeType.Intrinsic, "DDYCoarse");
            Register<DDYFineNode>(MaterialNodeType.Intrinsic, "DDYFine");
            Register<DegreesNode>(MaterialNodeType.Intrinsic, "Degrees");
            Register<DistanceNode>(MaterialNodeType.Intrinsic, "Distance");
            Register<DotNode>(MaterialNodeType.Intrinsic, "Dot");
            Register<ExpNode>(MaterialNodeType.Intrinsic, "Exp");
            Register<Exp2Node>(MaterialNodeType.Intrinsic, "Exp2");
            Register<FloorNode>(MaterialNodeType.Intrinsic, "Floor");
            Register<FmodNode>(MaterialNodeType.Intrinsic, "Fmod");
            Register<FracNode>(MaterialNodeType.Intrinsic, "Frac");
            Register<FWidthNode>(MaterialNodeType.Intrinsic, "FWidth");
            Register<LdexpNode>(MaterialNodeType.Intrinsic, "Ldexp");
            Register<LengthNode>(MaterialNodeType.Intrinsic, "Length");
            Register<LerpNode>(MaterialNodeType.Intrinsic, "Lerp");
            Register<LitNode>(MaterialNodeType.Intrinsic, "Lit");
            Register<LogNode>(MaterialNodeType.Intrinsic, "Log");
            Register<Log10Node>(MaterialNodeType.Intrinsic, "Log10");
            Register<Log2Node>(MaterialNodeType.Intrinsic, "Log2");
            Register<MaxNode>(MaterialNodeType.Intrinsic, "Max");
            Register<MinNode>(MaterialNodeType.Intrinsic, "Min");
            Register<NegateNode>(MaterialNodeType.Intrinsic, "Negate");
            Register<NormalizeNode>(MaterialNodeType.Intrinsic, "Normalize");
            Register<PowNode>(MaterialNodeType.Intrinsic, "Pow");
            Register<RadiansNode>(MaterialNodeType.Intrinsic, "Radians");
            Register<ReflectNode>(MaterialNodeType.Intrinsic, "Reflect");
            Register<RefractNode>(MaterialNodeType.Intrinsic, "Refract");
            Register<RoundNode>(MaterialNodeType.Intrinsic, "Round");
            Register<RpcNode>(MaterialNodeType.Intrinsic, "Rpc");
            Register<RsqrtNode>(MaterialNodeType.Intrinsic, "Rsqrt");
            Register<SaturateNode>(MaterialNodeType.Intrinsic, "Saturate");
            Register<SinNode>(MaterialNodeType.Intrinsic, "Sin");
            Register<SinhNode>(MaterialNodeType.Intrinsic, "Sinh");
            Register<SmoothStepNode>(MaterialNodeType.Intrinsic, "SmoothStep");
            Register<SqrtNode>(MaterialNodeType.Intrinsic, "Sqrt");
            Register<StepNode>(MaterialNodeType.Intrinsic, "Step");
            Register<TanNode>(MaterialNodeType.Intrinsic, "Tan");
            Register<TanhNode>(MaterialNodeType.Intrinsic, "Tanh");

            Register<AddNode>(MaterialNodeType.Operator, "Add");
            Register<DivideNode>(MaterialNodeType.Operator, "Divide");
            Register<MultiplyNode>(MaterialNodeType.Operator, "Multiply");
            Register<SubtractNode>(MaterialNodeType.Operator, "Subtract");

            Register<CodeNode>(MaterialNodeType.Custom, "Code");
        }

        public static IReadOnlyList<MaterialNodeFactory> GetAllFactories() => globalFactories;

        public static IReadOnlyList<MaterialNodeFactory> GetFactories(MaterialNodeType type)
        {
            return nodeTypeToFactory[type];
        }

        public static bool TryGetFactories(MaterialNodeType type, [NotNullWhen(true)] out IReadOnlyList<MaterialNodeFactory>? factories)
        {
            var result = nodeTypeToFactory.TryGetValue(type, out var _factories);
            factories = _factories;
            return result;
        }

        [Obsolete("Prefer generic methods over reflection-based ones for better type safety and performance.")]
        public static void Register([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, MaterialNodeType nodeType, string name)
        {
            if (!type.IsAssignableTo(typeof(Node)))
            {
                throw new InvalidOperationException();
            }
            MaterialNodeFactoryReflection factory = new(type, nodeType, name);
            AddFactory(factory);
        }

        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(MaterialNodeType nodeType, string name) where T : Node, new()
        {
            var type = typeof(T);
            if (!type.IsAssignableTo(typeof(Node)))
            {
                throw new InvalidOperationException();
            }
            MaterialNodeFactory<T> factory = new(nodeType, name);
            AddFactory(factory);
        }

        public static void Overwrite<TTarget, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TNode>(MaterialNodeType type, string name) where TTarget : Node, new() where TNode : Node, new()
        {
            Unregister<TTarget>();
            Register<TNode>(type, name);
        }

        public static bool Unregister<T>() where T : Node
        {
            return Unregister(typeof(T));
        }

        public static bool Unregister(Type type)
        {
            if (!type.IsAssignableTo(typeof(Node)))
            {
                throw new InvalidOperationException();
            }
            if (!typeToFactory.TryGetValue(type, out var factory))
            {
                return false;
            }
            if (nodeTypeToFactory.TryGetValue(factory.NodeType, out var factories))
            {
                factories.Remove(factory);
            }
            typeToFactory.Remove(type);
            globalFactories.Remove(factory);
            return true;
        }

        public static void AddFactory(MaterialNodeFactory factory)
        {
            if (!nodeTypeToFactory.TryGetValue(factory.NodeType, out var factories))
            {
                factories = [];
                nodeTypeToFactory.Add(factory.NodeType, factories);
            }
            factories.Add(factory);
            typeToFactory.Add(factory.Type, factory);
            globalFactories.Add(factory);
        }
    }

    public enum MaterialNodeType
    {
        Intrinsic,
        Operator,
        Method,
        Texture,
        Noise,
        Constant,
        Custom,
    }

    public abstract class MaterialNodeFactory
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public abstract Type Type { get; }

        public abstract string Name { get; }

        public abstract MaterialNodeType NodeType { get; }

        public abstract Node CreateInstance();
    }

    public class MaterialNodeFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : MaterialNodeFactory where T : Node, new()
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private readonly Type type = typeof(T);

        private readonly MaterialNodeType nodeType;
        private readonly string name;

        public MaterialNodeFactory(MaterialNodeType nodeType, string name)
        {
            this.nodeType = nodeType;
            this.name = name;
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public override Type Type => type;

        public override MaterialNodeType NodeType => nodeType;

        public override string Name => name;

        public override Node CreateInstance()
        {
            return new T();
        }
    }

    public class MaterialNodeFactoryReflection : MaterialNodeFactory
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private readonly Type type;

        private readonly MaterialNodeType nodeType;
        private readonly string name;

        public MaterialNodeFactoryReflection([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, MaterialNodeType nodeType, string name)
        {
            this.type = type;
            this.nodeType = nodeType;
            this.name = name;
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public override Type Type => type;

        public override MaterialNodeType NodeType => nodeType;

        public override string Name => name;

        public override Node CreateInstance()
        {
            return (Node?)Activator.CreateInstance(type) ?? throw new InvalidOperationException("Failed to create instance.");
        }
    }
}