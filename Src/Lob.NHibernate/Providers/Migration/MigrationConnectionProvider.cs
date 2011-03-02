using System;

namespace Lob.NHibernate.Providers.Migration
{
	public class MigrationConnectionProvider : AbstractExternalBlobConnectionProvider
	{
		readonly IExternalBlobConnectionProvider _from;
		readonly IExternalBlobConnectionProvider _to;

		public MigrationConnectionProvider(IExternalBlobConnectionProvider from, IExternalBlobConnectionProvider to)
		{
			_from = from;
			_to = to;
		}

		public override IExternalBlobConnection GetConnection()
		{
			throw new NotImplementedException();
		}
	}
}