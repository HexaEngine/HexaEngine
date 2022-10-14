using HexaEngine.Core.Unsafes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public class Program
{
    public static unsafe void Main()
    {
        /*
        List<string> strings = new List<string>();

        while (true)
        {
            string? input = Console.ReadLine();
            if (input == null) continue;
            if (input == "e") break;
            strings.Add(input);
        }
        string inp = string.Join(string.Empty, strings.ToArray());
        string[] nums = inp.Replace(Environment.NewLine, "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        int[] ints = nums.Select(int.Parse).ToArray();
        StringBuilder sb = new();
        for (int i = 0; i < ints.Length; i += 2)
        {
            sb.AppendLine($"{ints[i] - 1},{ints[i + 1] - 1},");
        }

        Console.WriteLine(sb.ToString());*/

        byte[] union = new byte[sizeof(RecordHeader)];

        RecordHeader* t;
        fixed (byte* ptr = union)
            t = (RecordHeader*)ptr;

        RecordHeader header = *t;

        UnsafeString str = new("Test");

        byte* bytes = stackalloc byte[12];

        UnsafeString.Write(&str, new Span<byte>(bytes, 12));
    }

    public unsafe struct RecordHeader
    {
        public ulong Id;
        public ulong ParentId;
        public int Type;
        public int Length;
    }

    public enum Type
    {
        T1,
        T2
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Union
    {
        [FieldOffset(0)]
        public Type Type;

        [FieldOffset(4)]
        public Type1 Type1;

        [FieldOffset(4)]
        public Type2 Type2;
    }

    public unsafe struct Type1
    {
        public uint Reserved;
    }

    public unsafe struct Type2
    {
        public uint Reserved0;
        public uint Reserved1;
        public uint Reserved2;
        public uint Reserved3;
    }
}