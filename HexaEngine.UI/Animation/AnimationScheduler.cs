namespace HexaEngine.UI.Animation
{
    public class AnimationScheduler
    {
        private static readonly List<IAnimatable> animatables = new();
        private static readonly Queue<int> completedQueue = new();
        private static readonly Lock _lock = new();

        public static void AddAnimatable(IAnimatable animatable)
        {
            lock (_lock)
            {
                animatables.Add(animatable);
            }
        }

        public static void RemoveAnimatable(IAnimatable animatable)
        {
            lock (_lock)
            {
                animatables.Remove(animatable);
            }
        }

        public static void Tick()
        {
            var deltaTime = Time.Delta;
            lock (_lock)
            {
                for (int i = 0; i < animatables.Count; i++)
                {
                    IAnimatable animatable = animatables[i];
                }

                while (completedQueue.TryDequeue(out var index))
                {
                    animatables.RemoveAt(index);
                }
            }
        }
    }
}