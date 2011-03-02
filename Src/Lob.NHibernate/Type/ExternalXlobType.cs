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

		public virtual IXmlCompressor Compression
		{
			get { return _compression; }
			protected set { _compression = value; }
		}

		public override System.Type ReturnedClass
		{
			get { return typeof (Xlob); }
		}

		public virtual void SetParameterValues(IDictionary<string, string> parameters)
		{
			int length;
			IXmlCompressor c;
			Parameters.GetXlobSettings(parameters, out c, out length);
			if (c != null) _compression = c;
			PayloadLength = length;
		}

		protected override object CreateLobInstance(IExternalBlobConnection connection, byte[] payload)
		{
			return new ExternalXlob(connection, payload, _compression);
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