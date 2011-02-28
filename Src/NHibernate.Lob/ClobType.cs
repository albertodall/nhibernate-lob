using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Calyptus.Lob;
using NHibernate.Engine;
using NHibernate.Lob.Compression;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace NHibernate.Lob
{
	public class ClobType : AbstractLobType, IParameterizedType
	{
		IStreamCompressor compression;

		Encoding encoding;

		public IStreamCompressor Compression
		{
			get { return compression; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
		}

		public override System.Type ReturnedClass
		{
			get { return typeof (Clob); }
		}

		public void SetParameterValues(IDictionary<string, string> parameters)
		{
			Parameters.GetClobSettings(parameters, out encoding, out compression);
			if (compression != null && encoding == null) encoding = Encoding.UTF8;
		}

		protected override object Get(IDataReader rs, int ordinal)
		{
			if (compression == null)
				return new StringClob(rs.GetString(ordinal));
			return base.Get(rs, ordinal);
		}

		protected override object GetData(object value)
		{
			var clob = value as Clob;
			if (clob == null) return null;
			if (compression == null)
			{
				if (clob.Equals(Clob.Empty)) return "";
				var sc = clob as StringClob;
				if (sc != null) return sc.Text;
				using (var sw = new StringWriter())
				{
					clob.WriteTo(sw);
					return sw.ToString();
				}
			}
			else
			{
				var cb = clob as CompressedClob;
				if (cb != null && cb.Compression.Equals(compression)) return cb.Data;
				using (var data = new MemoryStream())
				{
					using (Stream cs = compression.GetCompressor(data))
					using (var sw = new StreamWriter(cs, encoding))
						clob.WriteTo(sw);
					return data.ToArray();
				}
			}
		}

		protected override object GetValue(object dataObj)
		{
			if (compression == null)
			{
				var str = dataObj as string;
				return str == null ? null : new StringClob(str);
			}
			else
			{
				var data = dataObj as byte[];
				if (data == null) return null;
				return new CompressedClob(data, encoding, compression);
			}
		}

		public override SqlType[] SqlTypes(IMapping mapping)
		{
			if (compression == null)
				return new SqlType[] {new StringClobSqlType()};
			else
				return new SqlType[] {new BinaryBlobSqlType()};
		}

		public override bool Equals(object obj)
		{
			if (obj == this) return true;
			if (!base.Equals(obj)) return false;
			var ct = obj as ClobType;
			if (encoding != ct.encoding && encoding != null && !encoding.Equals(ct.encoding)) return false;
			if (compression == ct.compression) return true;
			return compression != null && compression.Equals(ct.compression);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}