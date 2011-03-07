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

		public override string ConnectionString
		{
			get { return ((AbstractExternalBlobConnectionProvider) _from).ConnectionString ?? ((AbstractExternalBlobConnectionProvider) _to).ConnectionString; }
			set
			{
				((AbstractExternalBlobConnectionProvider) _from).ConnectionString = value;
				((AbstractExternalBlobConnectionProvider) _to).ConnectionString = value;
			}
		}

		public override IExternalBlobConnection GetConnection()
		{
			IExternalBlobConnection connectionFrom = _from.GetConnection();
			IExternalBlobConnection connectionTo = _to.GetConnection();
			return new MigrationConnection(connectionFrom, connectionTo);
		}
	}
}