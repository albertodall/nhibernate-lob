using System;
using System.IO;
using System.Linq;

namespace Lob.Model
{
	public class ArrayBlob : Blob
	{
		readonly byte[] _data;

		public ArrayBlob(byte[] data)
		{
			if (data == null) throw new ArgumentNullException("data");
			_data = data;
		}

		public byte[] Data
		{
			get { return _data; }
		}

		public override Stream OpenReader()
		{
			return new MemoryStream(_data, false);
		}

		public override void WriteTo(Stream output)
		{
			output.Write(_data, 0, _data.Length);
		}

		public override bool Equals(Blob blob)
		{
			var ab = blob as ArrayBlob;
			if (ab == null) return false;
			if (this == ab) return true;

			byte[] a = _data, b = ab._data;

			if (a.Length != b.Length) return false;

			return !a.Where((t, i) => t != b[i]).Any();
		}
	}
}