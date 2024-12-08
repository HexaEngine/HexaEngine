namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;

    public class PostProcessingExecutionGroup : IDisposable
    {
        private bool isDynamicGroup;
        private bool isFirst;
        private bool isLastGroup;
        private readonly ICommandBuffer commandBuffer = null!;
        private PostProcessingExecutionGroup next = null!;
        private bool isDirty = true;

        public List<IPostFx> Passes = [];

        public PostProcessingExecutionGroup(IGraphicsDevice device, bool isDynamicGroup, bool isFirst, bool isLastGroup)
        {
            this.isDynamicGroup = isDynamicGroup;
            this.isFirst = isFirst;
            this.isLastGroup = isLastGroup;
            if (!isDynamicGroup)
            {
                commandBuffer = device.CreateCommandBuffer();
            }
        }

        public bool IsFirst { get => isFirst; set => isFirst = value; }

        public bool IsDynamic { get => isDynamicGroup; set => isDynamicGroup = value; }

        public bool IsLast { get => isLastGroup; set => isLastGroup = value; }

        public PostProcessingExecutionGroup Next { get => next; set => next = value; }

        public void Record(GraphResourceBuilder creator)
        {
            if (isDynamicGroup)
            {
                throw new InvalidOperationException($"Cannot record a dynamic group, that contains conditional render code.");
            }

            if (!isDirty)
            {
                return;
            }

            commandBuffer.Begin();

            Draw(commandBuffer, creator);

            commandBuffer.End();
            isDirty = false;
        }

        public void SetupInputOutputs(PostProcessingContext postContext)
        {
            for (int i = 0; i < Passes.Count; i++)
            {
                var effect = Passes[i];

                if (!effect.Enabled)
                {
                    continue;
                }

                var flags = effect.Flags;

                if ((flags & PostFxFlags.Inline) != 0 && postContext.IsFirst)
                {
                    if (i - 1 < 0 || Passes[i - 1] is not CopyPass)
                    {
                        Passes.Insert(i, new CopyPass());
                        i--;
                        continue;
                    }
                }

                if ((flags & PostFxFlags.NoInput) == 0 && (flags & PostFxFlags.ComposeTarget) == 0)
                {
                    effect.SetInput(postContext.Previous.SRV!, postContext.Previous);
                }

                if ((flags & PostFxFlags.NoOutput) == 0 && (flags & PostFxFlags.Compose) == 0)
                {
                    var buffer = postContext.Current;

                    if (i == Passes.Count - 1 && isLastGroup)
                    {
                        effect.SetOutput(postContext.Output, postContext.OutputTex, postContext.OutputViewport);
                    }
                    else
                    {
                        effect.SetOutput(buffer.RTV!, buffer, buffer.Viewport);
                    }

                    bool skipSwap = false;
                    if (i < Passes.Count - 1)
                    {
                        skipSwap = ShouldSkipSwap(Passes[i + 1]);
                    }
                    else if (!isLastGroup)
                    {
                        skipSwap = ShouldSkipSwap(next.Passes[0]);
                    }

                    if (!skipSwap)
                    {
                        postContext.Swap();
                    }

                    postContext.Signal();
                }
            }
        }

        private static bool ShouldSkipSwap(IPostFx fx)
        {
            var flags = fx.Flags;
            return (flags & PostFxFlags.Inline) != 0 || (flags & PostFxFlags.ComposeTarget) != 0;
        }

        private void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            for (int i = 0; i < Passes.Count; i++)
            {
#if DEBUG
                context.BeginEvent(Passes[i].Name);
#endif
                Passes[i].Draw(context);
#if DEBUG
                context.EndEvent();
#endif
            }
        }

        public void Execute(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (isDynamicGroup)
            {
                Draw(context, creator);
            }
            else
            {
                Record(creator);
                context.ExecuteCommandBuffer(commandBuffer);
            }
        }

        public void Invalidate()
        {
            isDirty = true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Passes.Clear();
            commandBuffer.Dispose();
        }
    }
}