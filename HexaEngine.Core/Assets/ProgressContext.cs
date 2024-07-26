namespace HexaEngine.Core.Assets
{
    using Hexa.NET.Mathematics;
    using System;

    public class ProgressContext
    {
        private readonly ImportContext context;
        private readonly List<SubStep> subSteps = [];
        private SubStep current;
        private int rootIndex = 0;

        public ProgressContext(ImportContext context, int rootSubSteps)
        {
            this.context = context;

            for (int i = 0; i < rootSubSteps; i++)
            {
                subSteps.Add(new(0, 0));
            }
        }

        private class SubStep
        {
            public int Progress;
            public int MaxProgress;

            public SubStep(int progress, int maxProgress)
            {
                Progress = progress;
                MaxProgress = maxProgress;
            }
        }

        public void BeginSubStep(int itemCount)
        {
            current = subSteps[rootIndex];
            current.Progress = 0;
            current.MaxProgress = itemCount;
        }

        public void EndSubStep()
        {
            rootIndex++;
            if (rootIndex == subSteps.Count)
            {
                context.ReportProgress(1);
                return;
            }
            current = subSteps[rootIndex];
        }

        public void AddProgress()
        {
            current.Progress++;

            float subStepProgress = current.Progress / (float)current.MaxProgress;
            float baseProgress = rootIndex / (float)subSteps.Count;
            float endProgress = (rootIndex + 1) / (float)subSteps.Count;
            float progress = MathUtil.Lerp(baseProgress, endProgress, subStepProgress);

            context.ReportProgress(progress);
        }
    }
}