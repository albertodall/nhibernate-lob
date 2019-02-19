using System;
using System.Collections.Generic;
using System.Text;
using Lob.NHibernate.Compression;
using NHibernate.UserTypes;

namespace Lob.NHibernate.Support
{
	public static class Parameters
	{
		public static void GetBlobSettings(IDictionary<string, string> parameters, out IStreamCompressor compression, out int length)
		{
			length = ParseLength(parameters);

			string compr = null;
			if (parameters != null) parameters.TryGetValue("compression", out compr);
			
			if (string.IsNullOrEmpty(compr))
			{
				compression = null;
			}
			else if (compr.Equals("gzip", StringComparison.OrdinalIgnoreCase))
			{
				compression = GZipCompressor.Instance;
			}
			else
			{
				System.Type compressor = System.Type.GetType(compr);
				compression = (IStreamCompressor) Activator.CreateInstance(compressor);
				var parameterized = compression as IParameterizedType;
				if (parameterized != null)
					parameterized.SetParameterValues(parameters);
			}
		}

		static int ParseLength(IDictionary<string, string> parameters)
		{
			int length;
			string len = null;
			if (parameters != null) parameters.TryGetValue("length", out len);

			if (!string.IsNullOrEmpty(len))
			{
				if (len.Equals("max", StringComparison.OrdinalIgnoreCase))
				{
					length = Int32.MaxValue;
				}
				else if (len.EndsWith("default", StringComparison.OrdinalIgnoreCase))
				{
					length = 32;
				}
				else
				{
					length = Convert.ToInt32(len);
				}
			}
			else
			{
				length = 0;
			}

			return length;
		}

		public static void GetClobSettings(IDictionary<string, string> parameters, out Encoding encoding, out IStreamCompressor compression, out int length)
		{
			length = ParseLength(parameters);

			string compr = null;
			if (parameters != null) parameters.TryGetValue("compression", out compr);
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

			string enc = null;
			if (parameters != null) parameters.TryGetValue("encoding", out enc);

			encoding = !string.IsNullOrEmpty(enc) ? Encoding.GetEncoding(enc) : null;
		}

		public static void GetXlobSettings(IDictionary<string, string> parameters, out IXmlCompressor compression, out int length)
		{
			length = ParseLength(parameters);

			string compr = null;
			if (parameters != null) parameters.TryGetValue("compression", out compr);
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