using SQLite;

namespace LembreteDividas
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetConnection();
    }
}
