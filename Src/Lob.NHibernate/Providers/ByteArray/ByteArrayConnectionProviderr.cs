namespace Lob.NHibernate.Providers.ByteArray
{
	public class ByteArrayConnectionProviderr : AbstractExternalBlobConnectionProvider
	{
		public override IExternalBlobConnection GetConnection()
		{
			return new ByteArrayConnection();
		}
	}
}