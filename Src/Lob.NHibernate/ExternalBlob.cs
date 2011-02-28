using System;
using System.IO;
using System.Linq;
using Lob.Model;
using Lob.NHibernate.Compression;

namespace Lob.NHibernate
{
	public class ExternalBlob : Blob
	{
		readonly IStreamCompressor _compression;
		readonly IExternalBlobConnection _connection;
		readonly byte[] _identifier;

		public ExternalBlob(IExternalBlobConnection connection, byte[] identifier)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (identifier == null) throw new ArgumentNullException("identifier");
			_connection = connection;
			_identifier = identifier;
		}

		public ExternalBlob(IExternalBlobConnection connection, byte[] identifier, IStreamCompressor compression) : this(connection, identifier)
		{
			_compression = compression;
		}

		public IExternalBlobConnection Connection
		{
			get { return _connection; }
		}

		public byte[] Identifier
		{
			get { return _identifier; }
		}

		public IStreamCompressor Compression
		{
			get { return _compression; }
		}

		public override Stream OpenReader()
		{
			return _compression == null ? _connection.OpenReader(_identifier) : _compression.GetDecompressor(_connection.OpenReader(_identifier));
		}

		public override bool Equals(Blob blob)
		{
			if (blob == null) return false;
			if (blob == this) return true;
			var eb = blob as ExternalBlob;
			if (eb == null || !Connection.Equals(eb.Connection) || _identifier.Length != eb._identifier.Length ||
			    (_compression != eb._compression && _compression != null && !_compression.Equals(eb._compression))) return false;
			byte[] a = _identifier, b = eb._identifier;
			return !a.Where((t, i) => t != b[i]).Any();
		}
	}
}