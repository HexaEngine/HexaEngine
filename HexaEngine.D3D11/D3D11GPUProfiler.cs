namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System.Collections.Generic;
    using Query = Silk.NET.Direct3D11.Query;

    public class D3D11GPUProfiler : IGPUProfiler
    {
        private readonly ComPtr<ID3D11Device5> device;
        private readonly List<string> blockNames = new();
        private readonly Dictionary<string, QueryData>[] queries = new Dictionary<string, QueryData>[FrameCount];
        private readonly Dictionary<string, double> results = new();
        private int currentFrame = 0;
        private bool disposedValue;
        private bool enabled;
        private bool disableLogging = true;
        private const int FrameCount = 3;

        public D3D11GPUProfiler(ComPtr<ID3D11Device5> device)
        {
            this.device = device;
            for (int i = 0; i < FrameCount; i++)
            {
                queries[i] = new();
            }
        }

        public bool Enabled { get => enabled; set => enabled = value; }

        public bool DisableLogging { get => disableLogging; set => disableLogging = value; }

        private class QueryData
        {
            public ComPtr<ID3D11Query> DisjointQuery;
            public ComPtr<ID3D11Query> TimestampQueryStart;
            public ComPtr<ID3D11Query> TimestampQueryEnd;
            public bool BeginCalled, EndCalled;
        };

        public double this[string index]
        {
            get => results[index];
        }

        public unsafe void CreateBlock(string name)
        {
            lock (blockNames)
            {
                blockNames.Add(name);
                results.Add(name, 0);
                for (int i = 0; i < FrameCount; i++)
                {
                    QueryData queryData = new();
                    QueryDesc desc = new(Query.TimestampDisjoint);
                    device.CreateQuery(desc, queryData.DisjointQuery.GetAddressOf());
                    desc = new(Query.Timestamp);
                    device.CreateQuery(desc, queryData.TimestampQueryStart.GetAddressOf());
                    device.CreateQuery(desc, queryData.TimestampQueryEnd.GetAddressOf());
                    queries[i].Add(name, queryData);
                }
            }
        }

        public unsafe void DestroyBlock(string name)
        {
            lock (blockNames)
            {
                blockNames.Remove(name);
                results.Remove(name);
                for (int i = 0; i < FrameCount; i++)
                {
                    QueryData queryData = queries[i][name];
                    queryData.DisjointQuery.Release();
                    queryData.TimestampQueryStart.Release();
                    queryData.TimestampQueryEnd.Release();
                    queries[i].Remove(name);
                }
            }
        }

        public void BeginFrame()
        {
        }

        public unsafe void EndFrame(IGraphicsContext context)
        {
            if (!enabled)
                return;

            if (currentFrame < FrameCount - 1)
            {
                ++currentFrame;
                return;
            }

            var ctx = ((D3D11GraphicsContext)context).DeviceContext;

            int oldIndex = (currentFrame - FrameCount + 1) % FrameCount;
            var oldQueries = queries[oldIndex];

            QueryDataTimestampDisjoint disjoint = default;
            lock (blockNames)
            {
                for (int i = 0; i < blockNames.Count; i++)
                {
                    var name = blockNames[i];
                    var query = oldQueries[name];

                    if (query.BeginCalled && query.EndCalled)
                    {
                        while (ctx.GetData(query.DisjointQuery, null, 0, 0) == 1)
                        {
                            if (!DisableLogging)
                                ImGuiConsole.Log(LogSeverity.Info, $"Waiting for disjoint timestamp of {name} in frame {currentFrame}");
                            Thread.Sleep(1);
                        }

                        ctx.GetData(query.DisjointQuery, &disjoint, (uint)sizeof(QueryDataTimestampDisjoint), 0);
                        if (disjoint.Disjoint)
                        {
                            if (!DisableLogging)
                                ImGuiConsole.Log(LogSeverity.Warning, $"Disjoint Timestamp Flag in {name}");
                        }
                        else
                        {
                            ulong begin = 0;
                            ulong end = 0;

                            ctx.GetData(query.TimestampQueryStart, &begin, sizeof(ulong), 0);

                            while (ctx.GetData(query.TimestampQueryEnd, null, 0, 0) == 1)
                            {
                                if (!DisableLogging)
                                    ImGuiConsole.Log(LogSeverity.Info, $"Waiting for frame end timestamp of {name} in frame {currentFrame}");
                                Thread.Sleep(1);
                            }
                            ctx.GetData(query.TimestampQueryEnd, &end, sizeof(ulong), 0);

                            double delta = (double)(end - begin) / disjoint.Frequency;

                            results[name] = delta;
                        }
                    }

                    query.BeginCalled = false;
                    query.EndCalled = false;
                }
            }
            ++currentFrame;
        }

        public void Begin(IGraphicsContext context, string name)
        {
            if (!enabled)
                return;
            int i = currentFrame % FrameCount;
            var ctx = ((D3D11GraphicsContext)context).DeviceContext;
            if (!queries[i].ContainsKey(name))
                return;
            var queryData = queries[i][name];
            ctx.Begin(queryData.DisjointQuery);
            ctx.End(queryData.TimestampQueryStart);
            queryData.BeginCalled = true;
        }

        public void End(IGraphicsContext context, string name)
        {
            int i = currentFrame % FrameCount;
            var ctx = ((D3D11GraphicsContext)context).DeviceContext;
            if (!queries[i].ContainsKey(name))
                return;
            var queryData = queries[i][name];
            if (!queryData.BeginCalled)
                return;
            ctx.End(queryData.TimestampQueryEnd);
            ctx.End(queryData.DisjointQuery);
            queryData.EndCalled = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < blockNames.Count; i++)
                {
                    DestroyBlock(blockNames[i]);
                }

                disposedValue = true;
            }
        }

        ~D3D11GPUProfiler()
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