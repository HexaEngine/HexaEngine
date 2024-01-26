namespace HexaEngine.Jobs
{
    public class JobPriorityComparer : IComparer<Job>
    {
        public int Compare(Job? x, Job? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.Priority.CompareTo(y.Priority);
        }

        public static readonly JobPriorityComparer Instance = new();
    }
}