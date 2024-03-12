namespace HexaEngine.Web
{
    using System;
    using System.IO;

    /// <summary>
    /// Provides extension methods for the <see cref="Stream"/> class.
    /// </summary>
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(
         this Stream source,
         Stream destination,
         int bufferSize,
         IProgress<long> progress = null,
         CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            ArgumentNullException.ThrowIfNull(destination);
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));

            byte[] buffer = bufferSize >= 0 ? new byte[bufferSize] : throw new ArgumentOutOfRangeException(nameof(bufferSize));
            long totalBytesRead = 0;

            while (true)
            {
                int num = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                int bytesRead;
                if ((bytesRead = num) != 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                    totalBytesRead += bytesRead;
                    progress?.Report(totalBytesRead);
                }
                else
                    break;
            }
            buffer = null;
        }
    }
}