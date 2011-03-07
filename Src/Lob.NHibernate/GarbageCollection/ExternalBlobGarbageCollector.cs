using System.Collections.Generic;
using System.Linq;
using Lob.NHibernate.Type;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Metadata;
using NHibernate.Type;

namespace Lob.NHibernate.GarbageCollection
{
	public class ExternalBlobGarbageCollector : IExternalBlobGarbageCollector
	{
		public void Collect(ISession session)
		{
			var connection = ((IExternalBlobConnection) session.Connection);

			if (!connection.SupportsGarbageCollection) return;

			List<byte[]> allIdentifiers = CollectAllIdentifiers(session).ToList();

			connection.GarbageCollect(allIdentifiers);
		}

		static IEnumerable<byte[]> CollectAllIdentifiers(ISession session)
		{
			ISessionFactory sessionFactory = session.SessionFactory;

			IDictionary<string, IClassMetadata> allMetadata = sessionFactory.GetAllClassMetadata();

			foreach (IClassMetadata classMetadata in allMetadata.Values)
			{
				string[] names = classMetadata.PropertyNames;
				IType[] types = classMetadata.PropertyTypes;

				for (int i = 0; i < names.Length; i++)
				{
					string name = names[i];
					IType type = types[i];

					if (typeof (ExternalBlobType).IsAssignableFrom(type.GetType()))
					{
						ICriteria criteria = CreateCriteria(session, classMetadata, name);

						foreach (ExternalBlob blob in criteria.List<ExternalBlob>())
						{
							yield return blob.Identifier;
						}
					}

					if (typeof (ExternalClobType).IsAssignableFrom(type.GetType()))
					{
						ICriteria criteria = CreateCriteria(session, classMetadata, name);

						foreach (ExternalClob clob in criteria.List<ExternalClob>())
						{
							yield return clob.Identifier;
						}
					}

					if (typeof (ExternalXlobType).IsAssignableFrom(type.GetType()))
					{
						ICriteria criteria = CreateCriteria(session, classMetadata, name);

						foreach (ExternalXlob xlob in criteria.List<ExternalXlob>())
						{
							yield return xlob.Identifier;
						}
					}
				}
			}
		}

		static ICriteria CreateCriteria(ISession session, IClassMetadata classMetadata, string name)
		{
			return session.CreateCriteria(classMetadata.EntityName)
				.SetProjection(Projections.Property(name));
		}
	}
}