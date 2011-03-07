using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace Lob.NHibernate.Wrappers
{
	public class ExternalBlobDbConnectionWrapper : DbConnection, IExternalBlobConnection
	{
		readonly IExternalBlobConnection _externalConnection;
		internal IDbConnection _db;

		public ExternalBlobDbConnectionWrapper(IDbConnection db, IExternalBlobConnection externalConnection)
		{
			_db = db;
			_externalConnection = externalConnection;
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
			_externalConnection.Dispose();
		}

		int IExternalBlobConnection.BlobIdentifierLength
		{
			get { return _externalConnection.BlobIdentifierLength; }
		}

		public bool DisassembleRequiresExternalBlob
		{
			get { return _externalConnection.DisassembleRequiresExternalBlob; }
		}

		void IExternalBlobConnection.Delete(byte[] fileReference)
		{
			_externalConnection.Delete(fileReference);
		}

		Stream IExternalBlobConnection.OpenReader(byte[] fileReference)
		{
			return _externalConnection.OpenReader(fileReference);
		}

		byte[] IExternalBlobConnection.Store(Stream stream)
		{
			return _externalConnection.Store(stream);
		}

		bool IExternalBlobConnection.Equals(IExternalBlobConnection connection)
		{
			if (connection is ExternalBlobDbConnectionWrapper) connection = (connection as ExternalBlobDbConnectionWrapper)._externalConnection;
			return _externalConnection.Equals(connection);
		}

		public bool SupportsGarbageCollection
		{
			get { return _externalConnection.SupportsGarbageCollection; }
		}

		ExternalBlobWriter IExternalBlobConnection.OpenWriter()
		{
			return _externalConnection.OpenWriter();
		}

		void IExternalBlobConnection.ReadInto(byte[] blobIdentifier, Stream output)
		{
			_externalConnection.ReadInto(blobIdentifier, output);
		}

		void IExternalBlobConnection.GarbageCollect(ICollection<byte[]> livingBlobIdentifiers)
		{
			_externalConnection.GarbageCollect(livingBlobIdentifiers);
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