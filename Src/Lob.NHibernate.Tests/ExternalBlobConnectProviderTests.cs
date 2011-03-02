using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lob.NHibernate.Wrappers;
using Xunit;

namespace Lob.NHibernate.Tests
{
	public class ExternalBlobConnectProviderTests
	{
		[Fact]
		public void Configure_with_alternative_underlying_provider_correctly_instantiates_driver_provider_and_uses_it_for_creating_drivers()
		{
			ExternalBlobDriverConnectionProvider provider = new ExternalBlobDriverConnectionProvider();

			provider.Configure(new Dictionary<string, string>
			                   	{
														{Environment.ConnectionUnderlyingDriverConnectionProvider, typeof(StubDriverConnectionProvider).AssemblyQualifiedName}														
			                   	});

			// we cast the wrapped driver, then fetch the underlying driver from there - if the StubdriverConnectionProvider has replaced
			// the default implementation of the underlying driver, then the type of this inner driver should be "StubDriver".

			var driver = ((ExternalBlobDriverWrapper)provider.Driver).UnderlyingDriver;

			Assert.IsType(typeof(StubDriver), driver);
		}
	}
}
