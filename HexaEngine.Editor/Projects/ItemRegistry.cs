namespace HexaEngine.Editor.Projects
{
    using HexaEngine.Editor.Packaging;
    using System.Diagnostics.CodeAnalysis;

    public static class ItemRegistry
    {
        private static readonly List<Type> types = [];
        private static readonly Dictionary<string, Type> typesByName = [];

        static ItemRegistry()
        {
            Register<PackageReference>(nameof(PackageReference));
        }

        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string name) where T : IItemGroupItem, new()
        {
            var type = typeof(T);
            types.Add(type);
            typesByName.Add(name, type);
        }

        public static Type GetType(string name)
        {
            if (typesByName.TryGetValue(name, out Type? type))
            {
                return type;
            }
            throw new ArgumentException($"Type with name '{name}' is not registered.");
        }

        public static IItemGroupItem CreateNew(string name)
        {
            var type = GetType(name);
            return (IItemGroupItem?)Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Failed to create instance of type '{type.FullName}'.");
        }
    }
}