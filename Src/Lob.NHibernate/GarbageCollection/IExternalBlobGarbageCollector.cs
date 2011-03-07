using System;
using NHibernate;

namespace Lob.NHibernate.GarbageCollection
{
	public interface IExternalBlobGarbageCollector
	{
		void Collect(ISession session);
		void Collect(ISession session, DateTime createdBefore);
	}
}