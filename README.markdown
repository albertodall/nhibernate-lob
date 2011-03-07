Introduction
------------

The "Lob" project is a library for extending the functionality of ORM's (Currently just NHibernate) to support storing blobs as part of an Entity which may or may not be stored in the database.

The implementation allows you to switch between External Connnection providers, which will then store the contents of the blob in different locations (File System, Document database etc.)

This implementation is a fork of the Calyptus Lab's NHibernate.Lob project - for more info about the original project see the [NHibernate.Lob Project][1].

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
  - **Lob.NHibernate.Providers.FileSystemCas.FileSystemCasConnectionProvider** - Which allows storing data on the file system - it uses a crytopgrahic hash to determine an identifier for the file, which is then stored in the database (see [Content-addressable storage][2] for more details).

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
  
Compression
-----------

There is optional support for compressing the contents of a blob while being streamed to/from the external storage.

To enable this, you need to set the compression type as a parameter in the NHibernate definition for the Blob property, like so:

	<class name="Image">
		<id name="Id">
			<generator class="guid" />
		</id>
		<property name="FileName" />
		<property name="ContentType" />
		<property name="Size" />
		<property name="Title" />
		<property name="Contents">
			<type name="Lob.NHibernate.Type.ExternalBlobType, Lob.NHibernate">
				<param name="compression">gzip</param>
			</type>			
		</property>
	</class>
	
There is a well known type "gzip" which can be used, otherwise you will need to supply the fully qualified type name of a type implementing either the IStreamCompressor or IXmlCompressor (for XLob's) interface.

Note: Castle ActiveRecord does not support type parameters - to work around this you will need to inherit from ExternalBlobType/ExternalClobType/ExternalXlobType and specify the compressor explicity, then use those inherited types when specifying the mappings for your class. 

Length Of Field
---------------

What get's store in the database for a blob is called a "payload" - this payload may be of fixed length i.e. an identifier, such as that used by the FileSystemCas implementation.  Or may be an actual blob, as per the BlobArray implementation, or some form of hybride i.e. if less then 64K the value is stored as a blob, over 64K  it is stored in some external storage - it's up the implementor of the external provider.

The maximum length of the payload can be configured via a length property in your mappings i.e.

	<property name="Contents">
		<type name="Lob.NHibernate.Type.ExternalBlobType, Lob.NHibernate">
			<param name="length">256</param>
		</type>			
	</property>
	
There are also two sentinel values you can use in place of an exact length:

  - "default" - which will currently resolve to 32.
  - "max" - which will resolve to Int32.Max
  
The reason your normally wish to provide this value is when testing, where you may be using SchemaExport to create a test database - in these cases an IExternalBlobConnection implementation will not be available to the custom type, and so it will make the most conservative guess and use a BinaryBlob, where as you may wish to specify an exact length matching the payload size you expect for your chose External Provider.

Underlying DriverConnectionProvider 
-----------------------------------

The Lob.NHibernate implementation works by replacing the DriverConnectionProvider instance most NHibernate users make use of, and replaces it with an "ExternalBlobDriverConnectionProvider".  This class works as a wrapper, instantiating an underlying "DriverConnectionProvider" which then handles creation of the Driver, Connection etc.

In some cases (often while testing with a database such as Sqlite) you may wish to use an alternative DriverConnectionProvider, to do so you can specify in your nhibernate configuration:

	<config>
		....
		<add key="connection.lob.driver.provider" value="MyApp.CustomDriverConnectionProvider, MyApp" />
		...
	</config>

Quirks
------

One thing that can trip up some people is that when reading the contents of a Blob, if the underlying connection has been closed then this will fail.

The error you will recieve is an Exception with the message "The ExternalBlobConnection has been closed."

To work around this, ensure the code that fetches the opens a reader for the Blob is wrapped in an explicit transaction i.e.

	using (var tx = session.BeginTransaction()) 
	{
		// .. get the model ..
		
		// read the contents
		using (var stream = model.MyBlob.OpenReader())
		{
			// read the blob
		}
	}

This will ensure the connection is held open for the duration.

Garbage Collection
------------------

Due the way this library is implemented, deleting an entity which has one or more Blob/Clob/Xlob fields will not delete the underlying resource immediately.

To work around this problem you must perform periodic "garbage collection" on the external storage to remove any storage content that is no longer referenced by the database.

To do this setup a scheduled background task in your application (something as a simple as starting a thread that contains a sleep/work cycle invoking the garbage collector will do).

The garbage collector needs to be passed an NHibernate session - from there it will take care of the rest (it discoveres all the Blob etc. classes/properties via configuration metadata).

	var collector = new ExternalBlobGarbageCollector();

	using (ISession session = sessionFactory.OpenSession())	
	{
		collector.Collect(session);
	}

Note: If you are just using the ByteArrayConnectionProvider there is no need to run Garbage collection in the background.

Additional Resources
--------------------

**Calyptus Blog posts on the original NHibernate.Lob Project**

  - [Calyptus Blog Post #1][1]
  - [Calyptus Blog Post #2][3]  

Maintainer
----------

The current maintainer of this Fork is Alex Henderson a.k.a [@bittercoder][5].  

  - [Bittercoder's Blog][4].
  - [Bittercoder's Twitter][5].
  - [Bittercoder's Email][6].

  [1]: http://blog.calyptus.eu/seb/2009/03/large-object-storage-for-nhibernate-and-ddd-part-1-blobs-clobs-and-xlobs/
  [2]: http://en.wikipedia.org/wiki/Content-addressable_storage
  [3]: http://blog.calyptus.eu/seb/2009/03/large-object-storage-for-nhibernate-part-2-storage-options/
  [4]: http://blog.bittercoder.com/
  [5]: http://twitter.com/bittercoder
  [6]: http://mailto:bittercoder@gmail.com