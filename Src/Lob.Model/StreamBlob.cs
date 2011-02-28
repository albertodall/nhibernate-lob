using System;
using System.IO;

namespace Lob.Model
{
	public class StreamBlob : Blob
	{
		readonly long _initialPosition;
		readonly Stream _stream;
		bool _alreadyOpen;
		bool _needRestart;

		public StreamBlob(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (!stream.CanRead) throw new NotSupportedException("Stream cannot read. Blobs are read-only.");

			_stream = stream;
			try
			{
				_initialPosition = stream.CanSeek ? stream.Position : -1L;
			}
			catch
			{
				_initialPosition = -1L;
			}
		}

		public Stream UnderlyingStream
		{
			get { return _stream; }
		}

		public override Stream OpenReader()
		{
			lock (this)
			{
				if (_needRestart && _initialPosition < 0L)
					throw new Exception("The underlying Stream cannot be reset. It has already been opened.");
				if (_alreadyOpen)
					throw new Exception("There's already a reader Stream open on this Blob. Close the first Stream before requesting a new one.");
				if (_needRestart)
					_stream.Seek(_initialPosition, SeekOrigin.Begin);
				_alreadyOpen = true;
			}
			return new BlobStream(this);
		}

		public override bool Equals(Blob blob)
		{
			var sb = blob as StreamBlob;
			if (sb != null)
			{
				if (_stream == sb._stream) return true;
				var fsa = _stream as FileStream;
				if (fsa == null) return false;
				var fsb = sb._stream as FileStream;
				if (fsb == null) return false;
				try
				{
					return fsa.Name.Equals(fsb.Name);
				}
				catch
				{
					return false;
				}
			}

			var fb = blob as FileBlob;
			if (fb == null) return false;

			var fs = _stream as FileStream;
			if (fs == null) return false;
			try
			{
				return fb.Filename.Equals(fs.Name);
			}
			catch
			{
				return false;
			}
		}

		#region Read-only stream wrapper class

		class BlobStream : Stream
		{
			StreamBlob _blob;

			public BlobStream(StreamBlob blob)
			{
				_blob = blob;
			}

			public override bool CanRead
			{
				get { return _blob != null ? _blob._stream.CanRead : false; }
			}

			public override bool CanSeek
			{
				get
				{
					if (_blob == null) return false;
					return _blob._stream.CanSeek;
				}
			}

			public override bool CanWrite
			{
				get { return false; }
			}

			public override long Length
			{
				get
				{
					ThrowClosed();
					return _blob._stream.Length;
				}
			}

			public override long Position
			{
				get
				{
					ThrowClosed();
					return _blob._stream.Position;
				}
				set
				{
					ThrowClosed();
					_blob._stream.Position = value;
					_blob._needRestart = true;
				}
			}

			public override bool CanTimeout
			{
				get { return _blob == null ? false : _blob._stream.CanTimeout; }
			}

			public override int ReadTimeout
			{
				get
				{
					ThrowClosed();
					return _blob._stream.ReadTimeout;
				}
				set
				{
					ThrowClosed();
					_blob._stream.ReadTimeout = value;
				}
			}

			public override int WriteTimeout
			{
				get { throw new NotSupportedException(); }
				set { throw new NotSupportedException(); }
			}

			void ThrowClosed()
			{
				if (_blob == null) throw new Exception("The Stream is already closed.");
			}

			public override void Flush()
			{
				throw new NotSupportedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				ThrowClosed();
				int i = _blob._stream.Read(buffer, offset, count);
				if (i > 0) _blob._needRestart = true;
				return i;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				ThrowClosed();
				_blob._needRestart = true;
				return _blob._stream.Seek(offset, origin);
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				ThrowClosed();
				_blob._needRestart = true;
				return _blob._stream.BeginRead(buffer, offset, count, callback, state);
			}

			public override int EndRead(IAsyncResult asyncResult)
			{
				ThrowClosed();
				return _blob._stream.EndRead(asyncResult);
			}

			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				throw new NotSupportedException();
			}

			public override void EndWrite(IAsyncResult asyncResult)
			{
				throw new NotSupportedException();
			}

			public override void Close()
			{
				Dispose(true);
			}

			protected override void Dispose(bool disposing)
			{
				if (_blob != null)
					lock (_blob)
					{
						_blob._alreadyOpen = false;
						_blob = null;
					}
			}

			public override int ReadByte()
			{
				ThrowClosed();
				int i = _blob._stream.ReadByte();
				_blob._needRestart = true;
				return i;
			}

			public override void WriteByte(byte value)
			{
				throw new NotSupportedException();
			}
		}

		#endregion
	}
}