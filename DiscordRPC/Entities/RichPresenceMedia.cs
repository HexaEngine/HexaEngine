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
    ///     Media information for <see cref="RichPresence"/> objects.
    /// </summary>
    public sealed class RichPresenceMedia
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RichPresenceMedia"/> class.
        /// </summary>
        /// <param name="imageKey">Image key from the <see href="https://discord.com/developers">Discord Developer Portal</see>.</param>
        /// <param name="tooltip">Image tooltip.</param>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public RichPresenceMedia(string imageKey, string tooltip = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            ImageKey = imageKey;
            Tooltip = tooltip;
        }

        /// <summary>
        ///     Gets or sets the image key from the <see href="https://discord.com/developers">Discord Developer Portal</see>.
        /// </summary>
        public string ImageKey { get; set; }

        /// <summary>
        ///     Gets or sets the image tooltip.
        /// </summary>
        public string Tooltip { get; set; }
    }
}