namespace HexaEngine.Core.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Provides a stream that can be reused after being closed, allowing for reopening of the underlying file.
    /// </summary>
    public class ReusableFileStream : Stream
    {
        private readonly string file;
        private readonly FileMode mode;
        private readonly FileAccess access;
        private readonly FileShare share;
        private Stream stream;
        private bool isOpen = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReusableFileStream"/> class with the specified file path, file mode, file access, and file share settings.
        /// </summary>
        /// <param name="file">The path to the file to be opened or created.</param>
        /// <param name="mode">One of the enumeration values that determines how the file is opened or created.</param>
        /// <param name="access">A bitwise combination of the enumeration values that determines how the file can be accessed by the <see cref="Stream"/>.</param>
        /// <param name="share">A bitwise combination of the enumeration values that determines how the file will be shared by processes.</param>
        public ReusableFileStream(string file, FileMode mode, FileAccess access, FileShare share)
        {
            this.file = file;
            this.mode = mode;
            this.access = access;
            this.share = share;
            stream = File.Open(file, mode, access, share);
            isOpen = true;
        }

        /// <inheritdoc/>
        public override bool CanRead => stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => stream.Length;

        /// <inheritdoc/>
        public override long Position { get => stream.Position; set => stream.Position = value; }

        /// <summary>
        /// Gets a value indicating whether the stream is currently open.
        /// </summary>
        public bool IsOpen => isOpen;

        /// <summary>
        /// Gets a value indicating whether the file can be reopened.
        /// </summary>
        public bool CanReOpen => File.Exists(file);

        /// <inheritdoc/>
        public override void Flush()
        {
            stream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Attempts to reopen the stream if it is closed and the file still exists.
        /// </summary>
        /// <returns><see langword="true"/> if the stream was successfully reopened; otherwise, <see langword="false"/>.</returns>
        public bool ReOpen()
        {
            if (isOpen)
            {
                return true;
            }

            if (!File.Exists(file))
            {
                return false;
            }

            GC.ReRegisterForFinalize(this);
            stream = File.Open(file, mode, access, share);
            isOpen = true;
            return true;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!isOpen)
            {
                return;
            }
            isOpen = false;
            base.Close();
            stream.Close();
        }
    }
}