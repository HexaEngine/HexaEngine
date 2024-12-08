namespace HexaEngine.Animations
{
    public class AnimatorState
    {
        public IMotion Motion { get; set; } = null!;

        public float Speed { get; set; }

        public int CycleOffset { get; set; }
    }
}