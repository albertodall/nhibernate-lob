using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Lob.NHibernate.Tests
{
	public class StubDriver : IDriver
	{
		public bool SupportsMultipleOpenReaders => throw new NotImplementedException();

		public bool SupportsMultipleQueries => throw new NotImplementedException();

		public bool RequiresTimeSpanForTime => throw new NotImplementedException();

		public bool SupportsSystemTransactions => throw new NotImplementedException();

		public bool SupportsNullEnlistment => throw new NotImplementedException();

		public bool SupportsEnlistmentWhenAutoEnlistmentIsDisabled => throw new NotImplementedException();

		public bool HasDelayedDistributedTransactionCompletion => throw new NotImplementedException();

		public DateTime MinDate => throw new NotImplementedException();

		public void Configure(IDictionary<string, string> settings)
		{
			throw new NotImplementedException();
		}

		public DbConnection CreateConnection()
		{
			throw new NotImplementedException();
		}

		public DbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
		{
			throw new NotImplementedException();
		}

		public void PrepareCommand(DbCommand command)
		{
			throw new NotImplementedException();
		}

		public DbParameter GenerateParameter(DbCommand command, string name, SqlType sqlType)
		{
			throw new NotImplementedException();
		}

		public void AdjustCommand(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public void ExpandQueryParameters(DbCommand cmd, SqlString sqlString, SqlType[] parameterTypes)
        {
	        throw new NotImplementedException();
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            throw new NotImplementedException();
        }

        public void RemoveUnusedCommandParameters(DbCommand cmd, SqlString sqlString)
        {
            throw new NotImplementedException();
        }
    }
}