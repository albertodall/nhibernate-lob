using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Lob.Model
{
	public abstract class Xlob : IPersistedLob
	{
		object _externalSource;
		byte[] _identifier;

		public static Xlob Empty
		{
			get { return new EmptyXlob(); }
		}

		bool IPersistedLob.IsPersisted
		{
			get { return GetIsPersisted(); }
		}

		byte[] IPersistedLob.GetPersistedIdentifier()
		{
			return GetPersistedIdentifier();
		}

		object IPersistedLob.GetExternalStore()
		{
			return GetExternalSource();
		}

		void IPersistedLob.SetPersistedIdentifier(byte[] contents, object externalStore)
		{
			SetPersistedIdentifier(contents, externalStore);
		}

		public static Xlob Create(Stream stream)
		{
			return Create(stream, new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment, CloseInput = false}, null);
		}

		public static Xlob Create(TextReader reader)
		{
			return Create(reader, new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment, CloseInput = false}, null);
		}

		public static Xlob Create(Stream stream, XmlReaderSettings settings)
		{
			return Create(stream, settings, null);
		}

		public static Xlob Create(TextReader reader, XmlReaderSettings settings)
		{
			return Create(reader, settings, null);
		}

		public static Xlob Create(Stream stream, XmlParserContext inputContext)
		{
			return Create(stream, new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment, CloseInput = false}, inputContext);
		}

		public static Xlob Create(TextReader reader, XmlParserContext inputContext)
		{
			return Create(reader, new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment, CloseInput = false}, inputContext);
		}

		public static Xlob Create(Stream stream, XmlReaderSettings settings, XmlParserContext inputContext)
		{
			return new XmlReaderXlob(XmlReader.Create(stream, settings, inputContext));
		}

		public static Xlob Create(TextReader reader, XmlReaderSettings settings, XmlParserContext inputContext)
		{
			return new XmlReaderXlob(XmlReader.Create(reader, settings, inputContext));
		}

		public static Xlob Create(XmlReader reader)
		{
			return new XmlReaderXlob(reader);
		}

		public static Xlob Create(XmlDocument document)
		{
			return new XmlNodeXlob(document);
		}

		public static Xlob Create(string xmlFragment)
		{
			return new StringXlob(xmlFragment);
		}

		public static Xlob Create(Uri uri)
		{
			return new WebXlob(uri);
		}

		public static Xlob Create(IXmlSerializable obj)
		{
			return new XmlSerializableObjectXlob(obj);
		}

		public static implicit operator Xlob(XmlReader reader)
		{
			return new XmlReaderXlob(reader);
		}

		public static implicit operator Xlob(XmlDocument document)
		{
			return new XmlNodeXlob(document);
		}

		public static implicit operator Xlob(string xml)
		{
			return new StringXlob(xml);
		}

		public static implicit operator Xlob(Uri uri)
		{
			return new WebXlob(uri);
		}

		public abstract XmlReader OpenReader();

		public virtual void WriteTo(XmlWriter writer)
		{
			using (XmlReader reader = OpenReader())
				writer.WriteNode(reader, true);
		}

		public virtual void WriteTo(TextWriter writer)
		{
			var settings = new XmlWriterSettings {ConformanceLevel = ConformanceLevel.Fragment, CloseOutput = false};
			using (XmlWriter xw = XmlWriter.Create(writer, settings))
			{
				WriteTo(xw);
				xw.Flush();
			}
		}

		public virtual void WriteTo(Stream output, Encoding encoding)
		{
			var settings = new XmlWriterSettings {Encoding = encoding, ConformanceLevel = ConformanceLevel.Fragment, CloseOutput = false};
			using (XmlWriter xw = XmlWriter.Create(output, settings))
			{
				WriteTo(xw);
				xw.Flush();
			}
		}

		public override bool Equals(object obj)
		{
			var x = obj as Xlob;
			if (x == null) return false;
			return Equals(x);
		}
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public abstract bool Equals(Xlob xlob);

		protected virtual bool GetIsPersisted()
		{
			return _identifier != null;
		}

		protected virtual byte[] GetPersistedIdentifier()
		{
			return _identifier;
		}

		protected virtual void SetPersistedIdentifier(byte[] contents, object externalSource)
		{
			if (contents == null) throw new ArgumentNullException("contents");
			if (externalSource == null) throw new ArgumentNullException("externalSource");
			_identifier = contents;
			_externalSource = externalSource;
		}

		protected virtual object GetExternalSource()
		{
			return _externalSource;
		}
	}
}