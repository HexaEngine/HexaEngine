namespace HexaEngine.PostFx
{
    using HexaEngine.Graphics.Graph;
    using System.Collections.Generic;

    public class PostFxDependencyBuilder
    {
        private readonly PostFxNode node;
        private readonly PostFxNameRegistry nameRegistry;
        private readonly List<ResourceBinding> bindings = [];
        private readonly List<ResourceBinding> sources = [];

        private readonly List<string> after = [];
        private readonly List<string> before = [];
        private readonly List<string> overrides = [];

        public PostFxDependencyBuilder(PostFxNode node, PostFxNameRegistry nameRegistry)
        {
            this.node = node;
            this.nameRegistry = nameRegistry;
        }

        /// <summary>
        /// This clears all references and sources. Only use if you know what you're doing.
        /// </summary>
        public void Clear()
        {
            bindings.Clear();
            sources.Clear();
            overrides.Clear();
            after.Clear();
            before.Clear();
        }

        /// <summary>
        /// Creates a compose node in the graph, for composing the effect later in the pipeline.
        /// </summary>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder Compose()
        {
            PostFxComposeNodeBuilder builder = new(nameRegistry, node);
            return builder;
        }

        /// <summary>
        /// Overrides <typeparamref name="T"/>, if it's found.
        /// </summary>
        /// <typeparam name="T">The type to override to.</typeparam>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder Override<T>() where T : IPostFx
        {
            if (nameRegistry.TryGetNameBy<T>(out string? name))
            {
                overrides.Add(name);
            }

            return this;
        }

        /// <summary>
        /// Adds a soft dependency <typeparamref name="T"/>, that means if the effect is not available/disabled it will be ignored.
        /// </summary>
        /// <typeparam name="T">The effect type.</typeparam>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder RunAfter<T>() where T : IPostFx
        {
            if (nameRegistry.TryGetNameBy<T>(out string? name))
            {
                after.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Adds a dependency if <typeparamref name="T"/> exists and is enabled.
        /// </summary>
        /// <typeparam name="T">The effect type.</typeparam>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder RunBefore<T>() where T : IPostFx
        {
            if (nameRegistry.TryGetNameBy<T>(out string? name))
            {
                before.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Overrides a effect with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the effect.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder Override(string name)
        {
            overrides.Add(name);
            return this;
        }

        /// <summary>
        /// References a resource and creates a hard dependency.
        /// </summary>
        /// <param name="binding">The binding name.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder AddBinding(ResourceBinding binding)
        {
            bindings.Add(binding);
            return this;
        }

        /// <summary>
        /// References a resource and creates a hard dependency.
        /// </summary>
        /// <param name="name">The name of the binding.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder AddBinding(string name)
        {
            bindings.Add(new(name));
            return this;
        }

        /// <summary>
        /// Adds a resource source where other effects can reference it.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder AddSource(ResourceBinding binding)
        {
            sources.Add(binding);
            return this;
        }

        /// <summary>
        /// Adds a source where other effects can reference it.
        /// </summary>
        /// <param name="name">The name the source.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder AddSource(string name)
        {
            sources.Add(new(name));
            return this;
        }

        /// <summary>
        /// Adds a soft dependency to the effect, that means if the effect is not available/disabled it will be ignored.
        /// </summary>
        /// <param name="name">The name the target.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder RunAfter(string name)
        {
            after.Add(name);
            return this;
        }

        /// <summary>
        /// Adds a dependency if the other effect exists and is enabled.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder RunBefore(string name)
        {
            before.Add(name);
            return this;
        }

        /// <summary>
        /// Adds a soft dependency to all effects that doesn't reference this as run after.
        /// </summary>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder RunAfterAllNotReferenced()
        {
            after.Add("!AllNotReferenced");
            return this;
        }

        /// <summary>
        /// Adds a soft dependency to all effects.
        /// </summary>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxDependencyBuilder RunAfterAll()
        {
            after.Add("!All");
            return this;
        }

        private static PostFxNode? ResolveDependency(ResourceBinding binding, IReadOnlyList<PostFxNode> others, IReadOnlyList<ResourceBinding> globalResources)
        {
            var name = binding.Name;
            if (name.StartsWith('#'))
            {
                throw new InvalidOperationException($"Couldn't find render graph global resource: {name}");
            }
            else
            {
                for (int i = 0; i < others.Count; i++)
                {
                    var node = others[i];
                    var builder = node.Builder;
                    for (int j = 0; j < builder.sources.Count; j++)
                    {
                        var source = builder.sources[j];
                        if (source.Name == name)
                        {
                            return node;
                        }
                    }
                }
                for (int i = 0; i < globalResources.Count; i++)
                {
                    var global = globalResources[i];
                    if (global.Name == name)
                    {
                        return null;
                    }
                }

                throw new InvalidOperationException($"Couldn't find local or sub graph local resource: {name}");
            }
        }

        private static PostFxNode? ResolveNode(string name, IReadOnlyList<PostFxNode> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                var other = others[i];
                if (other.Name == name && other.Enabled)
                {
                    return other;
                }
            }
            return null;
        }

        public bool HasDependencyOn(string name)
        {
            if (after.Contains(name))
            {
                return true;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].Name == name)
                    return true;
            }

            return false;
        }

        public bool IsReferencedBy(PostFxNode other)
        {
            if (other.Builder.HasDependencyOn(node.Name))
            {
                return true;
            }

            for (int i = 0; i < sources.Count; i++)
            {
                if (other.Builder.HasDependencyOn(sources[i].Name))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Overrides(PostFxNode other)
        {
            return overrides.Contains(other.Name);
        }

        public void Build(IReadOnlyList<PostFxNode> others, IReadOnlyList<ResourceBinding> globalResources)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var dep = ResolveDependency(bindings[i], others, globalResources);
                if (dep is null)
                {
                    node.GlobalDependencies.Add(bindings[i]);
                }
                else
                {
                    if (!node.Dependencies.Contains(dep))
                    {
                        node.Dependencies.Add(dep);
                        dep.Dependents.Add(node);
                    }
                }
            }

            for (int i = 0; i < after.Count; i++)
            {
                var runAfter = after[i];
                var afterNode = ResolveNode(runAfter, others);
                if (afterNode is null)
                {
                    if (runAfter == "!All")
                    {
                        for (int j = 0; j < others.Count; j++)
                        {
                            var other = others[j];
                            if (node != other)
                            {
                                if (!node.Dependencies.Contains(other))
                                    node.Dependencies.Add(other);
                            }
                        }
                    }
                    if (runAfter == "!AllNotReferenced")
                    {
                        for (int j = 0; j < others.Count; j++)
                        {
                            var other = others[j];
                            if (node != other && !IsReferencedBy(other))
                            {
                                if (!node.Dependencies.Contains(other))
                                    node.Dependencies.Add(other);
                            }
                        }
                    }
                    continue;
                }
                if (!node.Dependencies.Contains(afterNode))
                    node.Dependencies.Add(afterNode);
            }

            for (int i = 0; i < before.Count; i++)
            {
                var beforeNode = ResolveNode(before[i], others);
                if (beforeNode is null)
                {
                    continue;
                }
                if (!beforeNode.Dependencies.Contains(node))
                    beforeNode.Dependencies.Add(node);
            }
        }
    }
}