using System;
using System.IO;

namespace Lob.NHibernate
{
	public abstract class ExternalBlobWriter : Stream
	{
		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public abstract byte[] Commit();

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException();
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			throw new NotSupportedException();
		}

		public override int ReadByte()
		{
			throw new NotSupportedException();
		}
	}
}