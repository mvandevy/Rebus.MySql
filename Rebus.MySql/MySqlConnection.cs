using System;
using System.Data;
using System.Threading.Tasks;
using MySqlData = MySql.Data;

namespace Rebus.MySql
{
    /// <summary>
    /// Wraps an opened <see cref="MySqlData.MySqlClient.MySqlConnection"/> and makes it easier to work with it
    /// </summary>
    public class MySqlConnection : IDisposable
    {
        readonly MySqlData.MySqlClient.MySqlConnection _currentConnection;
        MySqlData.MySqlClient.MySqlTransaction _currentTransaction;

        bool _disposed;

        /// <summary>
        /// Constructs the wrapper with the given connection and transaction
        /// </summary>
        public MySqlConnection(MySqlData.MySqlClient.MySqlConnection currentConnection, MySqlData.MySqlClient.MySqlTransaction currentTransaction)
        {
            if (currentConnection == null) throw new ArgumentNullException(nameof(currentConnection));
            if (currentTransaction == null) throw new ArgumentNullException(nameof(currentTransaction));
            _currentConnection = currentConnection;
            _currentTransaction = currentTransaction;
        }

        /// <summary>
        /// Creates a new command, enlisting it in the current transaction
        /// </summary>
        public MySqlData.MySqlClient.MySqlCommand CreateCommand()
        {
            var command = _currentConnection.CreateCommand();
            command.Transaction = _currentTransaction;
            return command;
        }

        /// <summary>
        /// Completes the transaction
        /// </summary>

        public void Complete()
        {
            if (_currentTransaction == null) return;
            using (_currentTransaction)
            {
                _currentTransaction.Commit();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Rolls back the transaction if it hasn't been completed
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                try
                {
                    if (_currentTransaction == null) return;
                    using (_currentTransaction)
                    {
                        try
                        {
                            _currentTransaction.Rollback();
                        }
                        catch { }
                        _currentTransaction = null;
                    }
                }
                finally
                {
                    _currentConnection.Dispose();
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}