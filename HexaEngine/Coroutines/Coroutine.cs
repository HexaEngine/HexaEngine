namespace HexaEngine.Coroutines
{
    using System.Collections;

    public class Coroutine
    {
        public readonly IEnumerator Enumerator;

        public Coroutine(IEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        public float Delay { get; internal protected set; }

        public CoroutineStateFlags Flags { get; internal protected set; }

        public void Reset()
        {
            Enumerator.Reset();
        }
    }
}