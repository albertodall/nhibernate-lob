using System.Collections.Generic;
using System.IO;
using Lob.Model;
using Lob.NHibernate.Compression;
using Lob.NHibernate.Support;
using NHibernate.UserTypes;

namespace Lob.NHibernate.Type
{
	public class ExternalBlobType : AbstractExternalBlobType, IParameterizedType
	{
		IStreamCompressor _compression;

		public virtual IStreamCompressor Compression
		{
			get { return _compression; }
			protected set { _compression = value; }
		}

		public override System.Type ReturnedClass
		{
			get { return typeof (Blob); }
		}

		public virtual void SetParameterValues(IDictionary<string, string> parameters)
		{
			int length;
			Parameters.GetBlobSettings(parameters, out _compression, out length);
			PayloadLength = length;
		}

		protected override object CreateLobInstance(IExternalBlobConnection connection, byte[] payload)
		{
			return new ExternalBlob(connection, payload, _compression);
		}

		protected override bool ExtractLobData(object lob, out IExternalBlobConnection connection, out byte[] identifier)
		{
			var blob = lob as ExternalBlob;
			if (blob == null)
			{
				connection = null;
				identifier = null;
				return false;
			}
			connection = blob.Connection;
			identifier = blob.Identifier;
			return true;
		}

		protected override void WriteLobTo(object lob, Stream output)
		{
			var blob = lob as Blob;
			if (blob == null) return;
			if (_compression == null)
				blob.WriteTo(output);
			else
				using (Stream cs = _compression.GetCompressor(output))
					blob.WriteTo(cs);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj) && (_compression == ((ExternalBlobType) obj)._compression || (_compression != null && _compression.Equals(((ExternalBlobType) obj)._compression)));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}