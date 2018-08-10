# SqliteEncryption
Sqlite Encryption library


in file of SqliteService.cs, please be aware that the IFileService.cs needs to be implemented in your iOS and Android project to be able to set up the Sqlite and connection, also need to register the IFileService  like

//container.Register<IFileService, FileService>(); 
 
pay attention to these couple functions, especially when it’s in term of the recursive implementations

private T EncryptWithChildren<T>(T item) where T : class, new()  { }
private T DecryptWithChildren<T>(T item) where T : class, new() {}
