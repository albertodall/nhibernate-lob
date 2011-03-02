using System;
using System.Collections.Generic;
using System.Data;
using NHibernate.Connection;
using NHibernate.Driver;

namespace Lob.NHibernate.Tests
{
	public class StubDriverConnectionProvider: IConnectionProvider
	{
		public void Dispose()
		{
		}

		public void Configure(IDictionary<string, string> settings)
		{
		}

		public void CloseConnection(IDbConnection conn)
		{
		}

		public IDbConnection GetConnection()
		{
			throw new NotImplementedException();
		}

		public IDriver Driver
		{
			get { return new StubDriver(); }
		}
	}
}