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

using DiscordRPC.Entities.Embeds;

namespace DiscordRPC.Entities
{
    /// <summary>
    ///    Represents a message to be sent by a webhook.
    /// </summary>
    public sealed class WebhookMessage
    {
        /// <summary>
        ///     Gets or sets the message content.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'Content' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public string Content { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Content' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets the embeds.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable property 'Embeds' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public List<Embed> Embeds { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Embeds' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        /// <summary>
        ///     Gets or sets whether the message should be sent as a text-to-speech message.
        /// </summary>
        public bool Tts { get; set; }

        /// <summary>
        ///     Adds content to this message.
        /// </summary>
        /// <param name="content">Message content to add.</param>
        /// <returns>This message.</returns>
        public WebhookMessage WithContent(string content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        ///     Adds an embed to this message.
        /// </summary>
        /// <param name="embed">Embed to add.</param>
        /// <returns>This message.</returns>
        public WebhookMessage AddEmbed(Embed embed)
        {
            Embeds ??= new List<Embed>();

            Embeds.Add(embed);
            return this;
        }

        /// <summary>
        ///     Adds embeds to this message.
        /// </summary>
        /// <param name="embeds">Embeds to add.</param>
        /// <returns>This message.</returns>
        public WebhookMessage AddEmbeds(params Embed[] embeds)
        {
            Embeds ??= new List<Embed>();

            Embeds.AddRange(embeds);
            return this;
        }

        /// <summary>
        ///     Adds embeds to this message.
        /// </summary>
        /// <param name="embeds">Embeds to add.</param>
        /// <returns>This message.</returns>
        public WebhookMessage AddEmbeds(IEnumerable<Embed> embeds)
        {
            Embeds ??= new List<Embed>();

            Embeds.AddRange(embeds);
            return this;
        }
    }
}