using System.IO;

namespace NHibernate.Lob.External.ByteArray
{
	public class ByteArrayWriter : ExternalBlobWriter
	{
		readonly MemoryStream _stream = new MemoryStream();

		public override void Flush()
		{
			_stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public override bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public override long Length
		{
			get { return _stream.Length; }
		}

		public override long Position
		{
			get { return _stream.Position; }
			set { _stream.Position = value; }
		}

		public override byte[] Commit()
		{
			return _stream.ToArray();
		}
	}
}