using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DemoProject.Contract
{
    public interface ISQLiteService
    {
        int Count<T>() where T : class, new();
        int Count<T>(Expression<Func<T, bool>> predicate) where T : class, new();

        void InsertAll<T>(IEnumerable<T> items) where T : class, new();
        void InsertAllWithChildren<T>(IEnumerable<T> items) where T : class, new();
        void InsertOrReplaceAllWithChildren<T>(IEnumerable<T> items) where T : class, new();

        void Insert<T>(T item) where T : class, new();
        int InsertOrReplace<T>(T item) where T : class, new();

        T GetItem<T>(object primaryKey) where T : class, new();
        T GetItem<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        T GetItemWithChildren<T>(object primaryKey) where T : class, new();
        List<T> GetByQuery<T>(string query, params object[] args) where T : class, new();
        List<T> GetByQueryWithChildren<T>(string query, params object[] args) where T : class, new();
        List<T> GetAll<T>() where T : class, new();
        List<T> GetAll<T>(Expression<Func<T, bool>> filter) where T : class, new();
        List<T> GetAllWithChildren<T>() where T : class, new();
        List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> filter) where T : class, new();

        void ClearTable<T>() where T : class, new();
        void ClearTableAndChildren<T>() where T : class, new();
        object GetTableLock<T>() where T : class, new();

        int Delete<T>(T item) where T : class, new();
        void DeleteWithChildren<T>(T item) where T : class, new();
        int DeleteAll<T>(IEnumerable<int> identifiers) where T : class, new();

        byte[] GetSqliteData();
    }
}
