using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace Lob.NHibernate.Wrappers
{
	public class ExternalBlobDbConnectionWrapper : DbConnection, IExternalBlobConnection
	{
		readonly IExternalBlobConnection _cas;
		internal IDbConnection _db;

		public ExternalBlobDbConnectionWrapper(IDbConnection db, IExternalBlobConnection cas)
		{
			_db = db;
			_cas = cas;
		}

		public override string ConnectionString
		{
			get { return _db.ConnectionString; }
			set { _db.ConnectionString = value; }
		}

		public override string DataSource
		{
			get { return null; }
		}

		public override string Database
		{
			get { return _db.Database; }
		}

		public override string ServerVersion
		{
			get { return null; }
		}

		public override ConnectionState State
		{
			get { return _db.State; }
		}

		void IDisposable.Dispose()
		{
			_db.Dispose();
			_cas.Dispose();
		}

		int IExternalBlobConnection.BlobIdentifierLength
		{
			get { return _cas.BlobIdentifierLength; }
		}

		public bool DisassembleRequiresExternalBlob
		{
			get { return _cas.DisassembleRequiresExternalBlob; }
		}

		void IExternalBlobConnection.Delete(byte[] fileReference)
		{
			_cas.Delete(fileReference);
		}

		Stream IExternalBlobConnection.OpenReader(byte[] fileReference)
		{
			return _cas.OpenReader(fileReference);
		}

		byte[] IExternalBlobConnection.Store(Stream stream)
		{
			return _cas.Store(stream);
		}

		bool IExternalBlobConnection.Equals(IExternalBlobConnection connection)
		{
			if (connection is ExternalBlobDbConnectionWrapper) connection = (connection as ExternalBlobDbConnectionWrapper)._cas;
			return _cas.Equals(connection);
		}

		ExternalBlobWriter IExternalBlobConnection.OpenWriter()
		{
			return _cas.OpenWriter();
		}

		void IExternalBlobConnection.ReadInto(byte[] blobIdentifier, Stream output)
		{
			_cas.ReadInto(blobIdentifier, output);
		}

		void IExternalBlobConnection.GarbageCollect(IEnumerable<byte[]> livingBlobIdentifiers)
		{
			_cas.GarbageCollect(livingBlobIdentifiers);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return (DbTransaction) _db.BeginTransaction(isolationLevel);
		}

		public override void ChangeDatabase(string databaseName)
		{
			_db.ChangeDatabase(databaseName);
		}

		public override void Close()
		{
			_db.Close();
		}

		protected override DbCommand CreateDbCommand()
		{
			return new ExternalBlobDbCommandWrapper(this, _db.CreateCommand() as DbCommand);
		}

		public override void Open()
		{
			_db.Open();
		}
	}
}