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

using DiscordIntegration.Entities.Embeds;
using System.Text.Json.Serialization;

namespace DiscordRPC.Entities.Embeds
{
    /// <summary>
    ///     Represents a Discord embed.
    /// </summary>
    public sealed class Embed
    {
        /// <summary>
        ///     Gets or sets the title of this embed.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets the description of this embed.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the URL of this embed.
        ///     <para>Makes the title of this embed a hyperlink.</para>
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        ///     Gets or sets the timestamp of this embed.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        ///     Gets or sets the color information for this embed.
        /// </summary>
        [JsonPropertyName("color")]
        [JsonConverter(typeof(EmbedColor.Converter))]
        public EmbedColor? Color { get; set; }

        /// <summary>
        ///     Gets or sets the footer information for this embed.
        /// </summary>
        [JsonPropertyName("footer")]
        public EmbedFooter Footer { get; set; }

        /// <summary>
        ///     Gets or sets the image information for this embed.
        /// </summary>
        [JsonPropertyName("image")]
        public EmbedMedia Image { get; set; }

        /// <summary>
        ///     Gets or sets the thumbnail image information for this embed.
        /// </summary>
        [JsonPropertyName("thumbnail")]
        public EmbedMedia Thumbnail { get; set; }

        /// <summary>
        ///     Gets or sets the author information for this embed.
        /// </summary>
        [JsonPropertyName("author")]
        public EmbedAuthor Author { get; set; }

        /// <summary>
        ///     Gets or sets the fields of this embed.
        /// </summary>
        [JsonPropertyName("fields")]
        public List<EmbedField> Fields { get; set; }

        /// <summary>
        ///     Adds a title to this embed.
        /// </summary>
        /// <param name="title">Title to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        ///     Adds a description to this embed.
        /// </summary>
        /// <param name="description">Description to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        ///     Adds a URL to this embed.
        ///     <para>Makes the title of this embed a hyperlink.</para>
        /// </summary>
        /// <param name="url">URL to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithUrl(string url)
        {
            Url = url;
            return this;
        }

        /// <summary>
        ///     Adds a timestamp to this embed.
        /// </summary>
        /// <param name="timestamp">Timestamp to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        /// <summary>
        ///     Adds color information to this embed.
        /// </summary>
        /// <param name="color">Color information to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithColor(EmbedColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        ///     Adds footer information to this embed.
        /// </summary>
        /// <param name="footer">Footer information to add.</param>
        /// <returns></returns>
        public Embed WithFooter(EmbedFooter footer)
        {
            Footer = footer;
            return this;
        }

        /// <summary>
        ///     Adds image information to this embed.
        /// </summary>
        /// <param name="image">Image information to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithImage(EmbedMedia image)
        {
            Image = image;
            return this;
        }

        /// <summary>
        ///     Adds thumbnail image information to this embed.
        /// </summary>
        /// <param name="thumbnail">Thumbnail image information to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithThumbnail(EmbedMedia thumbnail)
        {
            Thumbnail = thumbnail;
            return this;
        }

        /// <summary>
        ///     Adds author information to this embed.
        /// </summary>
        /// <param name="author">Author information to add.</param>
        /// <returns>This embed.</returns>
        public Embed WithAuthor(EmbedAuthor author)
        {
            Author = author;
            return this;
        }

        /// <summary>
        ///     Adds a field to this embed.
        /// </summary>
        /// <param name="field">Field to add.</param>
        /// <returns>This embed.</returns>
        public Embed AddField(EmbedField field)
        {
            Fields ??= new List<EmbedField>();

            Fields.Add(field);
            return this;
        }

        /// <summary>
        ///     Adds fields to this embed.
        /// </summary>
        /// <param name="fields">Fields to add.</param>
        /// <returns>This embed.</returns>
        public Embed AddFields(params EmbedField[] fields)
        {
            Fields ??= new List<EmbedField>();

            Fields.AddRange(fields);
            return this;
        }

        /// <summary>
        ///     Adds fields to this embed.
        /// </summary>
        /// <param name="fields">Fields to add.</param>
        /// <returns>This embed.</returns>
        public Embed AddFields(IEnumerable<EmbedField> fields)
        {
            Fields ??= new List<EmbedField>();

            Fields.AddRange(fields);
            return this;
        }
    }
}