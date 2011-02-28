using System;
using System.IO;
using System.Xml;

namespace Lob.Model
{
	public class FileXlob : Xlob
	{
		readonly string _filename;
		readonly XmlReaderSettings _settings;

		public FileXlob(string filename)
		{
			if (filename == null) throw new ArgumentNullException("filename");
			_filename = Path.GetFullPath(filename);
		}

		public FileXlob(FileInfo file)
		{
			if (file == null) throw new ArgumentNullException("file");
			_filename = file.FullName;
		}

		public FileXlob(string filename, XmlReaderSettings readerSettings) : this(filename)
		{
			_settings = readerSettings;
		}

		public FileXlob(FileInfo file, XmlReaderSettings readerSettings) : this(file)
		{
			_settings = readerSettings;
		}

		public string Filename
		{
			get { return _filename; }
		}

		public XmlReaderSettings ReaderSettings
		{
			get { return _settings; }
		}

		public override XmlReader OpenReader()
		{
			return XmlReader.Create(_filename, ReaderSettings);
		}

		public override bool Equals(Xlob xlob)
		{
			var fx = xlob as FileXlob;
			if (fx == null) return false;
			if (fx == this) return true;
			return fx._filename.Equals(_filename) && (fx._settings == null || _settings == null || fx._settings.Equals(_settings));
		}
	}
}