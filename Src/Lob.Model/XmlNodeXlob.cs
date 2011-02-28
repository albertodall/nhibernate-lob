using System;
using System.Xml;

namespace Lob.Model
{
	public class XmlNodeXlob : Xlob
	{
		readonly XmlNode _node;

		public XmlNodeXlob(XmlNode node)
		{
			if (node == null) throw new ArgumentNullException("node");
			_node = node;
		}

		public XmlNode Node
		{
			get { return _node; }
		}

		public override XmlReader OpenReader()
		{
			return new XmlNodeReader(_node);
		}

		public override void WriteTo(XmlWriter writer)
		{
			_node.WriteTo(writer);
		}

		public override bool Equals(Xlob xlob)
		{
			if (xlob == null) return false;
			if (this == xlob) return true;
			var xn = xlob as XmlNodeXlob;
			if (xn == null) return false;
			return _node.Equals(xn._node);
		}
	}
}