using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SQLite;
using SQLiteNetExtensions.Extensions;
using Demo.Encrypt;
using DemoProject.Configuration;
using DemoProject.Contract;

namespace DemoProject.Service
{
    public class SqliteService : ISQLiteService
    {
        private readonly string folderPath;
        private readonly SQLiteConnection connection;
        private static bool tableCreationChecked;

        private static readonly object lockObj = new object();
        private static Dictionary<Type, object> tableLocks = new Dictionary<Type, object>();
        readonly IFileService fileService;

        public SqliteService(IFileService fileService)
        {
            this.fileService = fileService;
            lock (lockObj)
            {
                folderPath = fileService.DocumentsFolderPath;

                if (connection == null)
                {
                    var configSettings = FreshMvvm.FreshIOC.Container.Resolve<IConfigurationSettings>();
                    var sqliteFilename = $"local-{configSettings.Country}{configSettings.EnvironmentName}.db";
                    var path = Path.Combine(folderPath, sqliteFilename);

                    connection = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
                }

                if (tableCreationChecked == false)
                {
                    InitializeTables();
                    tableCreationChecked = true;
                }
            }
        }


        public int Update<T>(object item) where T : new()
        {
            lock (tableLocks[typeof(T)])
            {
                return connection.Update(item);
            }
        }

        public int Count<T>() where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                return connection.Table<T>().Count();
            }
        }

        public int Count<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                return connection.Table<T>().Count(predicate);
            }
        }

        public virtual T GetItem<T>(object primaryKey) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                T itemResult = connection.Find<T>(primaryKey);

                return typeof(IEncryptable).IsAssignableFrom(typeof(T)) ?
                                           SqliteEncryptHelper.DecryptItem<T>(itemResult) : itemResult;
            }
        }

        public virtual T GetItem<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                T itemResult = connection.Find<T>(predicate);

                return typeof(IEncryptable).IsAssignableFrom(typeof(T)) ?
                                           SqliteEncryptHelper.DecryptItem<T>(itemResult) : itemResult;
            }
        }

        public T GetItemWithChildren<T>(object primaryKey) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                T itemResult = connection.FindWithChildren<T>(primaryKey, recursive: true);

                return DecryptWithChildren<T>(itemResult);
            }
        }

        public List<T> GetByQuery<T>(string query, params object[] args) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var result = connection.Query<T>(query, args);

                return typeof(IEncryptable).IsAssignableFrom(typeof(T)) ?
                                           SqliteEncryptHelper.DecryptList<T>(result) : result;
            }
        }

        public List<T> GetByQueryWithChildren<T>(string query, params object[] args) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var items = GetByQuery<T>(query, args);
                foreach (var item in items)
                    connection.GetChildren(item, true);

                return DecryptListWithChildren(items);
            }
        }

        public List<T> GetAll<T>() where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var result = connection.Table<T>().ToList();

                return typeof(IEncryptable).IsAssignableFrom(typeof(T)) ?
                                           SqliteEncryptHelper.DecryptList<T>(result) : result;
            }
        }

        public List<T> GetAll<T>(Expression<Func<T, bool>> filter) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var result = connection.Table<T>().Where(filter).ToList();

                return typeof(IEncryptable).IsAssignableFrom(typeof(T)) ?
                                           SqliteEncryptHelper.DecryptList<T>(result) : result;
            }
        }

        public List<T> GetAllWithChildren<T>() where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var result = connection.GetAllWithChildren<T>(recursive: true);

                return DecryptListWithChildren<T>(result);
            }
        }
        public List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> filter) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var result = connection.GetAllWithChildren<T>(filter, true);

                return DecryptListWithChildren<T>(result);
            }
        }

        public void ClearTable<T>() where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                connection.DeleteAll<T>();
            }
        }

        public void ClearTableAndChildren<T>() where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                var items = connection.GetAllWithChildren<T>(recursive: true);
                connection.DeleteAll(items, recursive: true);
            }
        }

        public void InsertAll<T>(IEnumerable<T> items) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                if (typeof(IEncryptable).IsAssignableFrom(typeof(T)))
                    items = SqliteEncryptHelper.EncryptList<T>(items.ToList());

                connection.InsertAll(items);
            }
        }

        public void InsertAllWithChildren<T>(IEnumerable<T> items) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                items = EncryptListWithChildren<T>(items.ToList());

                connection.InsertAllWithChildren(items, recursive: true);
            }
        }

        public void InsertOrReplaceAllWithChildren<T>(IEnumerable<T> items) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                items = EncryptListWithChildren<T>(items.ToList());

                connection.InsertOrReplaceAllWithChildren(items, recursive: true);
            }
        }

        private void InitializeTables()
        {
            AssignTableAndLock(typeof(Entity.Configuration));
        }

        private void AssignTableAndLock(params Type[] types)
        {
            if (types == null) return;

            object groupLockObject = new object();
            foreach (var type in types)
            {
                tableLocks.Add(type, groupLockObject);
                connection.CreateTable(type);
            }
        }

        public object GetTableLock<T>() where T : class, new()
        {
            object tableLockObj = tableLocks[typeof(T)];
            return tableLockObj;
        }

        public void Insert<T>(T item) where T : class, new()
        {
            InsertAll(new[] { item });
        }

        public int InsertOrReplace<T>(T item) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                if (typeof(IEncryptable).IsAssignableFrom(typeof(T)))
                    item = SqliteEncryptHelper.EncryptItem<T>(item);

                return connection.InsertOrReplace(item);
            }
        }

        public int Delete<T>(T item) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                return connection.Delete(item);
            }
        }

        public void DeleteWithChildren<T>(T item) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                connection.Delete(item, recursive: true);
            }
        }

        public int DeleteAll<T>(IEnumerable<int> items) where T : class, new()
        {
            lock (tableLocks[typeof(T)])
            {
                connection.DeleteAllIds<T>(items.Cast<object>());
                return 1;
            }
        }

        private T EncryptWithChildren<T>(T item) where T : class, new()
        {
            if (item == null) { return item; }

            var refTypeProperties = typeof(T).GetTypeInfo().DeclaredProperties.Where(
                                    x => x.CanWrite &&
                                    !x.PropertyType.GetTypeInfo().IsValueType &&
                                    x.PropertyType != typeof(string));
            foreach (var refTypeProperty in refTypeProperties)
            {
                var refTypePropertyValue = refTypeProperty.GetValue(item);
                if (typeof(IEnumerable).IsAssignableFrom(refTypeProperty.PropertyType))
                {
                    var seq = (refTypePropertyValue as IEnumerable).GetEnumerator();
                    while (seq.MoveNext())
                    {
                        EncryptWithChildren(seq.Current);
                    }
                }
                else
                {
                    EncryptWithChildren(refTypePropertyValue);
                }
            }

            if (typeof(IEncryptable).IsAssignableFrom(item.GetType()))
            {
                item = SqliteEncryptHelper.EncryptItem(item);
            }

            return item;
        }

        private List<T> EncryptListWithChildren<T>(IEnumerable<T> items) where T : class, new()
        {
            var result = new List<T>();
            foreach (var item in items)
            {
                var newlist = EncryptWithChildren(item);
                result.Add(newlist);
            }

            return result;
        }

        private T DecryptWithChildren<T>(T item) where T : class, new()
        {
            if (item == null) { return item; }

            var refTypeProperties = typeof(T).GetTypeInfo().DeclaredProperties.Where(
                                    x => x.CanWrite &&
                                    !x.PropertyType.GetTypeInfo().IsValueType &&
                                    x.PropertyType != typeof(string));
            foreach (var refTypeProperty in refTypeProperties)
            {
                var refTypePropertyValue = refTypeProperty.GetValue(item);
                if (typeof(IEnumerable).IsAssignableFrom(refTypeProperty.PropertyType))
                {
                    var seq = (refTypePropertyValue as IEnumerable).GetEnumerator();
                    while (seq.MoveNext())
                    {
                        DecryptWithChildren(seq.Current);
                    }
                }
                else
                {
                    DecryptWithChildren(refTypePropertyValue);
                }
            }

            if (typeof(IEncryptable).IsAssignableFrom(item.GetType()))
            {
                item = SqliteEncryptHelper.DecryptItem(item);
            }

            return item;
        }

        private List<T> DecryptListWithChildren<T>(IEnumerable<T> items) where T : class, new()
        {
            var result = new List<T>();
            foreach (var item in items)
            {
                var newitem = DecryptWithChildren<T>(item);
                result.Add(newitem);
            }

            return result;
        }

        public byte[] GetSqliteData()
        {
            byte[] dbBytes = GetSqliteDataWithLockAll();
            return dbBytes;
        }

        private byte[] GetSqliteDataWithLockAll()
        {
            var lockObjs = tableLocks.Select(s => s.Value).Distinct().ToList();
            var result = LockEach(lockObjs, 0);

            return result;
        }

        private byte[] LockEach(List<object> lockObjs, int countIndex)
        {
            if (lockObjs.Count > countIndex)
            {
                object currentLockObj = lockObjs[countIndex];
                lock (currentLockObj)
                {
                    return LockEach(lockObjs, countIndex + 1);
                }
            }

            var resultData = GetSqliteDataHelper();
            return resultData;
        }

        private byte[] GetSqliteDataHelper()
        {
            lock (lockObj)
            {
                byte[] dbBytes = null;

                string dbPath = this.connection.DatabasePath;
                dbBytes = fileService.ReadDBByte(dbPath);

                return dbBytes;
            }
        }
    }
}
