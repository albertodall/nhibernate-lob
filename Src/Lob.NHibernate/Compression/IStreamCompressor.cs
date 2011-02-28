using System.IO;

namespace Lob.NHibernate.Compression
{
	public interface IStreamCompressor
	{
		Stream GetDecompressor(Stream input);
		Stream GetCompressor(Stream output);
	}
}