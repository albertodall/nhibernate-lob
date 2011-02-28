using System.Collections.Generic;
using System.Configuration;
using NHibernate;

namespace Lob.NHibernate
{
	public abstract class AbstractExternalBlobConnectionProvider : IExternalBlobConnectionProvider
	{
		public virtual string ConnectionString { get; set; }

		public abstract IExternalBlobConnection GetConnection();

		void IExternalBlobConnectionProvider.Configure(IDictionary<string, string> settings)
		{
			string connStr;
			if (settings.TryGetValue(Environment.ConnectionStringNameProperty, out connStr))
			{
				ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connStr];
				if (connectionStringSettings == null)
					throw new HibernateException(string.Format("Could not find named connection string {0}", connStr));
				ConnectionString = connectionStringSettings.ConnectionString;
			}
			else if (settings.TryGetValue(Environment.ConnectionStringProperty, out connStr))
			{
				ConnectionString = connStr;
			}
		}
	}
}