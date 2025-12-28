namespace HexaEngine.Core.Debugging
{
    using System;

    public class ConsoleMessageBuffer
    {
        private readonly CircularBuffer<ConsoleMessage> messages;
        private readonly int[] linePrefixSum;
        private int baseLineOffset;

        public ConsoleMessageBuffer(int capacity)
        {
            messages = new(capacity);
            linePrefixSum = new int[capacity];
        }

        private void RebuildPrefixSum()
        {
            var enumerator = messages.GetEnumerator();
            int accum = 0;
            while (enumerator.MoveNext())
            {
                accum += enumerator.Current.Lines;
                linePrefixSum[enumerator.AbsoluteIndex] = accum;
            }
            baseLineOffset = 0;
        }

        public void Add(in ConsoleMessage message)
        {
            int lines = message.Lines;

            var head = messages.Head;
            var tail = messages.Tail;
            var mask = messages.Mask;
            if (head == tail && messages.Count > 0)
            {
                baseLineOffset += linePrefixSum[messages.Tail];
            }

            if (head == 0)
            {
                linePrefixSum[head] = baseLineOffset;
            }
            else
            {
                linePrefixSum[head] = linePrefixSum[(head - 1) & mask] + lines;
            }

            if (linePrefixSum[head] > int.MaxValue >> 1)
            {
                RebuildPrefixSum();
            }

            messages.Add(message);
        }

        public int GetLineOffset(int index)
        {
            return linePrefixSum[(messages.Tail + index) & messages.Mask] - baseLineOffset;
        }

        public int FindIndexForLine(int targetLine)
        {
            int absoluteTarget = targetLine + baseLineOffset;
            int count = messages.Count;
            int left = 0, right = count;

            while (left < right)
            {
                int mid = (left + right) / 2;
                int bufferIdx = (messages.Tail + mid) & messages.Mask;

                if (linePrefixSum[bufferIdx] < absoluteTarget)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }
            }

            return Math.Max(0, left - 1);
        }

        public int TotalLineCount => linePrefixSum[(messages.Head - 1) & messages.Mask] - baseLineOffset;

        public int Count => messages.Count;

        public ConsoleMessage this[int index] { get => messages[index]; set => messages[index] = value; }

        public void Clear() => messages.Clear();

        public CircularBuffer<ConsoleMessage>.Enumerator GetEnumerator() => messages.GetEnumerator();
    }
}