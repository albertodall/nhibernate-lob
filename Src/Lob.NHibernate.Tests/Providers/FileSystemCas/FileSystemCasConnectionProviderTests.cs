using System;
using System.IO;
using Lob.Model;
using Lob.NHibernate.GarbageCollection;
using Lob.NHibernate.Providers.FileSystemCas;
using NHibernate;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Lob.Tests;
using NHibernate.Tool.hbm2ddl;
using Xunit;

namespace Lob.NHibernate.Tests.Providers.FileSystemCas
{
	public class FileSystemCasConnectionProviderTests : IUseFixture<TempFolder>
	{
		string _folder;

		public void SetFixture(TempFolder data)
		{
			SqlServerUtility.RemoveAllTablesFromDefaultDatabase(TestDatabases.SqlServerLobTests);

			_folder = data.Folder;
		}

		[Fact]
		public void Write_file_filesytem_with_second_level_cache_enabled_and_read_it_back()
		{
			Configuration configuration = CreateDefaultConfigurationWithSecondLevelCache();

			CreateDatabaseStructure(configuration);

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

		[Fact]
		public void Write_file_to_filesysem_and_read_it_back()
		{
			Configuration configuration = CreateDefaultConfiguration();

			CreateDatabaseStructure(configuration);

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

		[Fact]
		public void Garbage_collection_removes_unused_contents()
		{
			Configuration configuration = CreateDefaultConfiguration();

			CreateDatabaseStructure(configuration);

			ISessionFactory sessionFactory = configuration.BuildSessionFactory();

			var image1 = new Image
			             	{
			             		FileName = "test.txt",
			             		ContentType = "text/plain",
			             		Size = 10,
			             		Contents = Blob.Create(new byte[] {1, 2, 3, 4}),
			             		Title = "test"
			             	};

			var image2 = new Image
			             	{
			             		FileName = "test2.txt",
			             		ContentType = "text/plain",
			             		Size = 10,
			             		Contents = Blob.Create(new byte[] {4, 3, 2, 1}),
			             		Title = "test2"
			             	};

			// create image

			using (ISession session = sessionFactory.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					session.Save(image1);
					session.Save(image2);
					tx.Commit();
				}
			}

			// should have 1 file
			Assert.Equal(2, Directory.GetFiles(_folder, "*", SearchOption.AllDirectories).Length);

			using (ISession session = sessionFactory.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					session.Delete(image1);
					tx.Commit();
				}
			}

			var collector = new ExternalBlobGarbageCollector();

			using (ISession session = sessionFactory.OpenSession())
			{
				collector.Collect(session);
			}

			// should now have no files
			Assert.Equal(1, Directory.GetFiles(_folder, "*", SearchOption.AllDirectories).Length);
		}

		void CreateDatabaseStructure(Configuration configuration)
		{
			new SchemaExport(configuration).Execute(false, true, false);
		}

		Configuration CreateDefaultConfigurationWithSecondLevelCache()
		{
			Configuration config = CreateDefaultConfiguration();
            config.SetProperty("cache.provider_class", typeof(HashtableCacheProvider).FullName);
			config.SetProperty("cache.use_query_cache", "true");
			config.SetProperty("cache.use_second_level_cache", "true");
			return config;
		}

		Configuration CreateDefaultConfiguration()
		{
			var configuration = new Configuration();
			configuration.SetProperty("connection.provider", typeof (ExternalBlobDriverConnectionProvider).AssemblyQualifiedName);
			configuration.SetProperty("dialect", typeof (MsSql2008Dialect).AssemblyQualifiedName);
			configuration.SetProperty("connection.driver_class", typeof (SqlClientDriver).AssemblyQualifiedName);
			configuration.SetProperty("connection.connection_string", TestDatabases.SqlServerLobTests);
			configuration.SetProperty("connection.lob.external.provider", typeof (FileSystemCasConnectionProvider).AssemblyQualifiedName);
			configuration.SetProperty("connection.lob.external.connection_string", string.Format("Path={0};Hash=SHA256", _folder));
			configuration.SetProperty("show_sql", "true");
			configuration.AddAssembly(typeof (Image).Assembly);
			return configuration;
		}
	}
}