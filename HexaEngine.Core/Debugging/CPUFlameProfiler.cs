namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using System.Collections.Generic;
    using System.Diagnostics;

    public unsafe class CPUFlameProfiler : ICPUFlameProfiler
    {
        private readonly ImGuiWidgetFlameGraph.ValuesGetter getter;
        private readonly UnsafeArray<Entry> entries = new(bufferSize);

        private readonly Dictionary<string, uint> nameToId = new();
        private readonly Dictionary<uint, string> idToName = new();
        private readonly Queue<uint> destroyQueue = new();

        private uint id;

        private byte currentLevel;
        private int currentEntry = bufferSize - 1;
        private int lastEntry = 0;

        private const int bufferSize = 1;

        private readonly bool avgResults = true;

        public CPUFlameProfiler()
        {
            getter = ProfilerValueGetter;
        }

        public CPUFlameProfiler(string[] stages)
        {
            getter = ProfilerValueGetter;
            for (int i = 0; i < stages.Length; i++)
            {
                CreateStage(stages[i]);
            }
        }

        public Entry* Current => &entries.Data[currentEntry];

        public int StageCount => nameToId.Count;

        public ImGuiWidgetFlameGraph.ValuesGetter Getter => getter;

        public ref Scope this[int index]
        {
            get { return ref entries[currentEntry].Stages.Data[index]; }
        }

        public ref Scope this[string index]
        {
            get { return ref entries[currentEntry].Stages.Data[nameToId[index]]; }
        }

        double ICPUProfiler.this[string index]
        {
            get { return entries[currentEntry].Stages.Data[nameToId[index]].Duration; }
        }

        public void BeginFrame()
        {
            var prevEntry = entries.GetPointer(currentEntry);
            lastEntry = currentEntry;
            currentEntry = (currentEntry + 1) % bufferSize;
            prevEntry->End = entries.GetPointer(currentEntry)->Start = (ulong)Stopwatch.GetTimestamp();
        }

        public void EndFrame()
        {
            Entry* entry = entries.GetPointer(currentEntry);

            for (int i = 0; i < entry->Stages.Size; i++)
            {
                var stage = entry->Stages.GetPointer(i);
                if (!stage->Used)
                {
                    destroyQueue.Enqueue(stage->Id);
                    continue;
                }
                stage->Used = false;

                var fltStart = stage->Start - entry->Start;
                stage->StartSamples.Add(fltStart / (float)Stopwatch.Frequency * 1000);

                var fltEnd = stage->End - entry->Start;
                stage->EndSamples.Add(fltEnd / (float)Stopwatch.Frequency * 1000);
            }

            while (destroyQueue.TryDequeue(out var id))
            {
                DestroyStage(id);
            }
        }

        public void CreateStage(string name)
        {
            nameToId.Add(name, id);
            idToName.Add(id, name);
            for (uint i = 0; i < bufferSize; i++)
            {
                entries.GetPointer(i)->Stages.PushBack(new(id, name));
            }
            id++;
        }

        public void DestroyStage(string name)
        {
            var id = nameToId[name];
            for (uint i = 0; i < bufferSize; i++)
            {
                var entry = entries.GetPointer(i);
                var index = entry->IdToIndex(id);
                var stage = entry->Stages[index];
                entry->Stages.RemoveAt(index);

                stage.StartSamples.Release();
                stage.EndSamples.Release();
                stage.Name.Release();
            }

            nameToId.Remove(name);
            idToName.Remove(id);
        }

        public void DestroyStage(uint id)
        {
            var name = idToName[id];
            for (uint i = 0; i < bufferSize; i++)
            {
                var entry = entries.GetPointer(i);
                var index = entry->IdToIndex(id);
                var stage = entry->Stages[index];
                entry->Stages.RemoveAt(index);

                stage.StartSamples.Release();
                stage.EndSamples.Release();
                stage.Name.Release();
            }

            nameToId.Remove(name);
            idToName.Remove(id);
        }

        public void Begin(string name)
        {
            Trace.Assert(currentLevel < 255);

            if (!nameToId.ContainsKey(name))
            {
                CreateStage(name);
            }

            var entry = entries[currentEntry];

            var index = entry.IdToIndex(nameToId[name]);
            var stage = entry.Stages.GetPointer(index);

            stage->Level = currentLevel;
            currentLevel++;
            stage->Start = (ulong)Stopwatch.GetTimestamp();
            stage->Finalized = false;
            stage->Used = true;
        }

        public void End(string name)
        {
            Trace.Assert(currentLevel > 0);

            if (!nameToId.ContainsKey(name))
            {
                CreateStage(name);
            }

            var entry = entries[currentEntry];

            var index = entry.IdToIndex(nameToId[name]);
            var stage = entry.Stages.GetPointer(index);

            Trace.Assert(!stage->Finalized);
            currentLevel--;
            Trace.Assert(currentLevel == stage->Level);
            stage->End = (ulong)Stopwatch.GetTimestamp();
            stage->Duration = (stage->End - stage->Start) / (double)Stopwatch.Frequency;
            stage->Finalized = true;
        }

        public int GetCurrentEntryIndex()
        {
            return (currentEntry + bufferSize - 1) % bufferSize;
        }

        private void ProfilerValueGetter(float* startTimestamp, float* endTimestamp, byte* level, byte** caption, void* data, int idx)
        {
            var entry = (Entry*)data;
            var stage = entry->Stages[idx];

            if (startTimestamp != null)
            {
                if (avgResults)
                {
                    *startTimestamp = stage.StartSamples.HeadValue;
                }
                else
                {
                    var fltStart = stage.Start - entry->Start;
                    *startTimestamp = fltStart / (float)Stopwatch.Frequency * 1000;
                }
            }
            if (endTimestamp != null)
            {
                *endTimestamp = stage.End;

                if (avgResults)
                {
                    *endTimestamp = stage.EndSamples.HeadValue;
                }
                else
                {
                    var fltEnd = stage.End - entry->Start;
                    *endTimestamp = fltEnd / (float)Stopwatch.Frequency * 1000;
                }
            }
            if (level != null)
            {
                *level = stage.Level;
            }
            if (caption != null)
            {
                *caption = stage.Name;
            }
        }
    }
}