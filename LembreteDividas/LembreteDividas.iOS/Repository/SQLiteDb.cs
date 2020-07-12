using System;
using System.IO;
using SQLite;
using Xamarin.Forms;
using LembreteDividas.iOS;

[assembly: Dependency(typeof(SQLiteDb))]

namespace LembreteDividas.iOS
{
    public class SQLiteDb : ISQLiteDb
    {
        public SQLiteAsyncConnection GetConnection()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(documentsPath, "KontazaBancoDados.db3");

            return new SQLiteAsyncConnection(path);
        }
    }
}

