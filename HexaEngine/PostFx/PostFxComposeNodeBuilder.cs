namespace HexaEngine.PostFx
{
    public class PostFxComposeNodeBuilder
    {
        private readonly PostFxNode parentNode;
        private readonly PostFxNode composeNode;

        public PostFxComposeNodeBuilder(PostFxNameRegistry nameRegistry, PostFxNode parentNode)
        {
            this.parentNode = parentNode;
            composeNode = new(parentNode.Graph, new PostFxComposeTarget(parentNode.PostFx), nameRegistry, true);
            parentNode.Graph.AddComposeNode(composeNode);
        }

        public PostFxNode Parent => parentNode;

        public PostFxNode Node => composeNode;

        /// <summary>
        /// This clears all references and sources. Only use if you know what you're doing.
        /// </summary>
        public void Clear()
        {
            composeNode.Builder.Clear();
        }

        /// <summary>
        /// Overrides <typeparamref name="T"/>, if it's found.
        /// </summary>
        /// <typeparam name="T">The type to override to.</typeparam>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder Override<T>() where T : IPostFx
        {
            composeNode.Builder.Override<T>();
            return this;
        }

        /// <summary>
        /// Adds a soft dependency <typeparamref name="T"/>, that means if the effect is not available/disabled it will be ignored.
        /// </summary>
        /// <typeparam name="T">The effect type.</typeparam>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder After<T>() where T : IPostFx
        {
            composeNode.Builder.RunAfter<T>();
            return this;
        }

        /// <summary>
        /// Adds a dependency if <typeparamref name="T"/> exists and is enabled.
        /// </summary>
        /// <typeparam name="T">The effect type.</typeparam>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder Before<T>() where T : IPostFx
        {
            composeNode.Builder.RunBefore<T>();
            return this;
        }

        /// <summary>
        /// Overrides a effect with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the effect.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder Override(string name)
        {
            composeNode.Builder.Override(name);
            return this;
        }

        /// <summary>
        /// Adds a soft dependency to the effect, that means if the effect is not available/disabled it will be ignored.
        /// </summary>
        /// <param name="name">The name the target.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder After(string name)
        {
            composeNode.Builder.RunAfter(name);
            return this;
        }

        /// <summary>
        /// Adds a dependency if the other effect exists and is enabled.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder Before(string name)
        {
            composeNode.Builder.RunBefore(name);
            return this;
        }

        /// <summary>
        /// Adds a soft dependency to all effects that doesn't reference this as run after.
        /// </summary>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder AfterAllNotReferenced()
        {
            composeNode.Builder.RunAfterAllNotReferenced();
            return this;
        }

        /// <summary>
        /// Adds a soft dependency to all effects.
        /// </summary>
        /// <returns>Returns <see langword="this"/> for chaining operations.</returns>
        public PostFxComposeNodeBuilder AfterAll()
        {
            composeNode.Builder.RunAfterAll();
            return this;
        }
    }
}