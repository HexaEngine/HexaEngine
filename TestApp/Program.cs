using HexaEngine.Core;
using HexaEngine.Core.Unsafes;
using HexaEngine.Plugins;
using HexaEngine.Plugins.Records;

namespace TestApp
{
    public class Program
    {
        public static unsafe void Main()
        {
            /*
            List<string> strings = new List<string>();

            while (true)
            {
                string? input = Console.ReadLine();
                if (input == null) continue;
                if (input == "e") break;
                strings.Add(input);
            }
            string inp = string.Join(string.Empty, strings.ToArray());
            string[] nums = inp.Replace(Environment.NewLine, "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int[] ints = nums.Select(int.Parse).ToArray();
            StringBuilder sb = new();
            for (int i = 0; i < ints.Length; i += 2)
            {
                sb.AppendLine($"{ints[i] - 1},{ints[i + 1] - 1},");
            }

            Console.WriteLine(sb.ToString());*/

            Plugin plugin = new();

            PluginHeader pluginHeader = new();
            pluginHeader.Name = Utilities.AsPointer(new UnsafeString("Test"));
            pluginHeader.Version = new(1, 0, 0, 0);
            pluginHeader.Description = Utilities.AsPointer(new UnsafeString(string.Empty));
            pluginHeader.Endianness = BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;
            pluginHeader.FormatVersion = PluginVersion.LatestFormatVersion;

            plugin.Header = &pluginHeader;

            Plugin* pPlugin = &plugin;

            SceneRecord record = new();
            RecordHeader recordHeader = new();
            recordHeader.Id = 1;
            recordHeader.Parent = null;
            recordHeader.Type = RecordType.Scene;
            record.Header = &recordHeader;
            record.Name = Utilities.AsPointer(new UnsafeString("MainScene"));

            Record*[] records = new Record*[1];
            records[0] = (Record*)(void*)&record;

            fixed (Record** pRecords = records)
            {
                pPlugin->Records = pRecords;
            }
            pPlugin->Header->RecordCount = (ulong)records.LongLength;

            // Test for casting
            for (ulong i = 0; i < pPlugin->Header->RecordCount; i++)
            {
                Record* pRecord = pPlugin->Records[i];
                switch (pRecord->Header->Type)
                {
                    case RecordType.None:
                        break;

                    case RecordType.Scene:
                        SceneRecord* sceneRecord = (SceneRecord*)(void*)pRecord;
                        break;

                    case RecordType.Node:
                        break;

                    case RecordType.Camera:
                        break;

                    case RecordType.Terrain:
                        break;

                    case RecordType.Light:
                        break;

                    case RecordType.Mesh:
                        break;

                    case RecordType.Material:
                        break;
                }
            }

            int size = PluginEncoder.ComputePluginSize(pPlugin);
            byte[] bytes = new byte[size];
            PluginEncoder.EncodePlugin(bytes, Endianness.BigEndian, pPlugin);

            Plugin* pPlugin2;
            PluginDecoder.DecodePlugin(bytes, &pPlugin2);

            // Test for casting
            for (ulong i = 0; i < pPlugin2->Header->RecordCount; i++)
            {
                Record* pRecord = pPlugin2->Records[i];
                switch (pRecord->Header->Type)
                {
                    case RecordType.None:
                        break;

                    case RecordType.Scene:
                        SceneRecord* sceneRecord = (SceneRecord*)(void*)pRecord;
                        break;

                    case RecordType.Node:
                        break;

                    case RecordType.Camera:
                        break;

                    case RecordType.Terrain:
                        break;

                    case RecordType.Light:
                        break;

                    case RecordType.Mesh:
                        break;

                    case RecordType.Material:
                        break;
                }
            }
        }
    }
}