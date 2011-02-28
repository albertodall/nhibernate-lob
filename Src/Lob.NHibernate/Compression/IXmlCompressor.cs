using System.IO;
using System.Xml;

namespace Lob.NHibernate.Compression
{
	public interface IXmlCompressor
	{
		XmlReader GetDecompressor(Stream input);
		XmlWriter GetCompressor(Stream output);
	}
}