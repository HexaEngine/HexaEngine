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
            pluginHeader.Name = Utilities.UTF16("Test Plugin");
            pluginHeader.Version = new(1, 0, 0, 0);
            pluginHeader.Description = Utilities.UTF16(string.Empty);
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
            record.Name = Utilities.UTF16("MainScene");

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
            PluginEncoder.EncodePlugin(bytes, Endianness.LittleEndian, pPlugin);

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
                        string name = new(sceneRecord->Name);
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

            Plugin plugin1 = new();

            PluginHeader pluginHeader1 = new();
            pluginHeader1.Name = Utilities.UTF16("Test Plugin 1");
            pluginHeader1.Version = new(1, 0, 0, 0);
            pluginHeader1.Description = Utilities.UTF16(string.Empty);
            pluginHeader1.Endianness = BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;
            pluginHeader1.FormatVersion = PluginVersion.LatestFormatVersion;
            pluginHeader1.Dependencies = Utilities.GCAlloc<char>(1);
            pluginHeader1.Dependencies[0] = Utilities.UTF16("Test Plugin");
            pluginHeader1.DependencyCount = 1;

            plugin1.Header = &pluginHeader1;

            Plugin* pPlugin1 = &plugin1;

            Plugin** plugins = Utilities.GCAlloc<Plugin>(2);
            plugins[0] = pPlugin;
            plugins[1] = pPlugin1;

            Plugin** sorted;
            ReferenceBuilder.Sort(plugins, 2, &sorted);
        }
    }
}