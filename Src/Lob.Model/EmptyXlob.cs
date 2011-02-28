using System.IO;
using System.Text;
using System.Xml;

namespace Lob.Model
{
	public class EmptyXlob : Xlob
	{
		public override XmlReader OpenReader()
		{
			return new XmlNodeReader(new XmlDocument());
		}

		public override void WriteTo(Stream output, Encoding encoding)
		{
		}

		public override void WriteTo(TextWriter writer)
		{
		}

		public override void WriteTo(XmlWriter writer)
		{
		}

		public override bool Equals(Xlob xlob)
		{
			var ex = xlob as EmptyXlob;
			if (ex != null) return true;
			var nx = xlob as XmlNodeXlob;
			if (nx != null && (nx.Node.NodeType == XmlNodeType.Document || nx.Node.NodeType == XmlNodeType.DocumentFragment) && nx.Node.ChildNodes.Count == 0) return true;
			return false;
		}
	}
}