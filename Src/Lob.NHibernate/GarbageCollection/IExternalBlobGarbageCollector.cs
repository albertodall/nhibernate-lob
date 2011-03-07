using NHibernate;

namespace Lob.NHibernate.GarbageCollection
{
	public interface IExternalBlobGarbageCollector
	{
		void Collect(ISession session);
	}
}