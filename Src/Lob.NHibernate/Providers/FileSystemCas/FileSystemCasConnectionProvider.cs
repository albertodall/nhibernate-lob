using System;
using System.IO;
using System.Security.AccessControl;
using System.Web;
using System.Web.Hosting;

namespace Lob.NHibernate.Providers.FileSystemCas
{
	public class FileSystemCasConnectionProvider : AbstractExternalBlobConnectionProvider
	{
		string _hash;
		string _path;

		public FileSystemCasConnectionProvider()
		{
		}

		public FileSystemCasConnectionProvider(string path)
		{
			SetPath(path);
		}

		public FileSystemCasConnectionProvider(string path, string hash) : this(path)
		{
			SetHash(hash);
		}

		public override string ConnectionString
		{
			get
			{
				string cs = (_path != null) ? "Path=" + _path : null;
				if (_hash != null)
				{
					if (cs == null) cs = "";
					else cs += ";";
					cs += "Hash=" + _hash;
				}
				return cs;
			}
			set
			{
				if (value == null)
				{
					SetPath(null);
					return;
				}
				string[] props = value.Split(';');
				foreach (string p in props)
				{
					string[] kv = p.Split(new[] {'='}, 2);
					if (kv[0].Equals("Path", StringComparison.OrdinalIgnoreCase))
						SetPath(kv[1].Trim());
					else if (kv[0].Equals("Hash", StringComparison.OrdinalIgnoreCase))
						SetHash(kv[1].Trim());
				}
			}
		}

		void SetPath(string storagePath)
		{
			if (storagePath == null) throw new NullReferenceException("Storage path cannot be null. Must be existing path.");
			if (!Path.IsPathRooted(storagePath))
			{
				if (storagePath.StartsWith("~/"))
				{
					HttpContext context = HttpContext.Current;
					if (context != null)
						storagePath = context.Request.MapPath(storagePath);
					else if (HostingEnvironment.IsHosted)
						storagePath = HostingEnvironment.MapPath(storagePath);
				}
				else
					storagePath = Path.GetFullPath(storagePath);
			}
			if (!Directory.Exists(storagePath))
			{
				try
				{
					Directory.CreateDirectory(storagePath);
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("Storage path: {0} does not exist, and an attempt to create it failed.", storagePath), ex);
				}
			}
			_path = storagePath;
		}

		void SetHash(string hash)
		{
			_hash = hash;
		}

		public override IExternalBlobConnection GetConnection()
		{
			return new FileSystemCasConnection(_path, _hash);
		}
	}
}