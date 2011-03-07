using System.Collections.Generic;
using System.IO;
using System.Text;
using Lob.Model;
using Lob.NHibernate.Compression;
using Lob.NHibernate.Support;
using NHibernate.UserTypes;

namespace Lob.NHibernate.Type
{
	public class ExternalClobType : AbstractExternalBlobType, IParameterizedType
	{
		IStreamCompressor _compression;
		Encoding _encoding;

		public ExternalClobType()
		{
			_encoding = Encoding.UTF8;
		}

		public virtual IStreamCompressor Compression
		{
			get { return _compression; }
			protected set { _compression = value; }
		}

		public Encoding Encoding
		{
			get { return _encoding; }
		}

		public override System.Type ReturnedClass
		{
			get { return typeof (Clob); }
		}

		public virtual void SetParameterValues(IDictionary<string, string> parameters)
		{
			int length;
			Parameters.GetClobSettings(parameters, out _encoding, out _compression, out length);
			PayloadLength = length;
		}

		protected override object CreateLobInstance(IExternalBlobConnection connection, byte[] payload)
		{
			return new ExternalClob(connection, payload, _encoding, _compression);
		}

		protected override bool ExtractLobData(object lob, out IExternalBlobConnection connection, out byte[] identifier)
		{
			var clob = lob as ExternalClob;
			if (clob == null)
			{
				var persistedLob = lob as IPersistedLob;

				if (persistedLob != null && persistedLob.IsPersisted)
				{
					identifier = persistedLob.GetPersistedIdentifier();
					connection = (IExternalBlobConnection) persistedLob.GetExternalStore();
					return true;
				}

				connection = null;
				identifier = null;
				return false;
			}
			connection = clob.Connection;
			identifier = clob.Identifier;
			return true;
		}

		protected override void WriteLobTo(object lob, Stream output)
		{
			var clob = lob as Clob;
			if (clob == null) return;
			if (_compression == null)
				using (var sw = new StreamWriter(output, _encoding))
					clob.WriteTo(sw);
			else
				using (Stream cs = _compression.GetCompressor(output))
				using (var sw = new StreamWriter(cs, _encoding))
					clob.WriteTo(sw);
		}

		public override bool Equals(object obj)
		{
			if (obj == this) return true;
			if (!base.Equals(obj)) return false;
			var t = obj as ExternalClobType;
			if (t == null) return false;
			if (t._compression != _compression) return false;
			if (t._encoding != _encoding) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}