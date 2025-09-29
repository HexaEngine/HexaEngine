using HexaEngine.Materials.Generator;

namespace HexaEngine.ShadingLang
{
    public class HXSLProperty : IHXSLName
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Default { get; set; }

        public HXSLProperty()
        {
        }

        public SType ToSType()
        {
            var type = SType.Parse(Type);
            if (type.IsStruct)
            {
                throw new Exception($"Invalid type for shader property '{Name}'.");
            }
            return type;
        }
    }
}