using System.Collections.Generic;
using System.IO;
using System.Xml;
using Lob.Model;
using Lob.NHibernate.Compression;
using Lob.NHibernate.Support;
using NHibernate.UserTypes;

namespace Lob.NHibernate.Type
{
	public class ExternalXlobType : AbstractExternalBlobType, IParameterizedType
	{
		IXmlCompressor _compression;

		public ExternalXlobType()
		{
			_compression = new XmlTextCompressor();
		}

		public IXmlCompressor Compression
		{
			get { return _compression; }
		}

		public override System.Type ReturnedClass
		{
			get { return typeof (Xlob); }
		}

		public virtual void SetParameterValues(IDictionary<string, string> parameters)
		{
			IXmlCompressor c;
			Parameters.GetXlobSettings(parameters, out c);
			if (c != null) _compression = c;
		}

		protected override object CreateLobInstance(IExternalBlobConnection connection, byte[] identifier)
		{
			return new ExternalXlob(connection, identifier, _compression);
		}

		protected override bool ExtractLobData(object lob, out IExternalBlobConnection connection, out byte[] identifier)
		{
			var xlob = lob as ExternalXlob;
			if (xlob == null)
			{
				connection = null;
				identifier = null;
				return false;
			}
			connection = xlob.Connection;
			identifier = xlob.Identifier;
			return true;
		}

		protected override void WriteLobTo(object lob, Stream output)
		{
			var xlob = lob as Xlob;
			if (xlob == null) return;
			using (XmlWriter xw = _compression.GetCompressor(output))
				xlob.WriteTo(xw);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj) && _compression.Equals(((ExternalXlobType) obj)._compression);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}