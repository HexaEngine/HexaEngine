namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using HexaEngine.Scenes;

    public class ScriptGroup
    {
        private readonly Dictionary<Type, int> map = [];
        private readonly List<ScriptNode> nodes;
        private readonly bool parallel;
        private readonly FlaggedList<ScriptFlags, IScriptComponent> instances = [];
        private readonly object _lock = new();

        public ScriptGroup(List<ScriptNode> nodes, bool parallel)
        {
            this.nodes = nodes;
            this.parallel = parallel;
            for (int i = 0; i < nodes.Count; i++)
            {
                map.Add(nodes[i].ScriptType, i);
            }
        }

        public bool AddInstance(IScriptComponent component)
        {
            if (component.ScriptType == null) return false;
            lock (_lock)
            {
                if (map.TryGetValue(component.ScriptType, out int index))
                {
                    component.ExecutionOrderIndex = index;
                    instances.Add(component);
                    instances.Sort(new ScriptExecutionOrderComparer());
                    return true;
                }
            }

            return false;
        }

        public bool RemoveInstance(IScriptComponent component)
        {
            lock (_lock)
            {
                return instances.Remove(component);
            }
        }

        public IReadOnlyList<ScriptNode> Nodes => nodes;

        public IReadOnlyDictionary<ScriptFlags, IList<IScriptComponent>> Instances => instances;

        public void ExecuteAwake()
        {
            var scriptList = instances[ScriptFlags.Awake];

            if (parallel)
            {
                Parallel.ForEach(scriptList, script =>
                {
                    script.ScriptAwake();
                });
                return;
            }

            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].ScriptAwake();
            }
        }

        public void ExecuteDestroy()
        {
            var scriptList = instances[ScriptFlags.Destroy];

            if (parallel)
            {
                Parallel.ForEach(scriptList, script =>
                {
                    script.Destroy();
                });
                return;
            }

            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Destroy();
            }
        }

        public void ExecuteUpdate()
        {
            var scriptList = instances[ScriptFlags.Update];

            if (parallel)
            {
                Parallel.ForEach(scriptList, script =>
                {
                    script.Update();
                });
                return;
            }

            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Update();
            }
        }

        public void ExecuteFixedUpdate()
        {
            var scriptList = instances[ScriptFlags.FixedUpdate];

            if (parallel)
            {
                Parallel.ForEach(scriptList, script =>
                {
                    script.FixedUpdate();
                });
                return;
            }

            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].FixedUpdate();
            }
        }

        public void Clear()
        {
            instances.Clear();
            nodes.Clear();
            map.Clear();
        }
    }
}