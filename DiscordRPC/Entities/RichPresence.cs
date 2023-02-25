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

using DiscordGameSdk;

namespace DiscordRPC.Entities
{
    /// <summary>
    ///     Represents a Discord rich presence activity.
    /// </summary>
    public sealed class RichPresence
    {
        /// <summary>
        ///     Gets or sets the state of this RPC.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'State' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public string State { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'State' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the details of this RPC.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'Details' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public string Details { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Details' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the timestamp information of this RPC.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'Timestamp' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public RichPresenceTimestamp Timestamp { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Timestamp' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the large image information of this RPC.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'LargeImage' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public RichPresenceMedia LargeImage { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'LargeImage' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the small image information of this RPC.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'SmallImage' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public RichPresenceMedia SmallImage { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'SmallImage' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the party information of this RPC.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'Party' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public RichPresenceParty Party { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Party' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Adds a state to this RPC.
        /// </summary>
        /// <param name="state">State to add.</param>
        /// <returns>This RPC.</returns>
        public RichPresence WithState(string state)
        {
            State = state;
            return this;
        }

        /// <summary>
        ///     Adds details to this RPC.
        /// </summary>
        /// <param name="details">Details to add.</param>
        /// <returns>This RPC.</returns>
        public RichPresence WithDetails(string details)
        {
            Details = details;
            return this;
        }

        /// <summary>
        ///     Adds timestamp information to this RPC.
        /// </summary>
        /// <param name="timestamp">Timestamp information to add.</param>
        /// <returns>This RPC.</returns>
        public RichPresence WithTimestamp(RichPresenceTimestamp timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        /// <summary>
        ///     Adds large image information to this RPC.
        /// </summary>
        /// <param name="largeImage">Large image information to add.</param>
        /// <returns>This RPC.</returns>
        public RichPresence WithLargeImage(RichPresenceMedia largeImage)
        {
            LargeImage = largeImage;
            return this;
        }

        /// <summary>
        ///     Adds small image information to this RPC.
        /// </summary>
        /// <param name="smallImage">Small image information to add.</param>
        /// <returns>This RPC.</returns>
        public RichPresence WithSmallImage(RichPresenceMedia smallImage)
        {
            SmallImage = smallImage;
            return this;
        }

        /// <summary>
        ///     Adds party information to this RPC.
        /// </summary>
        /// <param name="party">Party information to add.</param>
        /// <returns>This RPC.</returns>
        public RichPresence WithParty(RichPresenceParty party)
        {
            Party = party;
            return this;
        }

        internal Activity ToActivity()
        {
            var activity = new Activity
            {
                State = State,
                Details = Details
            };

            if (Timestamp?.Start != null)
            {
                activity.Timestamps.Start = ((DateTimeOffset)Timestamp.Start).ToUnixTimeSeconds();
            }

            if (Timestamp?.End != null)
            {
                activity.Timestamps.End = ((DateTimeOffset)Timestamp.End).ToUnixTimeSeconds();
            }

            if (LargeImage != null)
            {
                activity.Assets.LargeImage = LargeImage.ImageKey;
                activity.Assets.LargeText = LargeImage.Tooltip;
            }

            if (SmallImage != null)
            {
                activity.Assets.SmallImage = SmallImage.ImageKey;
                activity.Assets.SmallText = SmallImage.Tooltip;
            }

            if (Party != null)
            {
                activity.Party.Id = Party.Id;
                activity.Party.Size.CurrentSize = Party.CurrentSize;
                activity.Party.Size.MaxSize = Party.MaxSize;
            }

            return activity;
        }
    }
}