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

using System.Text.Json.Serialization;

namespace DiscordRPC.Entities
{
    /// <summary>
    ///     Attachment information for webhooks messages.
    /// </summary>
    public class WebhookAttachment
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WebhookAttachment"/> class.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="altText">Alt-text for the file.</param>
        /// <param name="spoiler">Whether the file should have the spoiler tag.</param>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public WebhookAttachment(string path, string altText = null, bool spoiler = false)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            FileName = (spoiler ? "SPOILER_" : null) + Path.GetFileName(path);
            AltText = altText;
            FileData = File.ReadAllBytes(path);
        }

        internal WebhookAttachment(WebhookAttachment original, int id)
        {
            Id = id;
            FileName = original.FileName;
            AltText = original.AltText;
            FileData = original.FileData;
        }

        /// <summary>
        ///     Gets the ID of the attachment.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
        public int Id { get; internal set; }

        /// <summary>
        ///     Gets the name of the file.
        /// </summary>
        [JsonPropertyName("filename")]
        public string FileName { get; internal set; }

        /// <summary>
        ///     Gets or sets the alt-text for the file.
        /// </summary>
        [JsonPropertyName("description")]
        public string AltText { get; set; }

        /// <summary>
        ///     Gets the file data.
        /// </summary>
        [JsonIgnore]
        public byte[] FileData { get; internal set; }
    }
}