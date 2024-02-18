﻿namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using System.Threading.Tasks;

    public class MaterialShader : ResourceInstance, IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly List<MaterialShaderPass> passes = [];
        private readonly Dictionary<string, MaterialShaderPass> nameToPass = [];
        private MaterialShaderDesc desc;
        private volatile bool initialized;

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
            if (initialized)
            {
                return;
            }
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
            initialized = true;
        }

        public MaterialShaderPass? Find(string name)
        {
            if (!initialized)
                return null;
            nameToPass.TryGetValue(name, out var pass);
            return pass;
        }

        public int IndexOf(string name)
        {
            if (!initialized)
                return -1;

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
            ReleaseResources();
            Compile();
        }

        public Task ReloadAsync()
        {
            ReleaseResources();
            Compile();
            return Task.CompletedTask;
        }

        protected override void ReleaseResources()
        {
            initialized = false;
            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].Dispose();
            }
            passes.Clear();
            nameToPass.Clear();
        }
    }
}