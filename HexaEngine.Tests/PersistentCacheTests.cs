namespace HexaEngine.Tests
{
    using HexaEngine.Core.IO.Caching;
    using HexaEngine.Core.Unsafes;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    using NUnit.Framework;


    public unsafe class PersistentCacheTests
    {
        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory("./temp");
        }

        private static PersistentCache CreateTempCache()
        {
            string tempDir = GetTempDir();
            return new PersistentCache(tempDir);
        }

        private static string GetTempDir()
        {
            string tempDir = $"./temp/{Guid.NewGuid()}";
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        private static byte[] GenerateTestData(int size)
        {
            byte[] data = new byte[size];
            RandomNumberGenerator.Fill(data);
            return data;
        }

        private static byte[] GenerateTestData()
        {
            byte[] data = new byte[RandomNumberGenerator.GetInt32(1024) + 1];
            RandomNumberGenerator.Fill(data);
            return data;
        }

        private static void CheckData(PersistentCache cache, string keyName, byte[] testData)
        {
            byte* retrievedData = null;
            uint retrievedSize = 0;

            cache.Get(keyName, &retrievedData, &retrievedSize);

            Assert.That((Pointer)retrievedData, Is.Not.EqualTo(new Pointer(null)));
            Assert.That(retrievedSize, Is.EqualTo((uint)testData.Length));

            byte[] retrievedBytes = new byte[retrievedSize];
            for (int i = 0; i < retrievedSize; i++)
            {
                retrievedBytes[i] = retrievedData[i];
            }

            if (retrievedData != null)
            {
                Marshal.FreeHGlobal((nint)retrievedData);
            }

            Assert.That(testData, Is.EqualTo(retrievedBytes));
        }

        [Test]
        public void SetAndGet()
        {
            var cache = CreateTempCache();

            byte[] testData = GenerateTestData();

            cache.Set("TestKey", testData);
            CheckData(cache, "TestKey", testData);

            cache.Dispose();
        }

        [Test]
        public void SetAndGetMultiData()
        {
            var cache = CreateTempCache();

            byte[] testData1 = GenerateTestData();
            byte[] testData2 = GenerateTestData();
            byte[] testData3 = GenerateTestData();

            cache.Set("TestKey1", testData1);
            cache.Set("TestKey2", testData2);
            cache.Set("TestKey3", testData3);

            CheckData(cache, "TestKey1", testData1);
            CheckData(cache, "TestKey2", testData2);
            CheckData(cache, "TestKey3", testData3);

            cache.Dispose();
        }

        [Test]
        public void TryGet_WithInvalidKey()
        {
            var cache = CreateTempCache();

            byte* data = null;
            uint size = 0;

            bool result = cache.TryGet("InvalidKey", &data, &size);

            Assert.That(result, Is.False);
            Assert.That((Pointer)data, Is.EqualTo(new Pointer(null)));
            Assert.That(size, Is.EqualTo(0));

            cache.Dispose();
        }

        [Test]
        public void Set_WithOverMemoryLimit()
        {
            var cache = CreateTempCache();

            // Simulate a cache with a small memory limit
            cache.MaxMemorySize = 10;

            // Allocate large data
            byte[] testData = GenerateTestData(20);

            // Ensure that the cache handles exceeding memory limit gracefully
            cache.Set("LargeData", testData);

            CheckData(cache, "LargeData", testData);

            cache.Dispose();
        }

        [Test]
        public void PersistenceTest()
        {
            string tempDir = GetTempDir();
            var cache = new PersistentCache(tempDir);

            byte[] testData = GenerateTestData();

            cache.Set("TestKey", testData);

            cache.Dispose();

            cache = new PersistentCache(tempDir);

            CheckData(cache, "TestKey", testData);

            cache.Dispose();
        }

        [Test]
        public void PersistenceTestMultiData()
        {
            string tempDir = GetTempDir();
            var cache = new PersistentCache(tempDir);

            byte[] testData1 = GenerateTestData();
            byte[] testData2 = GenerateTestData();
            byte[] testData3 = GenerateTestData();

            cache.Set("TestKey1", testData1);
            cache.Set("TestKey2", testData2);
            cache.Set("TestKey3", testData3);

            cache.Dispose();

            cache = new PersistentCache(tempDir);
            CheckData(cache, "TestKey1", testData1);
            CheckData(cache, "TestKey2", testData2);
            CheckData(cache, "TestKey3", testData3);

            cache.Dispose();
        }

        private static readonly string MultiThreadTempPath = $"./temp/m";

        [Test]
        public void MultiThreadTest()
        {
            var cache = new PersistentCache(MultiThreadTempPath);
            cache.Clear();

            byte[] testData1 = GenerateTestData();
            byte[] testData2 = GenerateTestData();
            byte[] testData3 = GenerateTestData();

            cache.Set("TestKey1", testData1);

            var task1 = Task.Run(() =>
            {
                CheckData(cache, "TestKey1", testData1);
            });

            var task2 = Task.Run(() =>
            {
                cache.Set("TestKey2", testData2);
            });

            var task3 = Task.Run(() =>
            {
                cache.Set("TestKey3", testData3);
            });

            var task4 = Task.Run(() =>
            {
                task3.Wait();
                CheckData(cache, "TestKey3", testData3);
            });

            var task5 = Task.Run(() =>
            {
                task2.Wait();
                CheckData(cache, "TestKey2", testData2);
            });

            var tasks = new Task[] { task1, task2, task3, task4, task5 };

            Task.WaitAll(tasks);

            cache.Dispose();
        }

        [Test]
        public void MemoryCacheTest()
        {
            var cache = CreateTempCache();

            // Simulate a cache with a small memory limit
            cache.MaxMemorySize = 512;

            // Allocate large data
            byte[] testData1 = GenerateTestData(256);
            byte[] testData2 = GenerateTestData(256);
            byte[] testData3 = GenerateTestData(256);

            // Ensure that the cache handles exceeding memory limit gracefully
            cache.Set("LargeData1", testData1);
            cache.Set("LargeData2", testData2);
            cache.Set("LargeData3", testData3);

            CheckData(cache, "LargeData1", testData1);
            CheckData(cache, "LargeData2", testData2);
            CheckData(cache, "LargeData3", testData3);

            cache.Dispose();
        }

        [Test]
        public void SetAndGetAndOverwriteMultiData()
        {
            var cache = CreateTempCache();

            byte[] testData1 = GenerateTestData();
            byte[] testData2 = GenerateTestData();
            byte[] testData3 = GenerateTestData();

            cache.Set("TestKey1", testData1);
            cache.Set("TestKey2", testData2);
            cache.Set("TestKey3", testData3);

            CheckData(cache, "TestKey1", testData1);
            CheckData(cache, "TestKey2", testData2);
            CheckData(cache, "TestKey3", testData3);

            byte[] testData4 = GenerateTestData();
            byte[] testData5 = GenerateTestData();
            byte[] testData6 = GenerateTestData();

            cache.Set("TestKey1", testData4);
            cache.Set("TestKey2", testData5);
            cache.Set("TestKey3", testData6);

            CheckData(cache, "TestKey1", testData4);
            CheckData(cache, "TestKey2", testData5);
            CheckData(cache, "TestKey3", testData6);

            cache.Dispose();
        }

        [Test]
        public void SetAndGetAndOverwritePersistent()
        {
            string tempDir = GetTempDir();
            var cache = new PersistentCache(tempDir);

            byte[] testData1 = GenerateTestData();

            cache.Set("TestKey1", testData1);

            cache.Dispose();
            cache = new PersistentCache(tempDir);

            CheckData(cache, "TestKey1", testData1);

            byte[] testData4 = GenerateTestData();

            cache.Set("TestKey1", testData4);

            cache.Dispose();
            cache = new PersistentCache(tempDir);

            CheckData(cache, "TestKey1", testData4);

            cache.Dispose();
        }

        [Test]
        public void SetAndGetAndOverwritePersistentMultiData()
        {
            string tempDir = GetTempDir();
            var cache = new PersistentCache(tempDir);

            byte[] testData1 = GenerateTestData();
            byte[] testData2 = GenerateTestData();
            byte[] testData3 = GenerateTestData();

            cache.Set("TestKey1", testData1);
            cache.Set("TestKey2", testData2);
            cache.Set("TestKey3", testData3);

            cache.Dispose();
            cache = new PersistentCache(tempDir);

            CheckData(cache, "TestKey1", testData1);
            CheckData(cache, "TestKey2", testData2);
            CheckData(cache, "TestKey3", testData3);

            byte[] testData4 = GenerateTestData();
            byte[] testData5 = GenerateTestData();
            byte[] testData6 = GenerateTestData();

            cache.Set("TestKey1", testData4);
            cache.Set("TestKey2", testData5);
            cache.Set("TestKey3", testData6);

            cache.Dispose();
            cache = new PersistentCache(tempDir);

            CheckData(cache, "TestKey1", testData4);
            CheckData(cache, "TestKey2", testData5);
            CheckData(cache, "TestKey3", testData6);

            cache.Dispose();
        }
    }
}