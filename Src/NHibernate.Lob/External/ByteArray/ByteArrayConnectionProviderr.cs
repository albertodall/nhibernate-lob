namespace NHibernate.Lob.External.ByteArray
{
	public class ByteArrayConnectionProviderr : AbstractExternalBlobConnectionProvider
	{
		public override IExternalBlobConnection GetConnection()
		{
			return new ByteArrayConnection();
		}
	}
}