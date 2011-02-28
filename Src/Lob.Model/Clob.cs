﻿using System.IO;
using System.Text;

namespace Lob.Model
{
	public abstract class Clob
	{
		const int BufferSize = 0x800;

		public static Clob Empty
		{
			get { return new EmptyClob(); }
		}

		public static Clob Create(string filename, Encoding encoding)
		{
			return new FileClob(filename, encoding);
		}

		public static Clob Create(Stream input, Encoding encoding)
		{
			return new TextReaderClob(input, encoding);
		}

		public static Clob Create(TextReader reader)
		{
			return new TextReaderClob(reader);
		}

		public static Clob Create(char[] characters)
		{
			return new StringClob(new string(characters));
		}

		public static Clob Create(string text)
		{
			return new StringClob(text);
		}

		public static implicit operator Clob(TextReader reader)
		{
			return Create(reader);
		}

		public static implicit operator Clob(char[] characters)
		{
			return new StringClob(new string(characters));
		}

		public static implicit operator Clob(string text)
		{
			return new StringClob(text);
		}

		public abstract TextReader OpenReader();

		public virtual void WriteTo(Stream output, Encoding encoding)
		{
			using (var sw = new StreamWriter(output, encoding))
				WriteTo(sw);
		}

		public virtual void WriteTo(TextWriter writer)
		{
			using (TextReader reader = OpenReader())
			{
				var buffer = new char[BufferSize];
				int readChars;
				while ((readChars = reader.Read(buffer, 0, BufferSize)) > 0)
					writer.Write(buffer, 0, readChars);
			}
		}

		public override bool Equals(object obj)
		{
			var c = obj as Clob;
			if (c == null) return false;
			return Equals(c);
		}

		public abstract bool Equals(Clob clob);
	}
}