using System.Data;
using System.Data.Common;

namespace Lob.NHibernate.Wrappers
{
	public class ExternalBlobDbCommandWrapper : DbCommand
	{
		readonly DbCommand _cmd;
		DbConnection _conn;

		public ExternalBlobDbCommandWrapper(DbConnection conn, DbCommand cmd)
		{
			_cmd = cmd;
			_conn = conn;
		}

		public override string CommandText
		{
			get { return _cmd.CommandText; }
			set { _cmd.CommandText = value; }
		}

		public override int CommandTimeout
		{
			get { return _cmd.CommandTimeout; }
			set { _cmd.CommandTimeout = value; }
		}

		public override CommandType CommandType
		{
			get { return _cmd.CommandType; }
			set { _cmd.CommandType = value; }
		}

		protected override DbConnection DbConnection
		{
			get { return _conn; }
			set
			{
				_conn = value;
				_cmd.Connection = ((ExternalBlobDbConnectionWrapper) value)._db as DbConnection;
			}
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get { return _cmd.Parameters; }
		}

		protected override DbTransaction DbTransaction
		{
			get { return _cmd.Transaction; }
			set { _cmd.Transaction = value; }
		}

		public override bool DesignTimeVisible
		{
			get { return false; }
			set { }
		}

		public override UpdateRowSource UpdatedRowSource
		{
			get { return _cmd.UpdatedRowSource; }
			set { _cmd.UpdatedRowSource = value; }
		}

		public override void Cancel()
		{
			_cmd.Cancel();
		}

		protected override DbParameter CreateDbParameter()
		{
			return _cmd.CreateParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return _cmd.ExecuteReader(behavior);
		}

		public override int ExecuteNonQuery()
		{
			return _cmd.ExecuteNonQuery();
		}

		public override object ExecuteScalar()
		{
			return _cmd.ExecuteScalar();
		}

		public override void Prepare()
		{
			_cmd.Prepare();
		}
	}
}