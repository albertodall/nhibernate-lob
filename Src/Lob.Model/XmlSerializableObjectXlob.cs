using System;
using System.Xml;
using System.Xml.Serialization;

namespace Lob.Model
{
	public class XmlSerializableObjectXlob : Xlob
	{
		readonly IXmlSerializable _obj;

		public XmlSerializableObjectXlob(IXmlSerializable obj)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			_obj = obj;
		}

		public IXmlSerializable Object
		{
			get { return _obj; }
		}

		public override XmlReader OpenReader()
		{
			// NOTE: Renders in memory. Most objects will already be in memory and therefore small but some implementations might be large.
			XmlDocumentFragment frag = new XmlDocument().CreateDocumentFragment();
			_obj.WriteXml(frag.CreateNavigator().AppendChild());
			return new XmlNodeReader(frag);
		}

		public override void WriteTo(XmlWriter writer)
		{
			_obj.WriteXml(writer);
		}

		public override bool Equals(Xlob xlob)
		{
			var sx = xlob as XmlSerializableObjectXlob;
			if (sx == null) return false;
			return _obj == sx._obj;
		}
	}
}