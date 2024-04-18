namespace HexaEngine.Coroutines
{
    using HexaEngine.Scenes;
    using System.Collections;
    using System.Collections.Generic;

    public class CoroutineSystem : ISceneSystem
    {
        private readonly object _lock = new();
        private readonly List<Coroutine> coroutines = [];
        private readonly Queue<(int index, Coroutine coroutine)> fixedUpdateCoroutines = [];

        public string Name { get; } = "Coroutine System";

        public SystemFlags Flags { get; } = SystemFlags.Update | SystemFlags.FixedUpdate | SystemFlags.Destroy;

        public void Destroy()
        {
            coroutines.Clear();
            fixedUpdateCoroutines.Clear();
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            lock (_lock)
            {
                Coroutine coroutine = new(enumerator);
                coroutines.Add(coroutine);
                return coroutine;
            }
        }

        public Coroutine StartCoroutine(Coroutine coroutine)
        {
            lock (_lock)
            {
                coroutines.Add(coroutine);
                return coroutine;
            }
        }

        public void StopCoroutine(IEnumerator enumerator)
        {
            lock (_lock)
            {
                for (int i = 0; i < coroutines.Count; i++)
                {
                    if (coroutines[i].Enumerator == enumerator)
                    {
                        coroutines.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            lock (_lock)
            {
                coroutines.Remove(coroutine);
            }
        }

        public void Update(float deltaTime)
        {
            lock (_lock)
            {
                for (int i = coroutines.Count - 1; i >= 0; i--)
                {
                    var coroutine = coroutines[i];

                    if (coroutine.Delay > 0)
                    {
                        coroutine.Delay -= deltaTime;
                    }

                    if (coroutine.Delay > 0)
                    {
                        continue;
                    }

                    TickCoroutine(i, coroutine);
                }
            }
        }

        private void TickCoroutine(int index, Coroutine coroutine)
        {
            if (coroutine.Enumerator.MoveNext())
            {
                var current = coroutine.Enumerator.Current;

                if (current is WaitForSeconds waitForSeconds)
                {
                    coroutine.Delay = waitForSeconds.Seconds;
                    coroutine.Flags = CoroutineStateFlags.Wait;
                }

                if (current is WaitForFixedUpdate)
                {
                    coroutine.Flags = CoroutineStateFlags.WaitFixedUpdate;
                    coroutine.Delay = float.PositiveInfinity; // simply set to inf, it's cheaper then removing an element.
                    fixedUpdateCoroutines.Enqueue((index, coroutine)); // move over to the fixed update queue.
                }
            }
            else
            {
                coroutines.RemoveAt(index);
            }
        }

        public void FixedUpdate()
        {
            lock (_lock)
            {
                while (fixedUpdateCoroutines.TryDequeue(out var result))
                {
                    (int index, Coroutine coroutine) = result;
                    coroutine.Flags = CoroutineStateFlags.WaitFixedUpdate;
                    coroutine.Delay = 0;
                    TickCoroutine(index, coroutine);
                }
            }
        }
    }
}