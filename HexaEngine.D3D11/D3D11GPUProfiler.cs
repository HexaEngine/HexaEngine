namespace HexaEngine.D3D11
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;
    using Query = Hexa.NET.D3D11.Query;

    public class D3D11GPUProfiler : IGPUProfiler
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(D3D11));
        private readonly ComPtr<ID3D11Device5> device;
        private readonly ComPtr<ID3D11DeviceContext4> deviceContext;
        private readonly List<string> blockNames = new();
        private readonly Dictionary<string, QueryData>[] queries = new Dictionary<string, QueryData>[FrameCount];
        private readonly Dictionary<string, double> results = new();
        private int currentFrame = 0;
        private bool disposedValue;
        private bool enabled;
        private bool disableLogging = false;
        private const int FrameCount = 3;

        public D3D11GPUProfiler(ComPtr<ID3D11Device5> device, ComPtr<ID3D11DeviceContext4> deviceContext)
        {
            this.device = device;
            this.deviceContext = deviceContext;
            for (int i = 0; i < FrameCount; i++)
            {
                queries[i] = new();
            }
        }

        public bool Enabled { get => enabled; set => enabled = value; }

        public bool DisableLogging { get => disableLogging; set => disableLogging = value; }

        public IReadOnlyList<string> BlockNames => blockNames;

        private class QueryData
        {
            public ComPtr<ID3D11Query> DisjointQuery;
            public ComPtr<ID3D11Query> TimestampQueryStart;
            public ComPtr<ID3D11Query> TimestampQueryEnd;
            public bool BeginCalled, EndCalled;
        };

        public double this[string index]
        {
            get
            {
                if (results.TryGetValue(index, out var value))
                {
                    return value;
                }
                return -1;
            }
        }

        void IGPUProfiler.CreateBlock(string name)
        {
            CreateBlock(name);
        }

        private unsafe QueryData CreateBlock(string name)
        {
            lock (blockNames)
            {
                blockNames.Add(name);
                results.Add(name, 0);
                for (int i = 0; i < FrameCount; i++)
                {
                    QueryData queryData = new();
                    QueryDesc desc = new(Query.TimestampDisjoint);
                    device.CreateQuery(&desc, out queryData.DisjointQuery);
                    desc = new(Query.Timestamp);
                    device.CreateQuery(&desc, out queryData.TimestampQueryStart);
                    device.CreateQuery(&desc, out queryData.TimestampQueryEnd);
                    queries[i].Add(name, queryData);
                }

                return queries[currentFrame % FrameCount][name];
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
            {
                return;
            }

            if (currentFrame < FrameCount - 1)
            {
                ++currentFrame;
                return;
            }

            var ctx = ((D3D11GraphicsContext)context).DeviceContext;

            int index = currentFrame % FrameCount;
            int oldIndex = (currentFrame - FrameCount + 1) % FrameCount;
            var currentQueries = queries[index];
            var oldQueries = queries[oldIndex];

            QueryDataTimestampDisjoint disjoint = default;
            lock (blockNames)
            {
                // auto-cleanup stages.
                for (int i = blockNames.Count - 1; i >= 0; i--)
                {
                    var name = blockNames[i];
                    var query = currentQueries[name];
                    if (!query.BeginCalled || !query.EndCalled)
                    {
                        DestroyBlock(name);
                    }
                }

                for (int i = 0; i < blockNames.Count; i++)
                {
                    var name = blockNames[i];
                    var query = oldQueries[name];

                    if (query.BeginCalled && query.EndCalled)
                    {
                        while (ctx.GetData(query.DisjointQuery.As<ID3D11Asynchronous>(), null, 0, 0) == 1)
                        {
                            if (!DisableLogging)
                            {
                                Logger.Warn($"Waiting for disjoint timestamp of {name} in frame {currentFrame}");
                            }

                            Thread.Sleep(1);
                        }

                        ctx.GetData(query.DisjointQuery.As<ID3D11Asynchronous>(), &disjoint, (uint)sizeof(QueryDataTimestampDisjoint), 0);
                        if (disjoint.Disjoint)
                        {
                            if (!DisableLogging)
                            {
                                Logger.Warn($"Disjoint Timestamp Flag in {name}");
                            }
                        }
                        else
                        {
                            ulong begin = 0;
                            ulong end = 0;

                            ctx.GetData(query.TimestampQueryStart.As<ID3D11Asynchronous>(), &begin, sizeof(ulong), 0);

                            while (ctx.GetData(query.TimestampQueryEnd.As<ID3D11Asynchronous>(), null, 0, 0) == 1)
                            {
                                if (!DisableLogging)
                                {
                                    Logger.Warn($"Waiting for frame end timestamp of {name} in frame {currentFrame}");
                                }

                                Thread.Sleep(1);
                            }
                            ctx.GetData(query.TimestampQueryEnd.As<ID3D11Asynchronous>(), &end, sizeof(ulong), 0);

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
            {
                return;
            }

            int i = currentFrame % FrameCount;
            var ctx = ((D3D11GraphicsContext)context).DeviceContext;
            if (!queries[i].TryGetValue(name, out QueryData? value))
            {
                value = CreateBlock(name);
            }

            var queryData = value;
            ctx.Begin(queryData.DisjointQuery.As<ID3D11Asynchronous>());
            ctx.End(queryData.TimestampQueryStart.As<ID3D11Asynchronous>());
            queryData.BeginCalled = true;
        }

        public void End(IGraphicsContext context, string name)
        {
            int i = currentFrame % FrameCount;
            var ctx = ((D3D11GraphicsContext)context).DeviceContext;
            if (!queries[i].TryGetValue(name, out QueryData? value))
            {
                return;
            }

            var queryData = value;
            if (!queryData.BeginCalled)
            {
                return;
            }

            ctx.End(queryData.TimestampQueryEnd.As<ID3D11Asynchronous>());
            ctx.End(queryData.DisjointQuery.As<ID3D11Asynchronous>());
            queryData.EndCalled = true;
        }

        public unsafe void EndFrame()
        {
            if (!enabled)
            {
                return;
            }

            if (currentFrame < FrameCount - 1)
            {
                ++currentFrame;
                return;
            }

            int index = currentFrame % FrameCount;
            int oldIndex = (currentFrame - FrameCount + 1) % FrameCount;
            var currentQueries = queries[index];
            var oldQueries = queries[oldIndex];

            QueryDataTimestampDisjoint disjoint = default;
            lock (blockNames)
            {
                // auto-cleanup stages.
                for (int i = blockNames.Count - 1; i >= 0; i--)
                {
                    var name = blockNames[i];
                    var query = currentQueries[name];
                    if (!query.BeginCalled || !query.EndCalled)
                    {
                        DestroyBlock(name);
                    }
                }

                for (int i = 0; i < blockNames.Count; i++)
                {
                    var name = blockNames[i];
                    var query = oldQueries[name];

                    if (query.BeginCalled && query.EndCalled)
                    {
                        while (deviceContext.GetData(query.DisjointQuery.As<ID3D11Asynchronous>(), null, 0, 0) == 1)
                        {
                            if (!DisableLogging)
                            {
                                Logger.Warn($"Waiting for disjoint timestamp of {name} in frame {currentFrame}");
                            }

                            Thread.Sleep(1);
                        }

                        deviceContext.GetData(query.DisjointQuery.As<ID3D11Asynchronous>(), &disjoint, (uint)sizeof(QueryDataTimestampDisjoint), 0);
                        if (disjoint.Disjoint)
                        {
                            if (!DisableLogging)
                            {
                                Logger.Warn($"Disjoint Timestamp Flag in {name}");
                            }
                        }
                        else
                        {
                            ulong begin = 0;
                            ulong end = 0;

                            deviceContext.GetData(query.TimestampQueryStart.As<ID3D11Asynchronous>(), &begin, sizeof(ulong), 0);

                            while (deviceContext.GetData(query.TimestampQueryEnd.As<ID3D11Asynchronous>(), null, 0, 0) == 1)
                            {
                                if (!DisableLogging)
                                {
                                    Logger.Warn($"Waiting for frame end timestamp of {name} in frame {currentFrame}");
                                }

                                Thread.Sleep(1);
                            }
                            deviceContext.GetData(query.TimestampQueryEnd.As<ID3D11Asynchronous>(), &end, sizeof(ulong), 0);

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

        public void Begin(string name)
        {
            if (!enabled)
            {
                return;
            }

            int i = currentFrame % FrameCount;
            if (!queries[i].TryGetValue(name, out QueryData? value))
            {
                value = CreateBlock(name);
            }

            var queryData = value;
            deviceContext.Begin(queryData.DisjointQuery.As<ID3D11Asynchronous>());
            deviceContext.End(queryData.TimestampQueryStart.As<ID3D11Asynchronous>());
            queryData.BeginCalled = true;
        }

        public void End(string name)
        {
            int i = currentFrame % FrameCount;
            if (!queries[i].TryGetValue(name, out QueryData? value))
            {
                return;
            }

            var queryData = value;
            if (!queryData.BeginCalled)
            {
                return;
            }

            deviceContext.End(queryData.TimestampQueryEnd.As<ID3D11Asynchronous>());
            deviceContext.End(queryData.DisjointQuery.As<ID3D11Asynchronous>());
            queryData.EndCalled = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                var names = blockNames.ToArray();
                for (int i = 0; i < names.Length; i++)
                {
                    DestroyBlock(names[i]);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}