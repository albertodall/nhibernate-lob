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

		public IStreamCompressor Compression
		{
			get { return _compression; }
		}

		public override System.Type ReturnedClass
		{
			get { return typeof (Blob); }
		}

		public virtual void SetParameterValues(IDictionary<string, string> parameters)
		{
			Parameters.GetBlobSettings(parameters, out _compression);
		}

		protected override object CreateLobInstance(IExternalBlobConnection connection, byte[] identifier)
		{
			return new ExternalBlob(connection, identifier, _compression);
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