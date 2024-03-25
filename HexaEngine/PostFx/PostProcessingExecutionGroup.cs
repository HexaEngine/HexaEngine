namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;

    public class PostProcessingExecutionGroup(bool isDynamicGroup, bool isFirst, bool isLastGroup) : IDisposable
    {
        private bool isDynamicGroup = isDynamicGroup;
        private bool isFirst = isFirst;
        private bool isLastGroup = isLastGroup;
        private ICommandList? commandList;
        private PostProcessingExecutionGroup next;

        public List<IPostFx> Passes = [];

        public ICommandList? CommandList => commandList;

        public bool IsFirst { get => isFirst; set => isFirst = value; }

        public bool IsDynamic { get => isDynamicGroup; set => isDynamicGroup = value; }

        public bool IsLast { get => isLastGroup; set => isLastGroup = value; }

        public PostProcessingExecutionGroup Next { get => next; set => next = value; }

        public ICommandList Record(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (isDynamicGroup)
            {
                throw new InvalidOperationException($"Cannot record a dynamic group, that contains conditional render code.");
            }

            context.ClearState();

            Draw(context, creator);

            return context.FinishCommandList(true);
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

                if ((flags & PostFxFlags.Inline) != 0 && isFirst && postContext.IsFirst)
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
                    effect.SetInput(postContext.Previous.SRV, postContext.Previous);
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
                        effect.SetOutput(buffer.RTV, buffer, buffer.Viewport);
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

        public void Execute(IGraphicsContext context, IGraphicsContext deferredContext, GraphResourceBuilder creator)
        {
            if (isDynamicGroup)
            {
                Draw(context, creator);
            }
            else
            {
                commandList ??= Record(deferredContext, creator);
                context.ExecuteCommandList(commandList, false);
            }
        }

        public void Invalidate()
        {
            commandList?.Dispose();
            commandList = null;
        }

        public void Dispose()
        {
            commandList?.Dispose();
            commandList = null;
            Passes.Clear();
        }
    }
}