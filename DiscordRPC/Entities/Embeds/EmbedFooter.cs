﻿// MIT License
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

using System.Text.Json.Serialization;

namespace DiscordRPC.Entities.Embeds
{
    /// <summary>
    ///     Footer information for <see cref="Embed"/> objects.
    /// </summary>
    public sealed class EmbedFooter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbedFooter"/> class.
        /// </summary>
        /// <param name="text">Footer text.</param>
        /// <param name="iconUrl">URL to the footer icon.</param>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public EmbedFooter(string text, string iconUrl = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            Text = text;
            IconUrl = iconUrl;
        }

        /// <summary>
        ///     Gets or sets the footer text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets the URL to the footer icon.
        /// </summary>
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; }
    }
}