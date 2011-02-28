using System.IO;

namespace Lob.Model
{
	public class EmptyBlob : Blob
	{
		public override Stream OpenReader()
		{
			return new MemoryStream(new byte[0], false);
		}

		public override void WriteTo(Stream output)
		{
		}

		public override bool Equals(Blob blob)
		{
			var eb = blob as EmptyBlob;
			if (eb != null) return true;
			var ab = blob as ArrayBlob;
			if (ab != null && ab.Data.Length == 0) return true;
			return false;
		}
	}
}