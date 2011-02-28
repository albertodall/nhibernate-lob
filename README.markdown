Introduction
------------

The NHibernate.LOB project is a library for extending the functionality of NHibernate to support storing blobs as part of an Entity which may or may not be stored in the database.

The implementation allows you to switch between External Connnection providers, which will then store the contents of the blob in different locations (FileSystem, Document database etc.)

This implementation is a fork of the Calyptus Labs NHibernate.Lob project - for more info about the original project see the [NHibernate.Lob Project][1].

Why would I want this?
----------------------

If you enjoy the convenience of storing files in the database as blobs, but want the flexibility to switch to storing files somewhere else, this project may be for you.

Why would I not want to use this?
---------------------------------

The project is fairly immature, so you will most likely need to implement your own ConnectionProvider/Connection classes based on the ByteArrayConnectionProvider / FileSystemCasConnectionProvider examples specific to your needs.

What's supported
----------------
Currently there are two connection providers

  - **Lob.NHibernate.Providers.ByteArray.ByteArrayConnectionProvider** - Which allows storing data in a blob/image column (as you probably do today).
  - **Lob.NHibernate.Providers.FileSystemCas.FileSystemCasConnectionProvider** - Which allows storing data on the file system - it uses a crytopgrahic hash to determine an identifier for the file, which is then stored in the database (see Content-addressable storage for more details) http://en.wikipedia.org/wiki/Content-addressable_storage.

Future plans for additional storage providers include:

  - Chunked Sql Table (i.e. using a seperate database table to store file chunks of a fixed say, say 64KB).
  - File System provider that uses naming inferred from the associated object (i.e. associated entities identifier + some fixed key + property containing file name) - which allows for ease of recovery of lost files in the case of system failure.
  - Support for native streaming blob support in various databases, including support for Sql Server File Streams.
  - S3 Storage

The project is currently built against NHibernate 2.1.0.4000, targeting the .Net Framework 3.5 and uses Visual Studio 2010 project/solution files.

Types of Large Object
---------------------

3 Types of Large Object are supported:

  - Blob - Binary Large Object (equivalent to a byte array / stream)
  - Clob - String/Text Large Object (equivalent to a String/StringBuilder/StreamReader/StreamWriter)
  - XLob - Large XML Object (equivalent to an XDocument, XEelement, XmlFragment, XmlReader/XmlWriter)

Quick Example
-------------

**Model**

	public class Image
	{
		public virtual Blob Contents { get; set; }
		public virtual string FileName { get; set; }
		public virtual string ContentType { get; set; }
		public virtual int Size { get; set; }
		public virtual string Title { get; set; }
		public virtual Guid Id { get; set; }
	}    
	
**Mapping**

    <hibernate-mapping xmlns="urn:nhibernate-mapping-2.2" assembly="Lob.NHibernate.Tests" namespace="Lob.NHibernate.Tests">
    <class name="Image">
     	<id name="Id">
		    <generator class="guid" />
	    </id>
	    <property name="FileName" />
	    <property name="ContentType" />
	    <property name="Size" />
	    <property name="Title" />
    	<property name="Contents" type="Lob.NHibernate.Type.ExternalBlobType, Lob.NHibernate" />
    </class>
    </hibernate-mapping>
	
**Configuration**

	<config>
		<!-- normal NHibernate setup -->
		<add key="connection.driver_class" value="NHibernate.Driver.NpgsqlDriver" />
		<add key="dialect" value="NHibernate.Dialect.PostgreSQL82Dialect" />			
		<add key="connection.connection_string_name" value="Default" />
		<!-- Lob.NHibernate setup -->
		<add key="connection.provider" value="Lob.NHibernate.ExternalBlobDriverConnectionProvider, Lob.NHibernate" />			
		<add key="connection.lob.external.provider" value="Lob.NHibernate.Providers.FileSystemCas.FileSystemCasConnectionProvider, Lob.NHibernate" />
		<add key="connection.lob.external.connection_string" value="Path=c:\data\;Hash=SHA256" />
	</config>

**Usage - Saving**
	
	using (ISession session = sessionFactory.OpenSession())
	{
		using (ITransaction tx = session.BeginTransaction())
		{
			var image = new Image
			{
				FileName = "test.txt",
				ContentType = "text/plain",
				Size = 10,
				Contents = Blob.Create(new byte[] { 1, 2, 3, 4 }),
				Title = "test"
			};

			session.Save(image);

			tx.Commit();				
		}
	}
	
**Usage - Retrieving**

	using (ISession session = sessionFactory.OpenSession())
	{
		using (ITransaction tx = session.BeginTransaction())
		{
			var imageFromDb = session.Get<Image>(imageId);

			using (Stream streamReader = imageFromDb.Contents.OpenReader())
			{
				var buffer = new byte[1024];
				int length = streamReader.Read(buffer, 0, buffer.Length);
				Assert.Equal(4, length);
				Assert.Equal(1, buffer[0]);
				Assert.Equal(2, buffer[1]);
				Assert.Equal(3, buffer[2]);
				Assert.Equal(4, buffer[3]);
			}
		}
	}
	
Project Structure
-----------------

  - **Lob.Model**  - Persistence Ignorant set of models representing different sources of Blobs/Clobs/XLobs i.e. Web Requests, Empty blobs, From arrays, streams etc.  The base abstract classes Blob, Clob and Xlob provide static convenience members for instantianting these various classes.
  - **Lob.NHibernate** - Implementation of an external storage provider that wraps the NHibernate connection mechanics combined with some custom types that takes care of saving blobs to external storage.
  - **Lob.NHibernate.Tests** - XUnit.Net Tests for the framework (Very much a WIP as the original library had no associated tests).
  
Additional Resources
--------------------

**Calyptus Blog posts on the original NHibernate.Lob Project**

  - [Calyptus Blog Post #1][2]
  - [Calyptus Blog Post #2][3]  

Maintainer
----------

The current maintainer of this Fork is Alex Henderson a.k.a [@bittercoder][5].  

  - [Bittercoder's Blog][4].
  - [Bittercoder's Twitter][5].
  - [Bittercoder's Email][6].

  [1]: http://blog.calyptus.eu/seb/2009/03/large-object-storage-for-nhibernate-and-ddd-part-1-blobs-clobs-and-xlobs/
  [2]: http://blog.calyptus.eu/seb/2009/03/large-object-storage-for-nhibernate-and-ddd-part-1-blobs-clobs-and-xlobs/
  [3]: http://blog.calyptus.eu/seb/2009/03/large-object-storage-for-nhibernate-part-2-storage-options/
  [4]: http://blog.bittercoder.com/
  [5]: http://twitter.com/bittercoder
  [6]: http://mailto:bittercoder@gmail.com