using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NHibernate.Driver;
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
			_base.PrepareCommand(command);
		}

		public bool SupportsMultipleOpenReaders
		{
			get { return _base.SupportsMultipleOpenReaders; }
		}

		public bool SupportsMultipleQueries
		{
			get { return _base.SupportsMultipleQueries; }
		}

		public string MultipleQueriesSeparator
		{
			get { return _base.MultipleQueriesSeparator; }
		}

		public IDbDataParameter GenerateParameter(IDbCommand command, string name, SqlType sqlType)
		{
			return _base.GenerateParameter(command, name, sqlType);
		}
	}
}