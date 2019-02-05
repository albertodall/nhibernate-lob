using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Lob.NHibernate.Wrappers;
using NHibernate.Connection;
using NHibernate.Driver;

namespace Lob.NHibernate
{
	public class ExternalBlobDriverConnectionProvider : IConnectionProvider
	{
		IConnectionProvider _base;
		IExternalBlobConnectionProvider _provider;

		public ExternalBlobDriverConnectionProvider()
		{
			_base = new DriverConnectionProvider();
		}

		public IDriver Driver => new ExternalBlobDriverWrapper(_base.Driver);

		public void Configure(IDictionary<string, string> settings)
		{
			CreateBlobProviderFromConfiguration(settings);
			OverrideDefaultConnectProviderBaseIfSpecified(settings);
			_base.Configure(settings);
		}

		public DbConnection GetConnection()
		{
			var connection = _base.GetConnection();
			
			if (_provider == null) return connection;

			return new ExternalBlobDbConnectionWrapper(connection, _provider.GetConnection());
		}

		public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
		{
			var connection = await _base.GetConnectionAsync(cancellationToken);
			if (_provider == null) return connection;

			return new ExternalBlobDbConnectionWrapper(connection, _provider.GetConnection());
		}

		public void CloseConnection(DbConnection conn)
		{
			_base.CloseConnection(conn);
		}

		public void Dispose()
		{
			_base.Dispose();
		}

		void OverrideDefaultConnectProviderBaseIfSpecified(IDictionary<string, string> settings)
		{
			string typeAsString;

			if (settings.TryGetValue(Environment.ConnectionUnderlyingDriverConnectionProvider, out typeAsString))
			{
				System.Type providerType = typeAsString.Contains(",")
				                           	? System.Type.GetType(typeAsString, true)
				                           	: typeof (global::NHibernate.Cfg.Environment).Assembly.GetType(typeAsString, true, true);

				if (providerType == null) throw new Exception("Failed to resolved providerType: " + typeAsString);

				_base = (IConnectionProvider) Activator.CreateInstance(providerType);
			}
		}

		void CreateBlobProviderFromConfiguration(IDictionary<string, string> settings)
		{
			System.Type providerType;
			string typeAsString;
			if (settings.TryGetValue(Environment.ConnectionProviderProperty, out typeAsString) && typeAsString != null)
			{
				providerType = System.Type.GetType(typeAsString);
				_provider = (IExternalBlobConnectionProvider) Activator.CreateInstance(providerType);
				_provider.Configure(settings);
			}
			else if (settings.TryGetValue(Environment.ConnectionStringNameProperty, out typeAsString) && typeAsString != null)
			{
				ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[typeAsString];
				if (connectionStringSettings != null && !string.IsNullOrEmpty(connectionStringSettings.ProviderName))
				{
					providerType = System.Type.GetType(connectionStringSettings.ProviderName);
					if (typeof (IExternalBlobConnectionProvider).IsAssignableFrom(providerType))
					{
						_provider = (IExternalBlobConnectionProvider) Activator.CreateInstance(providerType);
						_provider.Configure(settings);
					}
				}
			}
		}
	}
}