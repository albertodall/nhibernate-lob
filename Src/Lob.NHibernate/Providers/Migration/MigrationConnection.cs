using System;
using System.Collections.Generic;
using System.IO;

namespace Lob.NHibernate.Providers.Migration
{
	public class MigrationConnection : IExternalBlobConnection
	{
		readonly IExternalBlobConnection _from;
		readonly IExternalBlobConnection _to;

		public MigrationConnection(IExternalBlobConnection from, IExternalBlobConnection to)
		{
			_from = from;
			_to = to;
		}

		public void Dispose()
		{
			_from.Dispose();
			_to.Dispose();
		}

		public int BlobIdentifierLength
		{
			get { return Math.Max(_from.BlobIdentifierLength, _to.BlobIdentifierLength); }
		}

		public bool DisassembleRequiresExternalBlob
		{
			get { return false; }
		}

		public void Delete(byte[] blobIdentifier)
		{
			_from.Delete(blobIdentifier);
		}

		public Stream OpenReader(byte[] blobIdentifier)
		{
			return _from.OpenReader(blobIdentifier);
		}

		public ExternalBlobWriter OpenWriter()
		{
			return _to.OpenWriter();
		}

		public byte[] Store(Stream input)
		{
			return _to.Store(input);
		}

		public void ReadInto(byte[] blobIdentifier, Stream output)
		{
			_from.ReadInto(blobIdentifier, output);
		}

		public bool Equals(IExternalBlobConnection connection)
		{
			return _from.Equals(connection);
		}

		public bool SupportsGarbageCollection
		{
			get { return false; }
		}

		public void GarbageCollect(ICollection<byte[]> livingBlobIdentifiers)
		{
			_from.GarbageCollect(livingBlobIdentifiers);
		}
	}
}