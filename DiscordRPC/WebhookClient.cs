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

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DiscordRPC.Entities;
using DiscordRPC.Entities.Embeds;

namespace DiscordRPC
{
    /// <summary>
    ///     A client for Discord webhooks.
    /// </summary>
    public class WebhookClient : IDisposable
    {
        private HttpClient _client;

        private bool _isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WebhookClient"/> class.
        /// </summary>
        /// <param name="webhookUrl">Webhook URL.</param>
        public WebhookClient(string webhookUrl)
        {
            _client = new HttpClient();
            WebhookUrl = webhookUrl;
        }

        /// <summary>
        ///     Gets or sets the webhook URL.
        /// </summary>
        public string WebhookUrl
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            get => _client.BaseAddress.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("WebhookClient");
                }

                if (!Regex.IsMatch(value, @"https:\/\/discord\.com\/api\/(v\d+\/)?webhooks\/\d{17,19}\/.{68}"))
                {
                    throw new Exception("Please provide a valid webhook URL.");
                }

                _client.BaseAddress = new Uri(value);
            }
        }

        /// <summary>
        ///     Executes this webhook.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="profile">Overrides the webhook profile.</param>
        /// <returns>The Message ID.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
        public async Task<ulong> ExecuteAsync(WebhookMessage message, WebhookProfile? profile = null)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
            var payload = new Payload()
            {
                Content = message.Content,
                Embeds = message.Embeds?.ToArray(),
                Username = profile?.Username,
                AvatarUrl = profile?.AvatarUrl
            };
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
            payload.Validate();

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json")),
                RequestUri = new Uri(WebhookUrl + "?wait=true")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.\n\n{await response.Content.ReadAsStringAsync()}");
            }

#pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'ulong ulong.Parse(string s)'.
            return ulong.Parse(JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString());
#pragma warning restore CS8604 // Possible null reference argument for parameter 's' in 'ulong ulong.Parse(string s)'.
        }

        /// <summary>
        ///     Executes this webhook.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="attachment">Attachment to send.</param>
        /// <param name="profile">Overrides the webhook profile.</param>
        /// <returns>The Message ID.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public async Task<ulong> ExecuteAsync(WebhookMessage message, WebhookAttachment attachment, WebhookProfile profile = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
            var payload = new Payload()
            {
                Content = message.Content,
                Embeds = message.Embeds?.ToArray(),
                Username = profile?.Username,
                AvatarUrl = profile?.AvatarUrl,
                Attachments = new[] { attachment }
            };
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
            payload.Validate();

            var content = new MultipartFormDataContent
            {
                {
                    new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json")),
                    "payload_json"
                },
                {
                    new ByteArrayContent(attachment.FileData),
                    $"files[{attachment.Id}]",
                    attachment.FileName
                }
            };

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(WebhookUrl + "?wait=true")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.\n\n{await response.Content.ReadAsStringAsync()}");
            }

#pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'ulong ulong.Parse(string s)'.
            return ulong.Parse(JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString());
#pragma warning restore CS8604 // Possible null reference argument for parameter 's' in 'ulong ulong.Parse(string s)'.
        }

        /// <summary>
        ///     Executes this webhook.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="attachments">Attachments to send.</param>
        /// <param name="profile">Overrides the webhook profile.</param>
        /// <returns>The Message ID.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public async Task<ulong> ExecuteAsync(WebhookMessage message, IEnumerable<WebhookAttachment> attachments, WebhookProfile profile = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
            var payload = new Payload()
            {
                Content = message.Content,
                Embeds = message.Embeds?.ToArray(),
                Username = profile?.Username,
                AvatarUrl = profile?.AvatarUrl,
                Attachments = attachments.Select(x => new WebhookAttachment(x, attachments.ToList().IndexOf(x))).ToArray()
            };
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8601 // Possible null reference assignment.
            payload.Validate();

            var content = new MultipartFormDataContent()
            {
                {
                    new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json")),
                    "payload_json"
                }
            };

            foreach (var attachment in payload.Attachments)
            {
                content.Add(new ByteArrayContent(attachment.FileData), $"files[{attachment.Id}]", attachment.FileName);
            }

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(WebhookUrl + "?wait=true")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.\n\n{await response.Content.ReadAsStringAsync()}");
            }

#pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'ulong ulong.Parse(string s)'.
            return ulong.Parse(JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetString());
#pragma warning restore CS8604 // Possible null reference argument for parameter 's' in 'ulong ulong.Parse(string s)'.
        }

        /// <summary>
        ///     Overwrites a message.
        /// </summary>
        /// <param name="messageId">Message ID of the message to edit.</param>
        /// <param name="newMessage">The new message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
        public async Task EditMessageAsync(ulong messageId, WebhookMessage newMessage)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            var payload = new Payload()
            {
                Content = newMessage.Content,
                Embeds = newMessage.Embeds?.ToArray()
            };
#pragma warning restore CS8601 // Possible null reference assignment.
            payload.Validate();

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                Content = new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json")),
                RequestUri = new Uri(WebhookUrl + $"/messages/{messageId}")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.\n\n{await response.Content.ReadAsStringAsync()}");
            }
        }

        /// <summary>
        ///     Overwrites a message.
        /// </summary>
        /// <param name="messageId">Message ID of the message to edit.</param>
        /// <param name="newMessage">The new message.</param>
        /// <param name="newAttachment">Attachment to send.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
        public async Task EditMessageAsync(ulong messageId, WebhookMessage newMessage, WebhookAttachment newAttachment)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            var payload = new Payload()
            {
                Content = newMessage.Content,
                Embeds = newMessage.Embeds?.ToArray(),
                Attachments = new[] { newAttachment }
            };
#pragma warning restore CS8601 // Possible null reference assignment.
            payload.Validate();

            var content = new MultipartFormDataContent()
            {
                {
                    new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json")),
                    "payload_json"
                },
                {
                    new ByteArrayContent(newAttachment.FileData),
                    $"files[{newAttachment.Id}]",
                    newAttachment.FileName
                }
            };

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(WebhookUrl + $"/messages/{messageId}")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.");
            }
        }

        /// <summary>
        ///     Overwrites a message.
        /// </summary>
        /// <param name="messageId">Message ID of the message to edit.</param>
        /// <param name="newMessage">The new message.</param>
        /// <param name="newAttachments">Attachments to send.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
        public async Task EditMessageAsync(ulong messageId, WebhookMessage newMessage, IEnumerable<WebhookAttachment> newAttachments)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            var payload = new Payload()
            {
                Content = newMessage.Content,
                Embeds = newMessage.Embeds?.ToArray(),
                Attachments = newAttachments.Select(x => new WebhookAttachment(x, newAttachments.ToList().IndexOf(x))).ToArray()
            };
#pragma warning restore CS8601 // Possible null reference assignment.
            payload.Validate();

            var content = new MultipartFormDataContent()
            {
                {
                    new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json")),
                    "payload_json"
                }
            };

            foreach (var attachment in payload.Attachments)
            {
                content.Add(new ByteArrayContent(attachment.FileData), $"files[{attachment.Id}]", attachment.FileName);
            }

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(WebhookUrl + $"/messages/{messageId}")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.");
            }
        }

        /// <summary>
        ///     Deletes a message.
        /// </summary>
        /// <param name="messageId">Message ID of the message to edit.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the client is disposed.</exception>
        /// <exception cref="Exception">Thrown when the webhook fails to execute.</exception>
        public async Task DeleteMessageAsync(ulong messageId)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("WebhookClient");
            }

            var response = await _client.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(WebhookUrl + $"/messages/{messageId}")
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.\n\n{await response.Content.ReadAsStringAsync()}");
            }
        }

        public void Dispose()
        {
            _client.Dispose();
            _isDisposed = true;

            GC.SuppressFinalize(this);
        }

        private class Payload
        {
            [JsonPropertyName("content")]
#pragma warning disable CS8618 // Non-nullable property 'Content' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
            public string Content { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Content' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

            [JsonPropertyName("username")]
#pragma warning disable CS8618 // Non-nullable property 'Username' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
            public string Username { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Username' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

            [JsonPropertyName("avatar_url")]
#pragma warning disable CS8618 // Non-nullable property 'AvatarUrl' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
            public string AvatarUrl { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'AvatarUrl' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

            [JsonPropertyName("tts")]
            public bool Tts { get; set; }

            [JsonPropertyName("embeds")]
#pragma warning disable CS8618 // Non-nullable property 'Embeds' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
            public Embed[] Embeds { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Embeds' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

            [JsonPropertyName("attachments")]
#pragma warning disable CS8618 // Non-nullable property 'Attachments' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
            public WebhookAttachment[] Attachments { get; set; }
#pragma warning restore CS8618 // Non-nullable property 'Attachments' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

            public void Validate()
            {
                if (Content?.Length > 2000)
                {
                    throw new ArgumentException("Content must be less than 2000 characters.", nameof(Content));
                }

                if (Username?.Length > 80)
                {
                    throw new ArgumentException("Username must be less than 80 characters.", nameof(Username));
                }

                if (string.IsNullOrWhiteSpace(Content) && Embeds.Length == 0 && Attachments.Length == 0)
                {
                    throw new ArgumentException("Content, embeds, or attachments must be provided.", nameof(Content));
                }

                if (Embeds != null)
                {
                    if (Embeds.Any(x => x.Title.Length > 256))
                    {
                        throw new ArgumentException("Embed title must be less than 256 characters.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Description.Length > 4096))
                    {
                        throw new ArgumentException("Embed description must be less than 4096 characters.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Fields.Count > 25))
                    {
                        throw new ArgumentException("Embed must have less than 25 fields.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Fields.Any(y => y.Name.Length > 256)))
                    {
                        throw new ArgumentException("Embed field name must be less than 256 characters.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Fields.Any(y => y.Value.Length > 1024)))
                    {
                        throw new ArgumentException("Embed field value must be less than 1024 characters.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Footer.Text.Length > 2048))
                    {
                        throw new ArgumentException("Embed footer text must be less than 2048 characters.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Author.Name.Length > 256))
                    {
                        throw new ArgumentException("Embed author name must be less than 256 characters.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => string.IsNullOrEmpty(x.Title) && string.IsNullOrEmpty(x.Description) && x.Fields.Count == 0 && x.Image == null && x.Thumbnail == null && x.Footer == null && x.Author == null))
                    {
                        throw new ArgumentException("Embed must have a title, description, fields, image, thumbnail, footer, or author.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Fields.Any(y => string.IsNullOrEmpty(y.Name) || string.IsNullOrEmpty(y.Value))))
                    {
                        throw new ArgumentException("Embed field must have a name and value.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Footer != null && string.IsNullOrEmpty(x.Footer.Text)))
                    {
                        throw new ArgumentException("Embed footer must have text.", nameof(Embeds));
                    }

                    if (Embeds.Any(x => x.Author != null && string.IsNullOrEmpty(x.Author.Name)))
                    {
                        throw new ArgumentException("Embed author must have a name.", nameof(Embeds));
                    }
                }
            }
        }
    }
}