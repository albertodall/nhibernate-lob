using System;
using Lob.Model;

namespace Lob.NHibernate.Tests
{
	public class Image
	{
		public virtual Blob Contents { get; set; }
		public virtual string FileName { get; set; }
		public virtual string ContentType { get; set; }
		public virtual int Size { get; set; }
		public virtual string Title { get; set; }
		public virtual Guid Id { get; set; }
	}
}