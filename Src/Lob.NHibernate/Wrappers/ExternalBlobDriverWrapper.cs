using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Lob.NHibernate.Wrappers
{
	public class ExternalBlobDriverWrapper : IDriver
	{
		readonly IDriver _base;

		public ExternalBlobDriverWrapper(IDriver driver)
		{
			_base = driver;
		}

		public bool SupportsMultipleOpenReaders => _base.SupportsMultipleOpenReaders;

		public bool SupportsMultipleQueries => _base.SupportsMultipleQueries;

		public bool RequiresTimeSpanForTime => _base.RequiresTimeSpanForTime;

		public bool SupportsSystemTransactions => _base.SupportsSystemTransactions;

		public bool SupportsNullEnlistment => _base.SupportsNullEnlistment;

		public bool SupportsEnlistmentWhenAutoEnlistmentIsDisabled => _base.SupportsEnlistmentWhenAutoEnlistmentIsDisabled;

		public bool HasDelayedDistributedTransactionCompletion => _base.HasDelayedDistributedTransactionCompletion;

		public DateTime MinDate => _base.MinDate;

		public IDriver UnderlyingDriver => _base;

		public void Configure(IDictionary<string, string> settings)
		{
			_base.Configure(settings);
		}

		public DbConnection CreateConnection()
		{
			return _base.CreateConnection();
		}

		public DbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
		{
			return new ExternalBlobDbCommandWrapper(null, (DbCommand) _base.GenerateCommand(type, sqlString, parameterTypes));
		}

		public void PrepareCommand(DbCommand command)
		{
			// we must unwrap command before preparing it, as the OracleDriver and OracleClientDriver
			// make use of PrepareCommand and reflection to tweak some settings of the command which are not
			// part of the DbCommand base class (so fails if invoked on our DB Command wrapper)
			_base.PrepareCommand(UnwrapCommand(command));
		}


		public void ExpandQueryParameters(DbCommand cmd, SqlString sqlString, SqlType[] parameterTypes)
		{
			_base.ExpandQueryParameters(cmd, sqlString, parameterTypes);
		}
		public DbParameter GenerateParameter(DbCommand command, string name, SqlType sqlType)
		{
			return _base.GenerateParameter(command, name, sqlType);
		}

        public void AdjustCommand(DbCommand command)
        {
	        _base.AdjustCommand(UnwrapCommand(command));
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            return _base.GetResultSetsCommand(session);
        }

        public void RemoveUnusedCommandParameters(DbCommand command, SqlString sqlString)
        {
	        _base.RemoveUnusedCommandParameters(UnwrapCommand(command), sqlString);
        }
        
        private static DbCommand UnwrapCommand(DbCommand command)
        {
	        return command is ExternalBlobDbCommandWrapper wrapper1
		        ? wrapper1.UnderlyingCommand
		        : command;
        }
    }
}