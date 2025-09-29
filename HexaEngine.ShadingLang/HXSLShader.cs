namespace HexaEngine.ShadingLang
{
    public class HXSLShader : IHXSLName
    {
        public string Name { get; set; }

        [HXSLCodeblock]
        public string Code { get; set; }

        public HXSLShader()
        {
        }
    }
}