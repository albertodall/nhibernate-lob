using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Lob.Model
{
	public class WebClob : Clob
	{
		readonly ICredentials _credentials;
		readonly NameValueCollection _headers;
		readonly Uri _uri;

		public WebClob(string uri) : this(uri, null, null)
		{
		}

		public WebClob(Uri uri) : this(uri, null, null)
		{
		}

		public WebClob(string uri, ICredentials credentials) : this(uri, null, credentials)
		{
		}

		public WebClob(Uri uri, ICredentials credentials) : this(uri, null, credentials)
		{
		}

		public WebClob(string uri, NameValueCollection customHeaders) : this(uri, customHeaders, null)
		{
		}

		public WebClob(Uri uri, NameValueCollection customHeaders) : this(uri, customHeaders, null)
		{
		}

		public WebClob(string uri, NameValueCollection customHeaders, ICredentials credentials)
		{
			if (uri == null) throw new ArgumentNullException("uri");
			_uri = new Uri(uri);
			_headers = customHeaders;
			_credentials = credentials;
		}

		public WebClob(Uri uri, NameValueCollection customHeaders, ICredentials credentials)
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

		public override TextReader OpenReader()
		{
			WebRequest request = WebRequest.Create(_uri);

			if (_credentials != null) request.Credentials = _credentials;

			if (_headers != null) request.Headers.Add(_headers);

			WebResponse response = request.GetResponse();

			var httpResponse = response as HttpWebResponse;

			return httpResponse != null ? new WebResponseReader(httpResponse) : new WebResponseReader(response);
		}

		public override bool Equals(Clob clob)
		{
			if (clob == null) return false;
			if (clob == this) return true;
			var wb = clob as WebClob;
			if (wb != null) return _uri.Equals(wb._uri) && _credentials == wb._credentials && _headers == wb._headers;
			if (!_uri.IsFile) return false;
			var fb = clob as FileClob;
			if (fb == null) return false;
			return _uri.LocalPath.Equals(fb.Filename);
		}

		#region Nested type: WebResponseReader

		class WebResponseReader : StreamReader
		{
			readonly WebResponse _response;

			public WebResponseReader(HttpWebResponse response) : base(response.GetResponseStream(), GetEncoding(response.CharacterSet))
			{
				_response = response;
			}

			public WebResponseReader(WebResponse response) : base(response.GetResponseStream(), true)
			{
				_response = response;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					BaseStream.Close();
					_response.Close();
				}
				base.Dispose(disposing);
			}

			static Encoding GetEncoding(string charset)
			{
				if (charset == null) return GetDefaultEncoding();

				try
				{
					return Encoding.GetEncoding(charset);
				}
				catch
				{
					return GetDefaultEncoding();
				}
			}

			static Encoding GetDefaultEncoding()
			{
				return Encoding.GetEncoding("iso8859-1");
			}
		}

		#endregion
	}
}