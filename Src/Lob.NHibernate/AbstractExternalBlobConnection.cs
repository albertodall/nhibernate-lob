using System;
using System.Collections.Generic;
using System.IO;

namespace Lob.NHibernate
{
	public abstract class AbstractExternalBlobConnection : IExternalBlobConnection
	{
		const int BufferSize = 0x1000;
		List<WeakReference> _openedStreams;

		protected AbstractExternalBlobConnection()
		{
			_openedStreams = new List<WeakReference>();
		}

		public abstract int BlobIdentifierLength { get; }

		public abstract void Delete(byte[] blobIdentifier);

		public abstract void GarbageCollect(IEnumerable<byte[]> livingBlobIdentifiers);

		public abstract bool Equals(IExternalBlobConnection connection);

		public virtual byte[] Store(Stream input)
		{
			if (input == null) throw new ArgumentNullException("input");
			if (!input.CanRead) throw new Exception("Input stream is not in a readable state.");
			using (ExternalBlobWriter s = OpenWriter())
			{
				var buffer = new byte[BufferSize];
				int readBytes;
				while ((readBytes = input.Read(buffer, 0, BufferSize)) > 0)
					s.Write(buffer, 0, readBytes);
				return s.Commit();
			}
		}

		public void ReadInto(byte[] blobIdentifier, Stream output)
		{
			if (blobIdentifier == null) throw new ArgumentNullException("blobIdentifier");
			if (output == null) throw new ArgumentNullException("output");
			if (!output.CanWrite) throw new Exception("Output stream is not in a writable state.");
			using (Stream s = OpenReader(blobIdentifier))
			{
				var buffer = new byte[BufferSize];
				int readBytes;
				while ((readBytes = s.Read(buffer, 0, BufferSize)) > 0)
				{
					output.Write(buffer, 0, readBytes);
				}
			}
		}

		Stream IExternalBlobConnection.OpenReader(byte[] blobIdentifier)
		{
			if (_openedStreams == null) throw new Exception("The ExternalBlobConnection has been closed.");
			Stream s = OpenReader(blobIdentifier);
			_openedStreams.Add(new WeakReference(s));
			return s;
		}

		ExternalBlobWriter IExternalBlobConnection.OpenWriter()
		{
			if (_openedStreams == null) throw new Exception("The ExternalBlobConnection has been closed.");
			ExternalBlobWriter w = OpenWriter();
			_openedStreams.Add(new WeakReference(w));
			return w;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public abstract Stream OpenReader(byte[] blobIdentifier);

		public abstract ExternalBlobWriter OpenWriter();

		public override bool Equals(object obj)
		{
			return Equals(obj as IExternalBlobConnection);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		~AbstractExternalBlobConnection()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (_openedStreams != null)
			{
				List<WeakReference> str = _openedStreams;
				_openedStreams = null;
				foreach (WeakReference r in str)
				{
					var d = r.Target as IDisposable;
					if (d != null) d.Dispose();
				}
			}
			GC.SuppressFinalize(this);
		}
	}
}