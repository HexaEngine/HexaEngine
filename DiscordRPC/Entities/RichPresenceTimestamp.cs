// MIT License
//
// Copyright(c) 2022 Bujju
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DiscordRPC.Entities
{
    /// <summary>
    ///     Timestamp information for <see cref="RichPresence"/> objects.
    /// </summary>
    public sealed class RichPresenceTimestamp
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RichPresenceTimestamp"/> class.
        /// </summary>
        /// <param name="timestamp"><see cref="DateTime"/> representing the start or end point of this timestamp.</param>
        /// <param name="displayType">Whether this should be a "left" or "elapsed" timestamp.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timestamp"/> is invalid for the specified display type.</exception>
        public RichPresenceTimestamp(DateTime timestamp, TimestampDisplayType displayType)
        {
            if (displayType == TimestampDisplayType.Left)
            {
                if (timestamp < DateTime.Now)
                    throw new ArgumentException("Timestamp must be in the future when using TimestampDisplayType.Left.", nameof(timestamp));

                End = timestamp;
            }
            else if (displayType == TimestampDisplayType.Elapsed)
            {
                if (timestamp > DateTime.Now)
                    throw new ArgumentException("Timestamp must be in the past when using TimestampDisplayType.Elapsed.", nameof(timestamp));

                Start = timestamp;
            }
        }

        /// <summary>
        ///     Gets or sets the start time of this timestamp. (Set for elapsed timestamps.)
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        ///     Gets or sets the end time of this timestamp. (Set for left timestamps.)
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        ///     The display types for timestamps.
        /// </summary>
        public enum TimestampDisplayType
        {
            /// <summary>
            ///     Defines that this timestamp should be displayed as a "left" timestamp. The timestamp should be after the current time.
            ///     <para>00:00 left</para>
            /// </summary>
            Left = 0,

            /// <summary>
            ///     Defines that this timestamp should be displayed as a "elapsed" timestamp. The timestamp should be before the current time.
            ///     <para>00:00 elapsed</para>
            /// </summary>
            Elapsed = 1
        }
    }
}