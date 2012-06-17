using System;
using System.IO;

namespace Lob.Model
{
	public abstract class Blob : IPersistedLob
	{
		const int BufferSize = 0x1000;
		object _externalSource;
		byte[] _identifier;

		public static Blob Empty
		{
			get { return new EmptyBlob(); }
		}

		bool IPersistedLob.IsPersisted
		{
			get { return GetIsPersisted(); }
		}

		byte[] IPersistedLob.GetPersistedIdentifier()
		{
			return GetPersistedIdentifier();
		}

		object IPersistedLob.GetExternalStore()
		{
			return GetExternalSource();
		}

		void IPersistedLob.SetPersistedIdentifier(byte[] contents, object externalStore)
		{
			SetPersistedIdentifier(contents, externalStore);
		}

		public static Blob Create(Stream input)
		{
			return new StreamBlob(input);
		}

		public static Blob Create(byte[] data)
		{
			return new ArrayBlob(data);
		}

		public static Blob Create(string filename)
		{
			return new FileBlob(filename);
		}

		public static implicit operator Blob(Stream input)
		{
			return new StreamBlob(input);
		}

		public static implicit operator Blob(byte[] data)
		{
			return new ArrayBlob(data);
		}

		public static implicit operator Blob(Uri uri)
		{
			return new WebBlob(uri);
		}

		public static implicit operator Blob(FileInfo file)
		{
			return new FileBlob(file);
		}

		public abstract Stream OpenReader();

		public virtual void WriteTo(Stream output)
		{
			using (Stream s = OpenReader())
			{
				var buffer = new byte[BufferSize];
				int readBytes;
				while ((readBytes = s.Read(buffer, 0, BufferSize)) > 0)
				{
					output.Write(buffer, 0, readBytes);
				}
			}
		}

		public override bool Equals(object obj)
		{
			var b = obj as Blob;
			if (b == null) return false;
			return Equals(b);
		}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public abstract bool Equals(Blob blob);
        
		protected virtual bool GetIsPersisted()
		{
			return _identifier != null;
		}

		protected virtual byte[] GetPersistedIdentifier()
		{
			return _identifier;
		}

		protected virtual void SetPersistedIdentifier(byte[] contents, object externalSource)
		{
			if (contents == null) throw new ArgumentNullException("contents");
			if (externalSource == null) throw new ArgumentNullException("externalSource");
			_identifier = contents;
			_externalSource = externalSource;
		}

		protected virtual object GetExternalSource()
		{
			return _externalSource;
		}
	}
}