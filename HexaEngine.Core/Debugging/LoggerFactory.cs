namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Threading;
    using System;
    using System.Collections.Concurrent;

    public class LoggerFactory
    {
        private static readonly ConcurrentDictionary<string, ILogger> loggers = new();
        private static readonly ReadWriteLock readWriteLock = new(int.MaxValue, 1);

        private static readonly List<ILogWriter> globalWriters = new();

        public static readonly ILogger General = GetLogger(nameof(General));

        public static IEnumerable<ILogWriter> GetGlobalWriters()
        {
            return new ReadWriteLockEnumerable<ILogWriter>(readWriteLock, globalWriters);
        }

        public static void AddGlobalWriter(ILogWriter writer)
        {
            using (readWriteLock.BeginWriteBlock())
            {
                globalWriters.Add(writer);
            }
        }

        public static bool RemoveGlobalWriter(ILogWriter writer)
        {
            lock (readWriteLock.BeginWriteBlock())
            {
                return globalWriters.Remove(writer);
            }
        }

        public static void ClearGlobalWriters()
        {
            lock (readWriteLock.BeginWriteBlock())
            {
                globalWriters.Clear();
            }
        }

        public static ILogger GetLogger(string name)
        {
            return loggers.GetOrAdd(name, name => new Logger(name));
        }

        public static T GetLogger<T>(string name) where T : ILogger, new()
        {
            return (T)loggers.GetOrAdd(name, name => { T t = new() { Name = name }; return t; });
        }

        public static bool DestroyLogger(string name)
        {
            return loggers.TryRemove(name, out _);
        }

        public static void DestroyAll()
        {
            loggers.Clear();
        }

        public static void CloseAll()
        {
            foreach (var logger in loggers.Values)
            {
                logger.Close();
            }

            readWriteLock.BeginWrite();
            foreach (var writer in globalWriters)
            {
                writer.Dispose();
            }
            globalWriters.Clear();
            readWriteLock.EndWrite();
        }
    }
}