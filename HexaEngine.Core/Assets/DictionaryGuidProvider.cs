namespace HexaEngine.Core.Assets
{
    using System;

    public readonly struct DictionaryGuidProvider : IGuidProvider, IEquatable<DictionaryGuidProvider>
    {
        private readonly Guid parent;
        private readonly Dictionary<string, Guid> dictionary;
        private readonly GuidNotFoundBehavior behavior;

        public DictionaryGuidProvider(Guid parent, Dictionary<string, Guid> dictionary, GuidNotFoundBehavior behavior)
        {
            this.parent = parent;
            this.dictionary = dictionary;
            this.behavior = behavior;
        }

        public Guid ParentGuid => parent;

        public override bool Equals(object? obj)
        {
            return obj is DictionaryGuidProvider provider && Equals(provider);
        }

        public bool Equals(DictionaryGuidProvider other)
        {
            return parent.Equals(other.parent) &&
                   EqualityComparer<Dictionary<string, Guid>>.Default.Equals(dictionary, other.dictionary) &&
                   behavior == other.behavior &&
                   ParentGuid.Equals(other.ParentGuid);
        }

        public readonly Guid GetGuid(string name)
        {
            if (dictionary.TryGetValue(name, out Guid guid))
            {
                return guid;
            }

            switch (behavior)
            {
                case GuidNotFoundBehavior.GenerateNew:
                    guid = Guid.NewGuid();
                    dictionary[name] = guid;
                    return guid;

                case GuidNotFoundBehavior.Throw:
                    throw new KeyNotFoundException();
                default:
                    throw new NotSupportedException();
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(parent, dictionary, behavior, ParentGuid);
        }

        public static bool operator ==(DictionaryGuidProvider left, DictionaryGuidProvider right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DictionaryGuidProvider left, DictionaryGuidProvider right)
        {
            return !(left == right);
        }
    }
}