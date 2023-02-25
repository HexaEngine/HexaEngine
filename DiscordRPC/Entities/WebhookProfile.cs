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
    ///     Profile information to override the webhook profile.
    /// </summary>
    public sealed class WebhookProfile
    {
        /// <summary>
        ///     Gets or sets the username to override the webhook profile.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'Username' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public string Username { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Username' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the URL to the avatar to override the webhook profile.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'AvatarUrl' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public string AvatarUrl { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'AvatarUrl' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Adds a username override to this profile override.
        /// </summary>
        /// <param name="username">Username override to add.</param>
        /// <returns>This profile override.</returns>
        public WebhookProfile WithUsername(string username)
        {
            Username = username;
            return this;
        }

        /// <summary>
        ///     Adds an avatar URL override to this profile override.
        /// </summary>
        /// <param name="avatarUrl">URL to the avatar override.</param>
        /// <returns>This profile override.</returns>
        public WebhookProfile WithAvatarUrl(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
            return this;
        }
    }
}