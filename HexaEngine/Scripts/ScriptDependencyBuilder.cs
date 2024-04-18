namespace HexaEngine.Scripts
{
    using HexaEngine.PostFx;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ScriptDependencyBuilder
    {
        private readonly ScriptNode node;

        public ScriptDependencyBuilder(ScriptNode node, ScriptTypeRegistry registry)
        {
            this.node = node;
        }

        public ScriptNode Node => node;

        public List<Type> Dependencies { get; } = [];

        public void Clear()
        {
            Dependencies.Clear();
        }

        public void Build(IReadOnlyList<ScriptNode> others, ScriptFlags stage)
        {
        }
    }
}