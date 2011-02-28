using System;
using System.IO;

namespace Lob.Model
{
	public class StringClob : Clob
	{
		readonly string _text;

		public StringClob(string text)
		{
			if (text == null) throw new ArgumentNullException("text");
			_text = text;
		}

		public string Text
		{
			get { return _text; }
		}

		public override TextReader OpenReader()
		{
			return new StringReader(_text);
		}

		public override void WriteTo(TextWriter writer)
		{
			writer.Write(_text);
		}

		public override bool Equals(Clob clob)
		{
			if (clob == this) return true;
			var sc = clob as StringClob;
			if (sc == null) return false;
			return _text.Equals(sc._text);
		}
	}
}