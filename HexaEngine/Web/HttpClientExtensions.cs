namespace HexaEngine.Web
{
    using HexaEngine.Web.Caching;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public static class HttpClientExtensions
    {
        public static async Task DownloadAsync(
          this HttpClient client,
          string requestUri,
          Stream destination,
          IProgress<float>? progress = null,
          CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            long? contentLength = response.Content.Headers.ContentLength;

            using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
            if (progress == null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination, cancellationToken);
                return;
            }

            Progress<long> relativeProgress = new(totalBytes => progress.Report(totalBytes / (float)contentLength.Value));
            await download.CopyToAsync(destination, 8192, relativeProgress, cancellationToken);
            progress.Report(1f);
        }

        private static unsafe bool TryGetCached(string requestUri, Stream destination, CancellationToken cancellationToken = default)
        {
            byte* data;
            uint size;
            if (WebCache.Shared.TryGet(requestUri, &data, &size))
            {
                destination.Write(new Span<byte>(data, (int)size));
                Marshal.FreeHGlobal((nint)data);
                return true;
            }
            return false;
        }

        public static async Task DownloadAsyncCached(this HttpClient client, Uri requestUri, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            if (TryGetCached(requestUri.AbsoluteUri, destination, cancellationToken))
            {
                return;
            }

            using HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            long? contentLength = response.Content.Headers.ContentLength;

            using MemoryStream ms = new();
            using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
            if (progress == null || contentLength != null)
            {
                await download.CopyToAsync(ms, cancellationToken);

                ms.Position = 0;
                ms.CopyTo(destination);

                if (response.Content.Headers.Expires != null)
                {
                    WebCache.Shared.Set(requestUri.AbsoluteUri, ms.ToArray(), response.Content.Headers.Expires.Value.LocalDateTime);
                }
                else
                {
                    WebCache.Shared.Set(requestUri.AbsoluteUri, ms.ToArray());
                }

                return;
            }

            Progress<long> relativeProgress = new(totalBytes => progress.Report(totalBytes / (float)contentLength.Value));
            await download.CopyToAsync(ms, 8192, relativeProgress, cancellationToken);
            progress.Report(1f);

            ms.Position = 0;
            ms.CopyTo(destination);

            WebCache.Shared.Set(requestUri.AbsoluteUri, ms.ToArray());
        }

        public static async Task DownloadAsyncCached(this HttpClient client, string requestUri, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            if (TryGetCached(requestUri, destination, cancellationToken))
            {
                return;
            }

            using HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            long? contentLength = response.Content.Headers.ContentLength;

            using MemoryStream ms = new();
            using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
            if (progress == null || contentLength != null)
            {
                await download.CopyToAsync(ms, cancellationToken);

                ms.Position = 0;
                ms.CopyTo(destination);

                WebCache.Shared.Set(requestUri, ms.ToArray());
                return;
            }

            Progress<long> relativeProgress = new(totalBytes => progress.Report(totalBytes / (float)contentLength.Value));
            await download.CopyToAsync(ms, 8192, relativeProgress, cancellationToken);
            progress.Report(1f);

            ms.Position = 0;
            ms.CopyTo(destination);

            WebCache.Shared.Set(requestUri, ms.ToArray());
        }
    }
}