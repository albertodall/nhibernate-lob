﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Lob.NHibernate
{
	public interface IExternalBlobConnection : IDisposable
	{
		int BlobIdentifierLength { get; }
		bool DisassembleRequiresExternalBlob { get; }
		bool SupportsGarbageCollection { get; }
		void Delete(byte[] blobIdentifier);
		Stream OpenReader(byte[] blobIdentifier);
		ExternalBlobWriter OpenWriter();
		byte[] Store(Stream input);
		void ReadInto(byte[] blobIdentifier, Stream output);
		bool Equals(IExternalBlobConnection connection);
		void GarbageCollect(ICollection<byte[]> livingBlobIdentifiers, DateTime createdBefore);
	}
}