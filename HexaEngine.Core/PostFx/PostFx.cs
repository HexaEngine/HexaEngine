namespace HexaEngine.Core.PostFx
{
    using HexaEngine.Core.Fx;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public enum FxPrimitive
    {
        Undefined,
        Quad,
        Sphere,
        Cube
    }

    public class PostFx : IPostFx
    {
        public FxPrimitive Primitive { get; set; }

        public PrimitiveTopology Topology { get; set; }

        public string Name { get; set; }

        public PostFxFlags Flags { get; set; }

        [XmlIgnore]
        public bool Enabled { get; set; }

        [XmlIgnore]
        public int Priority { get; set; }

        public PostFxPassCollection Passes { get; set; } = new();

        public void Save(string path)
        {
            XmlSerializer serializer = new(typeof(PostFx));
            var fs = File.Create(path);
            serializer.Serialize(fs, this);
            fs.Close();
        }

        public static PostFx Load(string path)
        {
            XmlSerializer serializer = new(typeof(PostFx));
            var fs = File.OpenRead(path);
            var result = serializer.Deserialize(fs);
            fs.Close();
            return (PostFx)result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void SetInput(IShaderResourceView view)
        {
            throw new NotImplementedException();
        }

        public void SetOutput(IRenderTargetView view, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class PostFxPassCollection : List<IPostFxPass>, IXmlSerializable
    {
        private static readonly Dictionary<Type, XmlSerializer> _cache = new();

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Passes");

            bool iter = true;
            while (iter)
            {
                switch (reader.Name)
                {
                    case nameof(GraphicsPass):
                        {
                            XmlSerializer xmlSerializer = new(typeof(GraphicsPass));
                            Add((IPostFxPass)xmlSerializer.Deserialize(reader));
                        }
                        break;

                    case nameof(ComputePass):
                        {
                            XmlSerializer xmlSerializer = new(typeof(ComputePass));
                            Add((IPostFxPass)xmlSerializer.Deserialize(reader));
                        }
                        break;

                    default:
                        iter = false;
                        break;
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (IPostFxPass dispatcher in this)
            {
                var type = dispatcher.GetType();

                if (!_cache.TryGetValue(type, out var xmlSerializer))
                {
                    xmlSerializer = new(dispatcher.GetType());
                    _cache.Add(type, xmlSerializer);
                }

                xmlSerializer.Serialize(writer, dispatcher);
            }
        }
    }

    public interface IPostFxPass : IDisposable
    {
        public List<string> SamplerStates { get; set; }

        public List<string> ConstantBufferViews { get; set; }

        public List<string> ShaderResourceViews { get; set; }

        public List<string> UnorderedAccessViews { get; set; }

        public string? OutputBuffer { get; set; }

        public Task Initialize(IGraphicsDevice device);

        public void Update(IGraphicsContext context);

        public void Render(IGraphicsContext context);
    }

    public class GraphicsPass : IPostFxPass
    {
        private unsafe void** cbvs;
        private unsafe void** srvs;

        public string? OutputBuffer { get; set; }

        public List<string> SamplerStates { get; set; } = new();

        public List<string> ConstantBufferViews { get; set; } = new();

        public List<string> ShaderResourceViews { get; set; } = new();

        public List<string> UnorderedAccessViews { get; set; } = new();

        public GraphicsPipelineDesc Description { get; set; } = default;

        public GraphicsPipelineState State { get; set; } = GraphicsPipelineState.Default;

        public Task Initialize(IGraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void Render(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class ComputePass : IPostFxPass
    {
        public List<string> SamplerStates { get; set; } = new();

        public List<string> ConstantBufferViews { get; set; } = new();

        public List<string> ShaderResourceViews { get; set; } = new();

        public List<string> UnorderedAccessViews { get; set; } = new();

        public string? OutputBuffer { get; set; }

        public ComputePipelineDesc Description { get; set; } = default;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task Initialize(IGraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void Render(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}