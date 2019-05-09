using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace ThrottlingGateway.Middleware
{
    public class LinkedBuffer : Stream
    {
        public class LinkedBufferDisposedException : ObjectDisposedException
        {
            private const string _name = "LinkedBuffer";

            public LinkedBufferDisposedException()
                : base(_name)
            {
            }

            public LinkedBufferDisposedException(string message)
                : base(_name, message)
            {
            }

            public LinkedBufferDisposedException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            protected LinkedBufferDisposedException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        private readonly List<byte[]> _memoryBlocks = new List<byte[]>();
        private long _length;
        public int BlockSize { get; private set; }
        private bool _disposed;
        private long _position;

        public LinkedBuffer(int blockSize = 1024 * 1024)
        {
            BlockSize = blockSize;
        }

        protected override void Dispose(bool disposing)
        {
            _memoryBlocks.Clear();

            base.Dispose(disposing);

            _disposed = true;
        }

        public override void Flush()
        {
        }

        private void ValidateNewPosition(long offset)
        {
            if (offset > Length || offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed) throw new LinkedBufferDisposedException();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    ValidateNewPosition(offset);
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    ValidateNewPosition(Position + offset);
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    ValidateNewPosition(Length + offset);
                    Position = Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin");
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            if (_disposed) throw new LinkedBufferDisposedException();

            var blocksNeeded = value / BlockSize + (value % BlockSize > 0 ? 1 : 0);

            var delta = blocksNeeded - _memoryBlocks.Count;

            for (int i = 0; i < delta; i++)
            {
                _memoryBlocks.Add(GetMemoryBlock());
            }

            _length = value;
        }

        private byte[] GetMemoryBlock()
        {
            return new byte[BlockSize];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new LinkedBufferDisposedException();

            if (Position >= Length || count == 0)
            {
                return 0;
            }

            var bufNum = (int)(Position / BlockSize);
            var posInBuf = (int)(Position % BlockSize);
            var bytesToRead = (int)Math.Min((long)count, Length - Position);
            var bytesToReadInCurrentBuff = (int)BlockSize - posInBuf;
            var bytesLeft = bytesToRead;

            while (bytesLeft > 0)
            {
                var bytesToCopy = Math.Min(bytesToReadInCurrentBuff, bytesLeft);
                var buf = _memoryBlocks[bufNum];

                Buffer.BlockCopy(buf, posInBuf, buffer, offset, bytesToCopy);

                Position += bytesToCopy;
                bytesToReadInCurrentBuff = BlockSize;
                offset += bytesToCopy;
                bytesLeft -= bytesToCopy;
                bufNum++;
                posInBuf = 0;
            }

            return bytesToRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new LinkedBufferDisposedException();

            long position = Position;

            if (position + count > Length)
            {
                SetLength(position + count);
            }

            var bufNum = (int)(Position / BlockSize);
            var posInBuf = (int)(Position % BlockSize);
            var bytesToWriteInCurrentBuff = BlockSize - posInBuf;
            var bytesLeft = count;

            while (bytesLeft > 0)
            {
                var bytesToCopy = Math.Min(bytesToWriteInCurrentBuff, bytesLeft);
                var buf = _memoryBlocks[bufNum];

                Buffer.BlockCopy(buffer, offset, buf, posInBuf, bytesToCopy);

                bytesToWriteInCurrentBuff = BlockSize;
                position += bytesToCopy;
                offset += bytesToCopy;
                bytesLeft -= bytesToCopy;
                bufNum++;
                posInBuf = 0;
            }

            Position = position;
        }

        public override bool CanRead
        {
            get
            {
                if (_disposed) throw new LinkedBufferDisposedException();
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (_disposed) throw new LinkedBufferDisposedException();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_disposed) throw new LinkedBufferDisposedException();
                return true;
            }
        }

        public override long Length
        {
            get
            {
                if (_disposed) throw new LinkedBufferDisposedException();
                return _length;
            }
        }

        public override long Position
        {
            get
            {
                if (_disposed) throw new LinkedBufferDisposedException();
                return _position;
            }
            set
            {
                if (_disposed) throw new LinkedBufferDisposedException();
                _position = value;
            }
        }
    }
}
