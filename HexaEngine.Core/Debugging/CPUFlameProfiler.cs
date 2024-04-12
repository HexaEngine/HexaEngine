namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A CPU flame profiler for measuring and visualizing execution time of various stages.
    /// </summary>
    public unsafe class CPUFlameProfiler : ICPUFlameProfiler
    {
        private readonly ImGuiWidgetFlameGraph.ValuesGetter getter;
        private readonly UnsafeList<ProfilerEntry> entries = new(bufferSize);

        private readonly Dictionary<string, uint> nameToId = new();
        private readonly Dictionary<uint, string> idToName = new();
        private readonly Queue<uint> destroyQueue = new();

        private uint id;

        private byte currentLevel;
        private int currentEntry = bufferSize - 1;
        private int lastEntry = 0;

        private const int bufferSize = 1;

        private readonly bool avgResults = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CPUFlameProfiler"/> class.
        /// </summary>
        public CPUFlameProfiler()
        {
            getter = ProfilerValueGetter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CPUFlameProfiler"/> class with a list of predefined stages.
        /// </summary>
        /// <param name="stages">An array of stage names to be pre-created.</param>
        public CPUFlameProfiler(string[] stages)
        {
            getter = ProfilerValueGetter;
            for (int i = 0; i < stages.Length; i++)
            {
                CreateStage(stages[i]);
            }
        }

        /// <summary>
        /// Gets the current profiling entry.
        /// </summary>
        public ProfilerEntry* Current => &entries.Data[currentEntry - 1 < 0 ? bufferSize - 1 : currentEntry - 1];

        /// <summary>
        /// Gets the number of profiling stages created.
        /// </summary>
        public int StageCount => nameToId.Count;

        /// <summary>
        /// Gets the flame graph values getter for ImGui visualization.
        /// </summary>
        public ImGuiWidgetFlameGraph.ValuesGetter Getter => getter;

        /// <summary>
        /// Gets or sets a scope by index.
        /// </summary>
        /// <param name="index">The index of the scope to get or set.</param>
        public ref ProfilerScope this[int index]
        {
            get
            {
                return ref entries[currentEntry].Stages.Data[index];
            }
        }

        /// <summary>
        /// Gets or sets a scope by name.
        /// </summary>
        /// <param name="index">The name of the scope to get or set.</param>
        public ref ProfilerScope this[string index]
        {
            get
            {
                return ref entries[currentEntry].Stages.Data[nameToId[index]];
            }
        }

        /// <inheritdoc/>
        double ICPUProfiler.this[string index]
        {
            get
            {
                if (nameToId.TryGetValue(index, out var value))
                {
                    return entries[currentEntry].Stages.Data[value].Duration;
                }
                return 0;
            }
        }

        /// <summary>
        /// Begins a new profiling frame, clearing all previous stage data.
        /// </summary>
        public void BeginFrame()
        {
            var prevEntry = entries.GetPointer(currentEntry);
            lastEntry = currentEntry;
            currentEntry = (currentEntry + 1) % bufferSize;
            prevEntry->End = entries.GetPointer(currentEntry)->Start = (ulong)Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Ends the current profiling frame, processing stage data.
        /// </summary>
        public void EndFrame()
        {
            ProfilerEntry* entry = entries.GetPointer(currentEntry);

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

        /// <summary>
        /// Creates a new profiling stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the stage to create.</param>
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

        /// <summary>
        /// Destroys a profiling stage by name.
        /// </summary>
        /// <param name="name">The name of the stage to destroy.</param>
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

        /// <summary>
        /// Destroys a profiling stage by ID.
        /// </summary>
        /// <param name="id">The ID of the stage to destroy.</param>
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

        /// <summary>
        /// Begins profiling a stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the stage to begin profiling.</param>
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

        /// <summary>
        /// Ends profiling a stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the stage to end profiling.</param>
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

        /// <summary>
        /// Gets the index of the previous entry in the circular buffer.
        /// </summary>
        /// <returns>The index of the previous entry.</returns>
        public int GetCurrentEntryIndex()
        {
            return (currentEntry + bufferSize - 1) % bufferSize;
        }

        private void ProfilerValueGetter(float* startTimestamp, float* endTimestamp, byte* level, byte** caption, void* data, int idx)
        {
            var entry = (ProfilerEntry*)data;
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
                *caption = stage.Name.Data;
            }
        }
    }
}