namespace HexaEngine.Scripts
{
    using HexaEngine.Core;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public class ScriptManager : ISceneSystem
    {
        private readonly ComponentTypeQuery<IScriptComponent> components = new();
        private readonly ScriptGraph graph = new();
        private readonly List<ScriptGroup> groups = [];
        private readonly object _lock = new();
        private bool awaked;

        public string Name => "Scripts";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Update | SystemFlags.FixedUpdate | SystemFlags.Destroy;

        public ScriptGraph Graph => graph;

        public IReadOnlyList<ScriptGroup> Groups => groups;

        private void AssemblyLoaded(object? sender, System.Reflection.Assembly e)
        {
            lock (_lock)
            {
                UpdateGraphInternal();
            }
        }

        private void AssembliesUnloaded(object? sender, EventArgs? e)
        {
            lock (_lock)
            {
                ClearGroups();
                graph.Clear();
            }
        }

        public void Awake(Scene scene)
        {
            lock (_lock)
            {
                ScriptAssemblyManager.AssemblyLoaded += AssemblyLoaded;
                ScriptAssemblyManager.AssembliesUnloaded += AssembliesUnloaded;

                scene.QueryManager.AddQuery(components);

                for (int i = 0; i < components.Count; i++)
                {
                    components[i].ScriptCreate();
                }

                for (int i = 0; i < components.Count; i++)
                {
                    components[i].ScriptLoad();
                }

                UpdateGraphInternal();

                components.OnAdded += OnAdded;
                components.OnRemoved += OnRemoved;

                if (Application.InEditMode)
                {
                    return;
                }

                awaked = true;

                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].ExecuteAwake();
                }
            }
        }

        public void UpdateGraph()
        {
            lock (_lock)
            {
                UpdateGraphInternal();
            }
        }

        private void UpdateGraphInternal()
        {
            ClearGroups();

            graph.Clear();

            IList<Type> types = ScriptAssemblyManager.GetAssignableTypes<ScriptBehaviour>();
            foreach (var item in types)
            {
                graph.AddNode(item);
            }

            graph.Build();

            var nodeGroups = graph.GroupNodesForParallelExecution();

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                var nodeGroup = nodeGroups[i];
                bool runInParallel = nodeGroup[0].RunInParallel;
                ScriptGroup group = new(nodeGroup, runInParallel);
                groups.Add(group);
            }

            for (int i = 0; i < components.Count; i++)
            {
                AddToGroup(components[i]);
            }
        }

        private void ClearGroups()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].Clear();
            }

            groups.Clear();
        }

        private void OnRemoved(GameObject gameObject, IScriptComponent component)
        {
            lock (_lock)
            {
                RemoveFromGroup(component);
            }

            if (Application.InEditMode || !awaked)
            {
                return;
            }

            component.Destroy();
        }

        private void OnAdded(GameObject gameObject, IScriptComponent component)
        {
            lock (_lock)
            {
                AddToGroup(component);
            }

            if (Application.InEditMode || !awaked)
            {
                return;
            }

            component.ScriptCreate();
            component.ScriptLoad();

            if ((component.Flags & ScriptFlags.Awake) != 0)
            {
                component.ScriptAwake();
            }
        }

        private void AddToGroup(IScriptComponent component)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].AddInstance(component))
                {
                    break;
                }
            }
        }

        private void RemoveFromGroup(IScriptComponent component)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].RemoveInstance(component))
                {
                    break;
                }
            }
        }

        public void Destroy()
        {
            lock (_lock)
            {
                ScriptAssemblyManager.AssemblyLoaded -= AssemblyLoaded;
                ScriptAssemblyManager.AssembliesUnloaded -= AssembliesUnloaded;

                graph.Clear();

                components.OnAdded -= OnAdded;
                components.OnRemoved -= OnRemoved;

                components.Dispose();

                if (Application.InEditMode)
                {
                    ClearGroups();
                    return;
                }

                awaked = false;

                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].ExecuteDestroy();
                }

                ClearGroups();
            }
        }

        public void Update(float delta)
        {
            if (Application.InEditMode)
            {
                return;
            }

            lock (_lock)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].ExecuteUpdate();
                }
            }
        }

        public void FixedUpdate()
        {
            if (Application.InEditMode)
            {
                return;
            }

            lock (_lock)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].ExecuteFixedUpdate();
                }
            }
        }
    }
}