using System;
using System.Collections.Generic;
using System.Data;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Lob.NHibernate.Tests
{
	public class StubDriver : IDriver
	{
		public void Configure(IDictionary<string, string> settings)
		{
			throw new NotImplementedException();
		}

		public IDbConnection CreateConnection()
		{
			throw new NotImplementedException();
		}

		public IDbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
		{
			throw new NotImplementedException();
		}

		public void PrepareCommand(IDbCommand command)
		{
			throw new NotImplementedException();
		}

		public IDbDataParameter GenerateParameter(IDbCommand command, string name, SqlType sqlType)
		{
			throw new NotImplementedException();
		}

		public void ExpandQueryParameters(IDbCommand cmd, SqlString sqlString)
		{
			throw new NotImplementedException();
		}

		public bool SupportsMultipleOpenReaders
		{
			get { throw new NotImplementedException(); }
		}

		public bool SupportsMultipleQueries
		{
			get { throw new NotImplementedException(); }
		}
        
        public void AdjustCommand(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            throw new NotImplementedException();
        }

        public void RemoveUnusedCommandParameters(IDbCommand cmd, SqlString sqlString)
        {
            throw new NotImplementedException();
        }
    }
}