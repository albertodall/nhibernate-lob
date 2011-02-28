using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NHibernate.UserTypes;

namespace Lob.NHibernate.Compression
{
	public sealed class XmlTextCompressor : IXmlCompressor, IParameterizedType
	{
		readonly XmlReaderSettings _readerSettings;
		readonly XmlWriterSettings _writerSettings;

		IStreamCompressor _compressor;

		public XmlTextCompressor()
		{
			_readerSettings = new XmlReaderSettings {CloseInput = true};
			_writerSettings = new XmlWriterSettings();
			_readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			_writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			_writerSettings.Encoding = Encoding.UTF8;
			_writerSettings.CloseOutput = true;
		}

		public XmlTextCompressor(IStreamCompressor compressor) : this()
		{
			_compressor = compressor;
		}

		public Encoding Encoding
		{
			get { return _writerSettings.Encoding; }
			set { _writerSettings.Encoding = value; }
		}

		public ConformanceLevel ConformanceLevel
		{
			get { return _readerSettings.ConformanceLevel; }
			set
			{
				_readerSettings.ConformanceLevel = value;
				_writerSettings.ConformanceLevel = value;
			}
		}

		public IStreamCompressor Compressor
		{
			get { return _compressor; }
			set { _compressor = value; }
		}

		public void SetParameterValues(IDictionary<string, string> parameters)
		{
			string conf = parameters["conformance"];
			if (!string.IsNullOrEmpty(conf)) ConformanceLevel = (ConformanceLevel) Enum.Parse(typeof (ConformanceLevel), conf, true);
			string enc = parameters["encoding"];
			if (!string.IsNullOrEmpty(enc)) Encoding = Encoding.GetEncoding(enc);
		}

		public XmlReader GetDecompressor(Stream input)
		{
			var parser = new XmlParserContext(null, null, null, XmlSpace.Preserve) {Encoding = Encoding};
			return XmlReader.Create(_compressor == null ? input : _compressor.GetDecompressor(input), _readerSettings, parser);
		}

		public XmlWriter GetCompressor(Stream output)
		{
			return XmlWriter.Create(_compressor == null ? output : _compressor.GetCompressor(output), _writerSettings);
		}

		public override bool Equals(object obj)
		{
			if (obj == this) return true;
			var tc = obj as XmlTextCompressor;
			if (tc == null) return false;
			if (Encoding != tc.Encoding) return false;
			if (ConformanceLevel != tc.ConformanceLevel) return false;
			return _compressor == tc._compressor || (_compressor != null && _compressor.Equals(tc._compressor));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}