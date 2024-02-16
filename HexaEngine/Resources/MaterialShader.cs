namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using System.Threading.Tasks;

    public struct MaterialShaderPassDesc
    {
        public string Name;
        public GraphicsPipelineDesc Pipeline;
        public GraphicsPipelineStateDesc State;

        public MaterialShaderPassDesc(string name, GraphicsPipelineDesc pipeline, GraphicsPipelineStateDesc state)
        {
            Name = name;
            Pipeline = pipeline;
            State = state;
        }
    }

    public struct MaterialShaderDesc
    {
        public Guid MaterialId;
        public InputElementDescription[] InputElements;
        public ShaderMacro[] Macros;
        public MaterialShaderPassDesc[] Passes;

        public MaterialShaderDesc(MaterialData data, InputElementDescription[] inputElements, params MaterialShaderPassDesc[] passes)
        {
            MaterialId = data.Guid;
            InputElements = inputElements;
            Macros = data.GetShaderMacros();
            Passes = passes;
        }

        public MaterialShaderDesc(Guid materialId, ShaderMacro[] macros, InputElementDescription[] inputElements, params MaterialShaderPassDesc[] passes)
        {
            MaterialId = materialId;
            InputElements = inputElements;
            Macros = macros;
            Passes = passes;
        }
    }

    public class MaterialShader : ResourceInstance, IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly List<MaterialShaderPass> passes = [];
        private readonly Dictionary<string, MaterialShaderPass> nameToPass = [];
        private MaterialShaderDesc desc;
        private volatile bool initialized;
        private bool disposedValue;

        public MaterialShader(IResourceFactory factory, IGraphicsDevice device, MaterialShaderDesc desc) : base(factory, desc.MaterialId)
        {
            this.device = device;
            this.desc = desc;
        }

        public IReadOnlyList<MaterialShaderPass> Passes => passes;

        public void Initialize()
        {
            Compile();
        }

        private void Compile()
        {
            ShaderMacro[] globalMacros = desc.Macros;
            for (int i = 0; i < desc.Passes.Length; i++)
            {
                MaterialShaderPassDesc passDesc = desc.Passes[i];
                passDesc.State.InputElements = desc.InputElements;
                passDesc.Pipeline.Macros = [.. passDesc.Pipeline.Macros, .. globalMacros];
                MaterialShaderPass pass = new(passDesc.Name, device, passDesc.Pipeline, passDesc.State);
                passes.Add(pass);
                nameToPass.Add(pass.Name, pass);
            }
        }

        public MaterialShaderPass? Find(string name)
        {
            nameToPass.TryGetValue(name, out var pass);
            return pass;
        }

        public int IndexOf(string name)
        {
            for (int i = 0; i < passes.Count; i++)
            {
                if (passes[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Update(ShaderMacro[] globalMacros)
        {
            desc.Macros = globalMacros;
        }

        public void Update(MaterialShaderDesc desc)
        {
            this.desc = desc;
        }

        public void Reload()
        {
            initialized = false;
            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].Dispose();
            }
            passes.Clear();
            nameToPass.Clear();
            Compile();
        }

        public Task ReloadAsync()
        {
            initialized = false;
            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].Dispose();
            }
            passes.Clear();
            nameToPass.Clear();
            Compile();
            return Task.CompletedTask;
        }

        public void Recompile()
        {
            initialized = false;
            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].Dispose();
            }
            passes.Clear();
            nameToPass.Clear();
            Compile();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < passes.Count; i++)
                {
                    passes[i].Dispose();
                }
                passes.Clear();
                disposedValue = true;
            }
        }

        ~MaterialShader()
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