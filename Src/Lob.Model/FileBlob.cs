using System;
using System.IO;

namespace Lob.Model
{
	public class FileBlob : Blob
	{
		readonly string _filename;

		public FileBlob(string filename)
		{
			if (filename == null) throw new ArgumentNullException("filename");
			_filename = Path.GetFullPath(filename);
		}

		public FileBlob(FileInfo file)
		{
			if (file == null) throw new ArgumentNullException("file");
			_filename = file.FullName;
		}

		public string Filename
		{
			get { return _filename; }
		}

		public override Stream OpenReader()
		{
			return File.Open(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public override bool Equals(Blob blob)
		{
			var fb = blob as FileBlob;
			if (fb != null) return fb._filename.Equals(_filename);
			if (Equals(fb, this)) return true;
			var sb = blob as StreamBlob;
			if (sb == null) return false;
			var fs = sb.UnderlyingStream as FileStream;
			if (fs == null) return false;
			try
			{
				return _filename.Equals(fs.Name);
			}
			catch
			{
				return false;
			}
		}
	}
}