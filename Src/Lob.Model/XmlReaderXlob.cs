using System;
using System.Xml;

namespace Lob.Model
{
	public class XmlReaderXlob : Xlob
	{
		XmlReader _reader;

		public XmlReaderXlob(XmlReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			_reader = reader;
		}

		public override XmlReader OpenReader()
		{
			lock (this)
			{
				if (_reader == null) throw new Exception("The XmlReader has already been opened once and cannot be reset.");
				XmlReader r = _reader;
				_reader = null;
				return r;
			}
		}

		public override void WriteTo(XmlWriter writer)
		{
			XmlReader r;
			lock (this)
			{
				if (_reader == null) throw new Exception("The XmlReader has already been opened once and cannot be reset.");
				r = _reader;
				_reader = null;
			}
			if (r.ReadState == ReadState.Initial)
				r.Read();
			while (!r.EOF)
				writer.WriteNode(r, true);
		}

		public override bool Equals(Xlob xlob)
		{
			var xr = xlob as XmlReaderXlob;
			if (xr == null) return false;
			if (xr == this) return true;
			return _reader != null && _reader == xr._reader;
		}
	}
}