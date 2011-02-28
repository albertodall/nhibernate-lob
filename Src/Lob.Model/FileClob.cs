using System;
using System.IO;
using System.Text;

namespace Lob.Model
{
	public class FileClob : Clob
	{
		readonly Encoding _encoding;
		readonly string _filename;

		public FileClob(string filename)
		{
			if (filename == null) throw new ArgumentNullException("filename");
			_filename = Path.GetFullPath(filename);
		}

		public FileClob(string filename, Encoding encoding) : this(filename)
		{
			_encoding = encoding;
		}

		public FileClob(FileInfo file)
		{
			if (file == null) throw new ArgumentNullException("file");
			_filename = file.FullName;
		}

		public FileClob(FileInfo file, Encoding encoding) : this(file)
		{
			_encoding = encoding;
		}

		public string Filename
		{
			get { return _filename; }
		}

		public Encoding Encoding
		{
			get { return _encoding; }
		}

		public override TextReader OpenReader()
		{
			return _encoding == null ? new StreamReader(_filename, true) : new StreamReader(_filename, _encoding);
		}

		public override bool Equals(Clob clob)
		{
			var fc = clob as FileClob;
			if (fc == null) return false;
			if (fc == this) return true;
			return fc._filename.Equals(_filename) && (fc._encoding == null || _encoding == null || fc._encoding == _encoding);
		}
	}
}