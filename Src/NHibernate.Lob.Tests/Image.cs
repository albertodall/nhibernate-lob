using System;
using Calyptus.Lob;

namespace NHibernate.Lob.Tests
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