using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using Lob.Model;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Lob.NHibernate.Type
{
    public abstract class AbstractExternalBlobType : AbstractType
    {
        protected static readonly IInternalLogger Logger = LoggerProvider.LoggerFor(typeof (AbstractExternalBlobType));

        protected int PayloadLength { get; set; }

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
            if (PayloadLength == 0) PayloadLength = c.BlobIdentifierLength;
            return c;
        }

        protected abstract object CreateLobInstance(IExternalBlobConnection connection, byte[] payload);

        protected abstract bool ExtractLobData(object lob, out IExternalBlobConnection connection, out byte[] identifier);

        protected abstract void WriteLobTo(object lob, Stream output);

        public override string ToLoggableString(object value, ISessionFactoryImplementor factory)
        {
            IExternalBlobConnection blobconn;
            byte[] payload;
            if (ExtractLobData(value, out blobconn, out payload))
            {
                var sb = new StringBuilder();
                foreach (byte b in payload)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            return null;
        }

        public override object Assemble(object cached, ISessionImplementor session, object owner)
        {
            var payload = cached as byte[];
            if (payload == null) return null;
            IExternalBlobConnection conn = GetExternalBlobConnection(session);
            return CreateLobInstance(conn, payload);
        }

        public override object Disassemble(object value, ISessionImplementor session, object owner)
        {
            if (value == null) return null;

            IExternalBlobConnection blobconn;
            byte[] payload;
            bool hasExternalLob = ExtractLobData(value, out blobconn, out payload);

            IExternalBlobConnection conn = GetExternalBlobConnection(session);

            if (conn.DisassembleRequiresExternalBlob && !hasExternalLob)
            {
                throw new Exception("Unable to cache an unsaved lob.");
            }

            return payload;
        }

        public override object DeepCopy(object value, EntityMode entityMode, ISessionFactoryImplementor factory)
        {
            IExternalBlobConnection blobconn;
            byte[] payload;
            if (ExtractLobData(value, out blobconn, out payload))
                return CreateLobInstance(blobconn, payload);
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
                byte[] payload;
                if (!ExtractLobData(value, out blobconn, out payload) || !conn.Equals(blobconn)) // Skip writing if an equal connection is used
                    using (ExternalBlobWriter writer = conn.OpenWriter())
                    {
                        WriteLobTo(value, writer);
                        payload = writer.Commit();

                        var persistedLob = ((IPersistedLob) value);

                        if (!persistedLob.IsPersisted)
                        {
                            ((IPersistedLob) value).SetPersistedIdentifier(payload, conn);
                        }
                    }
                ((IDataParameter) cmd.Parameters[index]).Value = payload;
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

            byte[] payload;

            if (conn.BlobIdentifierLength == Int32.MaxValue)
            {
                var length = (int) rs.GetBytes(index, 0L, null, 0, 0);
                payload = new byte[length];
                rs.GetBytes(index, 0L, payload, 0, length);
            }
            else
            {
                payload = new byte[conn.BlobIdentifierLength];

                var i = (int) rs.GetBytes(index, 0, payload, 0, payload.Length);

                if (i != payload.Length)
                {
                    if (Logger.IsErrorEnabled) Logger.ErrorFormat("Unknown payload (identifier) length. Recieved {0} but expected {1} bytes for owner: {2}", i, payload.Length, owner);
                    return null;
                }
            }

            return CreateLobInstance(conn, payload);
        }

        public override SqlType[] SqlTypes(IMapping mapping)
        {
            if (PayloadLength == 0 || PayloadLength > 8000)
            {
                return new SqlType[] {new BinaryBlobSqlType()};
            }

            return new[] {new SqlType(DbType.Binary, PayloadLength == 0 ? 32 : PayloadLength)};
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