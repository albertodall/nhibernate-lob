using System;
using System.IO;

namespace NHibernate.Lob.Tests
{
	public class TempFolder : IDisposable
	{
		readonly string _folder;

		public TempFolder()
		{
			_folder = Path.Combine(Path.GetTempPath(), "XUnitTests" + Guid.NewGuid());
			Directory.CreateDirectory(_folder);
		}

		public string Folder
		{
			get { return _folder; }
		}

		public void Dispose()
		{
			try
			{
				Directory.Delete(Folder, true);
			}
			catch
			{
				// do nothing
			}
		}
	}
}