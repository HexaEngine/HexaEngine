namespace HexaEngine.Core.Animations
{
    public class AnimatorState
    {
        public Motion Motion { get; set; }

        public float Speed { get; set; }

        public float CycleOffset { get; set; }

        public bool Mirror { get; set; }

        public bool IkOnFeet { get; set; }

        public bool WriteDefaultValues { get; set; }

        public string Tag { get; set; }

        public string SpeedParameter { get; set; }

        public string CycleOffsetParameter { get; set; }

        public string MirrorParameter { get; set; }

        public string TimeParameter { get; set; }

        public bool SpeedParameterActive { get; set; }

        public bool CycleOffsetParameterActive { get; set; }

        public bool MirrorParameterActive { get; set; }

        public bool TimeParameterActive { get; set; }

        public AnimatorStateTransition[] Transitions { get; set; }
    }
}