using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Xml;

namespace Lob.Model
{
	public class WebXlob : Xlob
	{
		readonly ICredentials _credentials;
		readonly NameValueCollection _headers;
		readonly Uri _uri;

		public WebXlob(string uri) : this(uri, null, null)
		{
		}

		public WebXlob(Uri uri) : this(uri, null, null)
		{
		}

		public WebXlob(string uri, ICredentials credentials) : this(uri, null, credentials)
		{
		}

		public WebXlob(Uri uri, ICredentials credentials) : this(uri, null, credentials)
		{
		}

		public WebXlob(string uri, NameValueCollection customHeaders) : this(uri, customHeaders, null)
		{
		}

		public WebXlob(Uri uri, NameValueCollection customHeaders) : this(uri, customHeaders, null)
		{
		}

		public WebXlob(string uri, NameValueCollection customHeaders, ICredentials credentials)
		{
			if (uri == null) throw new ArgumentNullException("uri");
			_uri = new Uri(uri);
			_headers = customHeaders;
			_credentials = credentials;
		}

		public WebXlob(Uri uri, NameValueCollection customHeaders, ICredentials credentials)
		{
			if (uri == null) throw new ArgumentNullException("uri");
			_uri = uri;
			_headers = customHeaders;
			_credentials = credentials;
		}

		public Uri Uri
		{
			get { return _uri; }
		}

		public NameValueCollection CustomHeaders
		{
			get { return _headers; }
		}

		public ICredentials Credentials
		{
			get { return _credentials; }
		}

		public override XmlReader OpenReader()
		{
			WebRequest request = WebRequest.Create(_uri);
			if (_credentials != null)
				request.Credentials = _credentials;
			if (_headers != null)
				request.Headers.Add(_headers);
			WebResponse response = request.GetResponse();
			return new WebResponseXmlReader(response);
		}

		public override bool Equals(Xlob xlob)
		{
			if (xlob == null) return false;
			if (xlob == this) return true;
			var xb = xlob as WebXlob;
			if (xb != null) return _uri.Equals(xb._uri) && _credentials == xb._credentials && _headers == xb._headers;
			if (!_uri.IsFile) return false;
			var fb = xlob as FileXlob;
			if (fb == null) return false;
			return _uri.LocalPath.Equals(fb.Filename);
		}

		#region Nested type: WebResponseXmlReader

		class WebResponseXmlReader : XmlTextReader
		{
			readonly WebResponse _response;

			public WebResponseXmlReader(WebResponse response)
				: base(response.GetResponseStream(), XmlNodeType.Element, GetParser(response))
			{
				_response = response;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_response.Close();
				base.Dispose(disposing);
			}

			static XmlParserContext GetParser(WebResponse response)
			{
				var context = new XmlParserContext(null, null, null, XmlSpace.Preserve);
				var httpResponse = response as HttpWebResponse;
				if (httpResponse != null && httpResponse.CharacterSet != null)
				{
					TryGetEncoding(httpResponse, context);
				}
				context.BaseURI = response.ResponseUri.ToString();
				return context;
			}

			static void TryGetEncoding(HttpWebResponse httpResponse, XmlParserContext context)
			{
				try
				{
					if (httpResponse.CharacterSet != null)
					{
						context.Encoding = Encoding.GetEncoding(httpResponse.CharacterSet);
					}
				}
				catch
				{
				}
			}
		}

		#endregion
	}
}