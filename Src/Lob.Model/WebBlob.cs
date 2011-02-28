using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Lob.Model
{
	public class WebBlob : Blob
	{
		readonly ICredentials _credentials;
		readonly NameValueCollection _headers;
		readonly Uri _uri;

		public WebBlob(string uri) : this(uri, null, null)
		{
		}

		public WebBlob(Uri uri) : this(uri, null, null)
		{
		}

		public WebBlob(string uri, ICredentials credentials) : this(uri, null, credentials)
		{
		}

		public WebBlob(Uri uri, ICredentials credentials) : this(uri, null, credentials)
		{
		}

		public WebBlob(string uri, NameValueCollection customHeaders) : this(uri, customHeaders, null)
		{
		}

		public WebBlob(Uri uri, NameValueCollection customHeaders) : this(uri, customHeaders, null)
		{
		}

		public WebBlob(string uri, NameValueCollection customHeaders, ICredentials credentials)
		{
			if (uri == null) throw new ArgumentNullException("uri");
			_uri = new Uri(uri);
			_headers = customHeaders;
			_credentials = credentials;
		}

		public WebBlob(Uri uri, NameValueCollection customHeaders, ICredentials credentials)
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

		public override Stream OpenReader()
		{
			var client = new WebClient();
			if (_credentials != null)
				client.Credentials = _credentials;
			if (_headers != null)
				client.Headers.Add(_headers);
			return client.OpenRead(_uri);
		}

		public override bool Equals(Blob blob)
		{
			if (blob == null) return false;
			if (blob == this) return true;
			var wb = blob as WebBlob;
			if (wb != null) return _uri.Equals(wb._uri) && _credentials == wb._credentials && _headers == wb._headers;
			if (!_uri.IsFile) return false;
			var fb = blob as FileBlob;
			if (fb == null) return false;
			return _uri.LocalPath.Equals(fb.Filename);
		}
	}
}