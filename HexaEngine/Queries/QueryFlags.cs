namespace HexaEngine.Queries
{
    public enum QueryFlags
    {
        None = 0,

        ObjectAdded = 1,
        ObjectRemoved = 2,
        ComponentAdded = 4,
        ComponentRemoved = 8,
        PropertyChanged = 16,

        NameChanged = 32,
        TagChanged = 64,
        ParentChanged = 128,
        Transformed = 256,

        Default = ObjectAdded | ObjectRemoved | ComponentAdded | ComponentRemoved,
        All = ObjectAdded | ObjectRemoved | ComponentAdded | ComponentRemoved | PropertyChanged,
    }
}