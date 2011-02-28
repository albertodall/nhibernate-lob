using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Lob.Compression;
using NHibernate.UserTypes;

namespace NHibernate.Lob
{
	internal static class Parameters
	{
		internal static void GetBlobSettings(IDictionary<string, string> parameters, out IStreamCompressor compression)
		{
			string compr = parameters == null ? null : parameters["compression"];
			if (string.IsNullOrEmpty(compr))
				compression = null;
			else if (compr.Equals("gzip", StringComparison.OrdinalIgnoreCase))
				compression = GZipCompressor.Instance;
			else
			{
				System.Type compressor = System.Type.GetType(compr);
				compression = (IStreamCompressor) Activator.CreateInstance(compressor);
				var parameterized = compression as IParameterizedType;
				if (parameterized != null)
					parameterized.SetParameterValues(parameters);
			}
		}

		internal static void GetClobSettings(IDictionary<string, string> parameters, out Encoding encoding, out IStreamCompressor compression)
		{
			string compr = parameters == null ? null : parameters["compression"];
			if (string.IsNullOrEmpty(compr))
				compression = null;
			else if (compr.Equals("gzip", StringComparison.OrdinalIgnoreCase))
				compression = GZipCompressor.Instance;
			else
			{
				System.Type compressor = System.Type.GetType(compr);
				compression = (IStreamCompressor) Activator.CreateInstance(compressor);
				var parameterized = compression as IParameterizedType;
				if (parameterized != null)
					parameterized.SetParameterValues(parameters);
			}

			string enc = parameters == null ? null : parameters["encoding"];
			if (!string.IsNullOrEmpty(enc)) encoding = Encoding.GetEncoding(enc);
			else encoding = null;
		}

		internal static void GetXlobSettings(IDictionary<string, string> parameters, out IXmlCompressor compression)
		{
			string compr = parameters == null ? null : parameters["compression"];
			if (string.IsNullOrEmpty(compr))
				compression = null;
			else if (compr.Equals("gzip", StringComparison.OrdinalIgnoreCase))
				compression = new XmlTextCompressor(GZipCompressor.Instance);
			else
			{
				System.Type compressor = System.Type.GetType(compr);
				if (typeof (IXmlCompressor).IsAssignableFrom(compressor))
					compression = (IXmlCompressor) Activator.CreateInstance(compressor);
				else if (typeof (IStreamCompressor).IsAssignableFrom(compressor))
					compression = new XmlTextCompressor((IStreamCompressor) Activator.CreateInstance(compressor));
				else
					throw new Exception("Unknown compression type.");
			}
			var parameterized = compression as IParameterizedType;
			if (parameterized != null) parameterized.SetParameterValues(parameters);
		}
	}
}