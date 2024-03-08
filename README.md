# Tlabs.Data

### The Tlabs data persistence abstraction library.

This builds an abstraction layer on top of any ORM framework being used underneath of
* `IDataStore` as the core abstraction of a data storage facility.
* and `IRepo<TEntity>` as a repository to manipulate the persistence of a TEntity type.

Also some generalized functionality for serializing object data into streams is provided.
