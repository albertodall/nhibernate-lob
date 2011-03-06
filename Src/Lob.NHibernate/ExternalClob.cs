using System;
using System.IO;
using System.Linq;
using System.Text;
using Lob.Model;
using Lob.NHibernate.Compression;

namespace Lob.NHibernate
{
	public class ExternalClob : Clob
	{
		readonly IStreamCompressor _compression;
		readonly IExternalBlobConnection _connection;
		readonly Encoding _encoding;
		readonly byte[] _identifier;

		public ExternalClob(IExternalBlobConnection connection, byte[] identifier, Encoding encoding)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (identifier == null) throw new ArgumentNullException("identifier");
			if (encoding == null) throw new ArgumentNullException("encoding");
			_connection = connection;
			_identifier = identifier;
			_encoding = encoding;
		}

		public ExternalClob(IExternalBlobConnection connection, byte[] identifier, Encoding encoding, IStreamCompressor compression) : this(connection, identifier, encoding)
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

		public Encoding Encoding
		{
			get { return _encoding; }
		}

		public override TextReader OpenReader()
		{
			return new StreamReader(
				_compression == null ? _connection.OpenReader(_identifier) : _compression.GetDecompressor(_connection.OpenReader(_identifier)),
				_encoding
				);
		}

		public override bool Equals(Clob clob)
		{
			if (clob == null) return false;
			if (clob == this) return true;
			var ec = clob as ExternalClob;
			if (ec == null || !Connection.Equals(ec.Connection) || _identifier.Length != ec._identifier.Length ||
			    !_encoding.Equals(ec._encoding) ||
			    (_compression != ec._compression && _compression != null && !_compression.Equals(ec._compression))) return false;
			byte[] a = _identifier, b = ec._identifier;
			return !a.Where((t, i) => t != b[i]).Any();
		}

		protected override bool GetIsPersisted()
		{
			return true;
		}

		protected override object GetExternalSource()
		{
			return _connection;
		}

		protected override byte[] GetPersistedIdentifier()
		{
			return _identifier;
		}

		protected override void SetPersistedIdentifier(byte[] contents, object externalSource)
		{
			throw new InvalidOperationException("You can not set the persisted identifier of an ExternalBlob (it's already been persisted)");
		}
	}
}