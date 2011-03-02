using System.IO;

namespace Lob.Model
{
	public static class LobExtensions
	{
		public static long GetLength(this Blob blob)
		{
			using (Stream reader = blob.OpenReader())
			{
				return reader.Length;
			}
		}

		public static long GetLength(this Clob clob)
		{
			using (TextReader reader = clob.OpenReader())
			{
				var buffer = new char[1024];

				long count = 0;

				while (true)
				{
					int length = reader.ReadBlock(buffer, 0, buffer.Length);
					if (length > 0) count += length;
					else break;
				}

				return count;
			}
		}
	}
}