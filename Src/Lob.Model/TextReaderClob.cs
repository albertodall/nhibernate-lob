using System;
using System.IO;
using System.Text;

namespace Lob.Model
{
	public class TextReaderClob : Clob
	{
		readonly long _initialPosition;
		readonly TextReader _reader;
		bool _alreadyOpen;
		bool _needRestart;

		public TextReaderClob(Stream stream, Encoding encoding)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (encoding == null) throw new ArgumentNullException("encoding");
			try
			{
				_initialPosition = stream.CanSeek ? stream.Position : -1L;
			}
			catch
			{
				_initialPosition = -1L;
			}
			_reader = new StreamReader(stream, encoding);
		}

		public TextReaderClob(TextReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			var sr = reader as StreamReader;
			if (sr != null)
				try
				{
					_initialPosition = sr.BaseStream.CanSeek ? sr.BaseStream.Position : -1L;
				}
				catch
				{
					_initialPosition = -1L;
				}
			else
				_initialPosition = -1L;
			_reader = reader;
		}

		public override TextReader OpenReader()
		{
			lock (this)
			{
				if (_needRestart && _initialPosition < 0L)
				{
					throw new Exception("The underlying TextReader cannot be reset. It has already been opened.");
				}
				if (_alreadyOpen)
				{
					throw new Exception("There's already a reader open on this Clob. Close the first reader before requesting a new one.");
				}
				if (_needRestart)
				{
					((StreamReader) _reader).BaseStream.Seek(_initialPosition, SeekOrigin.Begin);
				}
				_alreadyOpen = true;
			}
			return new ClobReader(this);
		}

		public override bool Equals(Clob clob)
		{
			var rc = clob as TextReaderClob;
			if (rc == null) return false;
			if (rc == this) return true;
			return _reader == rc._reader;
		}

		#region Read-only stream wrapper class

		class ClobReader : TextReader
		{
			TextReaderClob _clob;

			public ClobReader(TextReaderClob clob)
			{
				_clob = clob;
			}

			void ThrowClosed()
			{
				if (_clob == null) throw new Exception("The TextReader is already closed.");
			}

			public override void Close()
			{
				Dispose(true);
			}

			protected override void Dispose(bool disposing)
			{
				if (_clob != null)
					lock (_clob)
					{
						_clob._alreadyOpen = false;
						_clob = null;
					}
			}

			public override int Peek()
			{
				ThrowClosed();
				return _clob._reader.Peek();
			}

			public override int Read()
			{
				ThrowClosed();
				_clob._needRestart = true;
				return _clob._reader.Read();
			}

			public override int Read(char[] buffer, int index, int count)
			{
				ThrowClosed();
				if (index > 0 || count > 0) _clob._needRestart = true;
				return _clob._reader.Read(buffer, index, count);
			}

			public override int ReadBlock(char[] buffer, int index, int count)
			{
				ThrowClosed();
				if (index > 0 || count > 0) _clob._needRestart = true;
				return _clob._reader.ReadBlock(buffer, index, count);
			}

			public override string ReadLine()
			{
				ThrowClosed();
				_clob._needRestart = true;
				return _clob._reader.ReadLine();
			}

			public override string ReadToEnd()
			{
				ThrowClosed();
				_clob._needRestart = true;
				return _clob._reader.ReadToEnd();
			}
		}

		#endregion
	}
}