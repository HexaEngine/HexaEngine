namespace HexaEngine.Core.Extensions
{
    public static class SemaphoreSlimExtensions
    {
        public static async Task<IDisposable> LockAsync(this SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            return new Releaser(semaphore, true);
        }

        public static Releaser Lock(this SemaphoreSlim semaphore)
        {
            semaphore.Wait();
            return new Releaser(semaphore, true);
        }

        public static Releaser TryLock(this SemaphoreSlim semaphore)
        {
            bool result = semaphore.Wait(TimeSpan.Zero);
            return new Releaser(semaphore, result);
        }

        public readonly struct Releaser : IDisposable
        {
            private readonly SemaphoreSlim semaphore;

            public Releaser(SemaphoreSlim semaphore, bool owns)
            {
                this.semaphore = semaphore;
                OwnsLock = owns;
            }

            public bool OwnsLock { get; }

            public readonly void Dispose()
            {
                semaphore.Release();
            }
        }
    }
}
