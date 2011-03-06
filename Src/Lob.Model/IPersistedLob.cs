namespace Lob.Model
{
	public interface IPersistedLob
	{
		bool IsPersisted { get; }
		byte[] GetPersistedIdentifier();
		object GetExternalStore();
		void SetPersistedIdentifier(byte[] contents, object externalStore);
	}
}