using Lob.NHibernate.Providers.ByteArray;
using Lob.NHibernate.Providers.FileSystemCas;

namespace Lob.NHibernate.Providers.Migration
{
	public class MigrationFromFileSystemCasToByteArrayProvider : MigrationConnectionProvider
	{
		public MigrationFromFileSystemCasToByteArrayProvider()
			: base(new FileSystemCasConnectionProvider(), new ByteArrayConnectionProvider())
		{
		}
	}
}