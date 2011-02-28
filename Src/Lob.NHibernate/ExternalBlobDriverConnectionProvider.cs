using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Lob.NHibernate.Wrappers;
using NHibernate.Connection;
using NHibernate.Driver;

namespace Lob.NHibernate
{
	public class ExternalBlobDriverConnectionProvider : IConnectionProvider
	{
		readonly DriverConnectionProvider _base;
		IExternalBlobConnectionProvider _provider;

		public ExternalBlobDriverConnectionProvider()
		{
			_base = new DriverConnectionProvider();
		}

		public void Configure(IDictionary<string, string> settings)
		{
			System.Type providerType;
			string t;
			if (settings.TryGetValue(Environment.ConnectionProviderProperty, out t) && t != null)
			{
				providerType = System.Type.GetType(t);
				_provider = (IExternalBlobConnectionProvider) Activator.CreateInstance(providerType);
				_provider.Configure(settings);
			}
			else if (settings.TryGetValue(Environment.ConnectionStringNameProperty, out t) && t != null)
			{
				ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[t];
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
			_base.Configure(settings);
		}

		public IDbConnection GetConnection()
		{
			if (_provider == null) return _base.GetConnection();

			return new ExternalBlobDbConnectionWrapper(_base.GetConnection(), _provider.GetConnection());
		}

		public void CloseConnection(IDbConnection conn)
		{
			_base.CloseConnection(conn);
		}

		public IDriver Driver
		{
			get { return new ExternalBlobDriverWrapper(_base.Driver); }
		}

		public void Dispose()
		{
			_base.Dispose();
		}
	}
}