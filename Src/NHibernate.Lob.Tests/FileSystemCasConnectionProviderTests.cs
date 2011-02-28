using System;
using System.IO;
using Calyptus.Lob;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Lob.External;
using NHibernate.Tool.hbm2ddl;
using Xunit;

namespace NHibernate.Lob.Tests
{
	public class FileSystemCasConnectionProviderTests : IUseFixture<TempFolder>
	{
		string _folder;

		public void SetFixture(TempFolder data)
		{
			_folder = data.Folder;
		}

		[Fact]
		public void Write_file_to_filesysem_and_read_it_back()
		{
			SqlServerUtility.RemoveAllTablesFromDefaultDatabase(TestDatabases.SqlServerLobTests);

			var configuration = new Configuration();
			configuration.SetProperty("connection.provider", typeof (DriverConnectionProvider).AssemblyQualifiedName);
			configuration.SetProperty("dialect", typeof (MsSql2008Dialect).AssemblyQualifiedName);
			configuration.SetProperty("connection.driver_class", typeof (SqlClientDriver).AssemblyQualifiedName);
			configuration.SetProperty("connection.connection_string", TestDatabases.SqlServerLobTests);
			configuration.SetProperty("proxyfactory.factory_class", typeof (ProxyFactoryFactory).AssemblyQualifiedName);
			configuration.SetProperty("connection.lob.external.provider", typeof (FileSystemCasConnectionProvider).AssemblyQualifiedName);
			configuration.SetProperty("connection.lob.external.connection_string", string.Format("Path={0};Hash=SHA256", _folder));
			configuration.SetProperty("show_sql", "true");

			configuration.AddAssembly(typeof (Image).Assembly);

			new SchemaExport(configuration).Execute(false, true, false);

			ISessionFactory sessionFactory = configuration.BuildSessionFactory();

			var image = new Image
			            	{
			            		FileName = "test.txt",
			            		ContentType = "text/plain",
			            		Size = 10,
			            		Contents = Blob.Create(new byte[] {1, 2, 3, 4}),
			            		Title = "test"
			            	};

			Guid imageId;

			using (ISession session = sessionFactory.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					session.Save(image);
					tx.Commit();
					imageId = image.Id;
				}
			}

			using (ISession session = sessionFactory.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					var imageFromDb = session.Get<Image>(imageId);

					using (Stream streamReader = imageFromDb.Contents.OpenReader())
					{
						var buffer = new byte[1024];
						int length = streamReader.Read(buffer, 0, buffer.Length);
						Assert.Equal(4, length);
						Assert.Equal(1, buffer[0]);
						Assert.Equal(2, buffer[1]);
						Assert.Equal(3, buffer[2]);
						Assert.Equal(4, buffer[3]);
					}
				}
			}
		}
	}
}