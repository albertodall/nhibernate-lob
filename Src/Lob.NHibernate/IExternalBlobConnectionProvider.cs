using System.Collections.Generic;

namespace Lob.NHibernate
{
	public interface IExternalBlobConnectionProvider
	{
		void Configure(IDictionary<string, string> settings);

		IExternalBlobConnection GetConnection();
	}
}