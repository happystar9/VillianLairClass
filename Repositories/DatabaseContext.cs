using System;
using System.Data.SQLite;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Database context that manages the SQLite connection
    /// Replaces the static singleton pattern with an injectable dependency
    /// </summary>
    public class DatabaseContext : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private bool _disposed = false;

        public DatabaseContext(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

        public SQLiteConnection Connection => _connection;

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}
