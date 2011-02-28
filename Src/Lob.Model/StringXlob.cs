using System;
using System.IO;
using System.Xml;

namespace Lob.Model
{
	public class StringXlob : Xlob
	{
		readonly XmlParserContext _context;
		readonly XmlNodeType _nodeType;
		readonly string _xmlFragment;

		public StringXlob(string xmlFragment)
		{
			if (xmlFragment == null) throw new ArgumentNullException("xmlFragment");
			_xmlFragment = xmlFragment;
			_nodeType = XmlNodeType.Element;
		}

		public StringXlob(string xmlFragment, XmlParserContext parserContext) : this(xmlFragment)
		{
			_context = parserContext;
		}

		public StringXlob(string xmlFragment, XmlNodeType nodeType, XmlParserContext parserContext)
			: this(xmlFragment, parserContext)
		{
			_nodeType = nodeType;
			_context = parserContext;
		}

		public string XmlFragment
		{
			get { return _xmlFragment; }
		}

		public XmlNodeType NodeType
		{
			get { return _nodeType; }
		}

		public XmlParserContext ParserContext
		{
			get { return _context; }
		}

		public override XmlReader OpenReader()
		{
			return new XmlTextReader(_xmlFragment, _nodeType, _context);
		}

		public override void WriteTo(XmlWriter writer)
		{
			writer.WriteRaw(_xmlFragment);
		}

		public override void WriteTo(TextWriter writer)
		{
			writer.Write(_xmlFragment);
		}

		public override bool Equals(Xlob xlob)
		{
			if (xlob == this) return true;
			var sx = xlob as StringXlob;
			if (sx == null) return false;
			return _xmlFragment.Equals(sx._xmlFragment) && _context == sx._context && _nodeType == sx._nodeType;
		}
	}
}