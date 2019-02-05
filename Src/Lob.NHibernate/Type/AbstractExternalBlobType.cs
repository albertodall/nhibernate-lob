using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lob.Model;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Lob.NHibernate.Type
{
    public abstract class AbstractExternalBlobType : AbstractType
    {
        protected static readonly INHibernateLogger Logger = NHibernateLogger.For(typeof(AbstractExternalBlobType));

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

        public override object DeepCopy(object value, ISessionFactoryImplementor factory)
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

        public override Task<object> ReplaceAsync(object original, object current, ISessionImplementor session, object owner, IDictionary copiedAlready, CancellationToken cancellationToken)
        {
            return Task.FromResult(original);
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            if (settable[0]) NullSafeSet(cmd, value, index, session);
        }
               
        public override Task NullSafeSetAsync(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session, CancellationToken cancellationToken)
        {
            if (settable[0]) return NullSafeSetAsync(cmd, value, index, session, cancellationToken);
            return Task.CompletedTask;
        }
        
        public override void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
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

        public override Task NullSafeSetAsync(DbCommand cmd, object value, int index, ISessionImplementor session, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            try
            {
                NullSafeSet(cmd, value, index, session);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
        
        public override object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            return NullSafeGet(rs, names[0], session, owner);
        }

        public override object NullSafeGet(DbDataReader rs, string name, ISessionImplementor session, object owner)
        {
            int index = rs.GetOrdinal(name);

            if (rs.IsDBNull(index)) return null;

            IExternalBlobConnection conn = GetExternalBlobConnection(session);

            byte[] payload;

            if (conn.BlobIdentifierLength == Int32.MaxValue)
            {
                var length = (int) rs.GetBytes(index, 0L, null, 0, 0);
                payload = new byte[length];
                // The if here makes MySql happy (See https://nhibernate.jira.com/browse/NH-2096 for details)
                if (length > 0)
                {
                    rs.GetBytes(index, 0L, payload, 0, length);
                }
            }
            else
            {
                payload = new byte[conn.BlobIdentifierLength];

                // The if here makes MySql happy (See https://nhibernate.jira.com/browse/NH-2096 for details)
                if (payload.Length > 0)
                {
                    var i = (int) rs.GetBytes(index, 0, payload, 0, payload.Length);

                    if (i != payload.Length)
                    {
                        if (Logger.IsErrorEnabled()) Logger.Error("Unknown payload (identifier) length. Recieved {0} but expected {1} bytes for owner: {2}", i, payload.Length, owner);
                        return null;
                    }
                }
                else
                {
                    if (Logger.IsErrorEnabled()) Logger.Error("Unknown payload (identifier) length. Recieved 0 but expected {0} bytes for owner: {1}", payload.Length, owner);
                    return null;
                }
            }

            return CreateLobInstance(conn, payload);
        }

        public override Task<object> NullSafeGetAsync(DbDataReader rs, string name, ISessionImplementor session, object owner, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<object>(cancellationToken);
            }
            try
            {
                return Task.FromResult(NullSafeGet(rs, name, session, owner));
            }
            catch (Exception e)
            {
                return Task.FromException<object>(e);
            }
        }

        public override Task<object> NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner, CancellationToken cancellationToken)
        {
            return NullSafeGetAsync(rs, names[0], session, owner, cancellationToken);
        }

        public override Task<bool> IsDirtyAsync(object old, object current, bool[] checkable, ISessionImplementor session, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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