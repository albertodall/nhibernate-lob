using System.IO;
using Lob.Model;
using Xunit;

namespace Lob.NHibernate.Tests
{
	public class LobExtensionsTests
	{
		[Fact]
		public void Get_length_for_blob_gets_length_of_underlying_stream_without_breaking_stream()
		{
			var stream = new MemoryStream(new byte[] {1, 2, 3, 4});
			var blob = new StreamBlob(stream);
			Assert.Equal(4, blob.GetLength());
			using (Stream reader = blob.OpenReader())
			{
				Assert.Equal(1, reader.ReadByte());
				Assert.Equal(2, reader.ReadByte());
				Assert.Equal(3, reader.ReadByte());
				Assert.Equal(4, reader.ReadByte());
			}
		}
	}
}