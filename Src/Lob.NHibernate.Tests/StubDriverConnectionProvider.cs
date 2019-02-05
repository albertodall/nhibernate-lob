using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Connection;
using NHibernate.Driver;

namespace Lob.NHibernate.Tests
{
	public class StubDriverConnectionProvider : IConnectionProvider
	{
		public void Dispose()
		{
		}

		public void Configure(IDictionary<string, string> settings)
		{
		}

		public void CloseConnection(DbConnection conn)
		{
		}

		public DbConnection GetConnection()
		{
			throw new NotImplementedException();
		}
	
		public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public IDriver Driver => new StubDriver();
	}
}