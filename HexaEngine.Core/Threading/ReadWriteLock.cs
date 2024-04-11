namespace HexaEngine.Core.Threading
{
    using System;

    public class ReadWriteLock : IDisposable
    {
        private readonly ManualResetEventSlim writeLock = new(true);
        private readonly ManualResetEventSlim readLock = new(true);
        private readonly int maxReader;
        private readonly int maxWriter;
        private readonly SemaphoreSlim readSemaphore;
        private readonly SemaphoreSlim writeSemaphore;
        private bool disposedValue;

        public ReadWriteLock(int maxReader, int maxWriter)
        {
            this.maxReader = maxReader;
            this.maxWriter = maxWriter;
            readSemaphore = new(maxReader);
            writeSemaphore = new(maxWriter);
        }

        private readonly struct ReadBlock(ReadWriteLock readWriteLock) : IDisposable
        {
            private readonly ReadWriteLock readWriteLock = readWriteLock;

            public void Dispose()
            {
                readWriteLock.EndRead();
            }
        }

        private readonly struct WriteBlock(ReadWriteLock readWriteLock) : IDisposable
        {
            private readonly ReadWriteLock readWriteLock = readWriteLock;

            public void Dispose()
            {
                readWriteLock.EndWrite();
            }
        }

        public void BeginRead()
        {
            readLock.Reset();

            writeLock.Wait();

            readSemaphore.Wait();
        }

        public IDisposable BeginReadBlock()
        {
            BeginRead();
            return new ReadBlock(this);
        }

        public void EndRead()
        {
            readSemaphore.Release();
            if (readSemaphore.CurrentCount == maxReader)
            {
                readLock.Set();
            }
        }

        public void BeginWrite()
        {
            writeLock.Reset();

            readLock.Wait();

            writeSemaphore.Wait();
        }

        public IDisposable BeginWriteBlock()
        {
            BeginWrite();
            return new WriteBlock(this);
        }

        public void EndWrite()
        {
            writeSemaphore.Release();

            if (writeSemaphore.CurrentCount == maxWriter)
            {
                writeLock.Set();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    writeLock.Dispose();
                    readLock.Dispose();
                    writeSemaphore.Dispose();
                    readSemaphore.Dispose();
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