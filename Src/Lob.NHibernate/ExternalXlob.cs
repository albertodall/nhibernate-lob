using System;
using System.Linq;
using System.Xml;
using Lob.Model;
using Lob.NHibernate.Compression;

namespace Lob.NHibernate
{
	public class ExternalXlob : Xlob
	{
		readonly IXmlCompressor _compression;
		readonly IExternalBlobConnection _connection;
		readonly byte[] _identifier;

		public ExternalXlob(IExternalBlobConnection connection, byte[] identifier, IXmlCompressor compression)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (identifier == null) throw new ArgumentNullException("identifier");
			if (compression == null) throw new ArgumentNullException("compression");
			_connection = connection;
			_identifier = identifier;
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

		public IXmlCompressor Compression
		{
			get { return _compression; }
		}

		public override XmlReader OpenReader()
		{
			return _compression.GetDecompressor(_connection.OpenReader(_identifier));
		}

		public override bool Equals(Xlob xlob)
		{
			if (xlob == null) return false;
			if (xlob == this) return true;
			var ex = xlob as ExternalXlob;
			if (ex == null || !Connection.Equals(ex.Connection) || _identifier.Length != ex._identifier.Length ||
			    (_compression != ex._compression && _compression != null && !_compression.Equals(ex._compression))) return false;
			byte[] a = _identifier, b = ex._identifier;
			return !a.Where((t, i) => t != b[i]).Any();
		}
	}
}