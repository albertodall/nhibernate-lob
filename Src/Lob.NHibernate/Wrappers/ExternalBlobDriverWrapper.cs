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

		public IDriver UnderlyingDriver
		{
			get { return _base; }
		}

		public void Configure(IDictionary<string, string> settings)
		{
			_base.Configure(settings);
		}

		public IDbConnection CreateConnection()
		{
			return _base.CreateConnection();
		}

		public IDbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
		{
			return new ExternalBlobDbCommandWrapper(null, (DbCommand) _base.GenerateCommand(type, sqlString, parameterTypes));
		}

		public void PrepareCommand(IDbCommand command)
		{
			// we must unwrap command before preparing it, as the OracleDriver and OracleClientDriver
			// make use of PrepareCommand and reflection to tweak some settings of the command which are not
			// part of the DbCommand base class (so fails if invoked on our DB Command wrapper)

			var wrapper = command as ExternalBlobDbCommandWrapper;

			var unwrappedCommand = (wrapper != null) ? wrapper.UnderlyingCommand : command;

			_base.PrepareCommand(unwrappedCommand);
		}

		public void ExpandQueryParameters(IDbCommand cmd, SqlString sqlString)
		{
			_base.ExpandQueryParameters(cmd, sqlString);
		}

		public bool SupportsMultipleOpenReaders
		{
			get { return _base.SupportsMultipleOpenReaders; }
		}

		public bool SupportsMultipleQueries
		{
			get { return _base.SupportsMultipleQueries; }
		}

		public IDbDataParameter GenerateParameter(IDbCommand command, string name, SqlType sqlType)
		{
			return _base.GenerateParameter(command, name, sqlType);
		}

        public void AdjustCommand(IDbCommand command)
        {
            var wrapper = command as ExternalBlobDbCommandWrapper;

            var unwrappedCommand = (wrapper != null) ? wrapper.UnderlyingCommand : command;

            _base.AdjustCommand(unwrappedCommand);
        }

        public IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            return _base.GetResultSetsCommand(session);
        }

        public void RemoveUnusedCommandParameters(IDbCommand command, SqlString sqlString)
        {
            var wrapper = command as ExternalBlobDbCommandWrapper;

            var unwrappedCommand = (wrapper != null) ? wrapper.UnderlyingCommand : command;

            _base.RemoveUnusedCommandParameters(unwrappedCommand, sqlString);
        }
    }
}