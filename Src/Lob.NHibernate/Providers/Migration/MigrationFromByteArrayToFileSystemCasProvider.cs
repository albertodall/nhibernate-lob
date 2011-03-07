using Lob.NHibernate.Providers.ByteArray;
using Lob.NHibernate.Providers.FileSystemCas;

namespace Lob.NHibernate.Providers.Migration
{
	public class MigrationFromByteArrayToFileSystemCasProvider : MigrationConnectionProvider
	{
		public MigrationFromByteArrayToFileSystemCasProvider() : base(new ByteArrayConnectionProvider(), new FileSystemCasConnectionProvider())
		{
		}
	}
}