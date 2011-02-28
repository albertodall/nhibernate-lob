using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Lob.NHibernate.Type
{
	public abstract class AbstractExternalBlobType : AbstractType
	{
		int _identifierLength;

		public override bool IsMutable
		{
			get { return true; }
		}

		public override string Name
		{
			get { return "ExternalBlobIdentifier"; }
		}

		protected IExternalBlobConnection GetExternalBlobConnection(ISessionImplementor session)
		{
			if (session.Connection == null) throw new NullReferenceException("CasBlobType requires an open connection.");
			var c = session.Connection as IExternalBlobConnection;
			if (c == null)
				throw new Exception(
					"ExternalBlobTypes requires a IExternalBlobConnection. Make sure you use NHibernate.Lob.External.DriverConnectionProvider and specify an IExternalBlobConnectionProvider in your NHibernate configuration.");
			if (_identifierLength == 0) _identifierLength = c.BlobIdentifierLength;
			return c;
		}

		protected abstract object CreateLobInstance(IExternalBlobConnection connection, byte[] identifier);

		protected abstract bool ExtractLobData(object lob, out IExternalBlobConnection connection, out byte[] identifier);

		protected abstract void WriteLobTo(object lob, Stream output);

		public override string ToLoggableString(object value, ISessionFactoryImplementor factory)
		{
			IExternalBlobConnection blobconn;
			byte[] identifier;
			if (ExtractLobData(value, out blobconn, out identifier))
			{
				var sb = new StringBuilder();
				foreach (byte b in identifier)
					sb.Append(b.ToString("x2"));
				return sb.ToString();
			}
			return null;
		}

		public override object Assemble(object cached, ISessionImplementor session, object owner)
		{
			var identifier = cached as byte[];
			if (identifier == null) return null;
			IExternalBlobConnection conn = GetExternalBlobConnection(session);
			return CreateLobInstance(conn, identifier);
		}

		public override object Disassemble(object value, ISessionImplementor session, object owner)
		{
			if (value == null) return null;
			IExternalBlobConnection blobconn;
			byte[] identifier;
			if (ExtractLobData(value, out blobconn, out identifier))
			{
				IExternalBlobConnection conn = GetExternalBlobConnection(session);
				if (conn.Equals(blobconn))
					return identifier;
			}
			throw new Exception("Unable to cache an unsaved lob.");
		}

		public override object DeepCopy(object value, EntityMode entityMode, ISessionFactoryImplementor factory)
		{
			IExternalBlobConnection blobconn;
			byte[] identifier;
			if (ExtractLobData(value, out blobconn, out identifier))
				return CreateLobInstance(blobconn, identifier);
			return value;
		}

		public override object Replace(object original, object target, ISessionImplementor session, object owner, IDictionary copiedAlready)
		{
			return original;
		}

		public override void NullSafeSet(IDbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
		{
			if (settable[0]) NullSafeSet(cmd, value, index, session);
		}

		public override void NullSafeSet(IDbCommand cmd, object value, int index, ISessionImplementor session)
		{
			if (value == null)
			{
				((IDataParameter) cmd.Parameters[index]).Value = DBNull.Value;
			}
			else
			{
				IExternalBlobConnection conn = GetExternalBlobConnection(session);
				IExternalBlobConnection blobconn;
				byte[] identifier;
				if (!ExtractLobData(value, out blobconn, out identifier) || !conn.Equals(blobconn)) // Skip writing if an equal connection is used
					using (ExternalBlobWriter writer = conn.OpenWriter())
					{
						WriteLobTo(value, writer);
						identifier = writer.Commit();
					}
				((IDataParameter) cmd.Parameters[index]).Value = identifier;
			}
		}

		public override object NullSafeGet(IDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			return NullSafeGet(rs, names[0], session, owner);
		}

		public override object NullSafeGet(IDataReader rs, string name, ISessionImplementor session, object owner)
		{
			int index = rs.GetOrdinal(name);

			if (rs.IsDBNull(index)) return null;

			IExternalBlobConnection conn = GetExternalBlobConnection(session);

			byte[] identifier;

			if (conn.BlobIdentifierLength == Int32.MaxValue)
			{
				var length = (int) rs.GetBytes(index, 0L, null, 0, 0);
				identifier = new byte[length];
				rs.GetBytes(index, 0L, identifier, 0, length);
			}
			else
			{
				identifier = new byte[conn.BlobIdentifierLength];
				var i = (int) rs.GetBytes(index, 0, identifier, 0, identifier.Length);
				if (i != identifier.Length) throw new Exception("Unknown identifier length. Recieved " + i + " but expected " + identifier.Length + " bytes");
			}

			return CreateLobInstance(conn, identifier);
		}

		public override SqlType[] SqlTypes(IMapping mapping)
		{
			return new[] {new SqlType(DbType.Binary, _identifierLength == 0 ? 32 : _identifierLength)};
		}

		public override int GetColumnSpan(IMapping session)
		{
			return 1;
		}

		public override bool IsDirty(object old, object current, bool[] checkable, ISessionImplementor session)
		{
			return checkable[0] && IsDirty(old, current, session);
		}

		public override bool[] ToColumnNullness(object value, IMapping mapping)
		{
			return value == null ? new[] {false} : new[] {true};
		}

		public override object FromXMLNode(XmlNode xml, IMapping factory)
		{
			return null;
		}

		public override void SetToXMLNode(XmlNode xml, object value, ISessionFactoryImplementor factory)
		{
			xml.Value = null;
		}

		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			if (obj == null) return false;
			return GetType() == obj.GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}
}