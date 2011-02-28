using System;
using System.Collections.Generic;
using System.IO;

namespace Lob.NHibernate.Providers.ByteArray
{
	public class ByteArrayConnection : AbstractExternalBlobConnection
	{
		public override int BlobIdentifierLength
		{
			get { return Int32.MaxValue; }
		}

		public override void Delete(byte[] blobIdentifier)
		{
		}

		public override Stream OpenReader(byte[] blobIdentifier)
		{
			return new MemoryStream(blobIdentifier);
		}

		public override ExternalBlobWriter OpenWriter()
		{
			return new ByteArrayWriter();
		}

		public override void GarbageCollect(IEnumerable<byte[]> livingBlobIdentifiers)
		{
		}

		public override bool Equals(IExternalBlobConnection connection)
		{
			return connection is ByteArrayConnection;
		}
	}
}