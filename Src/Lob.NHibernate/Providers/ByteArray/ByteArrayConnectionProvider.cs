namespace Lob.NHibernate.Providers.ByteArray
{
	public class ByteArrayConnectionProvider : AbstractExternalBlobConnectionProvider
	{
		public override IExternalBlobConnection GetConnection()
		{
			return new ByteArrayConnection();
		}
	}
}