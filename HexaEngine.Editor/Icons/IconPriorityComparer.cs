namespace HexaEngine.Editor.Icons
{
    public class IconPriorityComparer : IComparer<Icon>
    {
        public static readonly IconPriorityComparer Default = new();

        public int Compare(Icon? x, Icon? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            if (x.Priority < y.Priority)
            {
                return 1;
            }
            else if (x.Priority > y.Priority)
            {
                return -1;
            }

            return 0;
        }
    }
}