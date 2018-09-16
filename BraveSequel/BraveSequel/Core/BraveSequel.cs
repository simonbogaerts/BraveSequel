using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using BraveSequel.Enums;
using BraveSequel.Interfaces;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace BraveSequel.Core
{
    public sealed class BraveSequel : IBraveSequel, IDisposable
    {
        #region properties and variables

        public SqlProvider Provider { get; set; }
        public IsolationLevel IsolationLevel { get; set; }
        private IDbTransaction Transaction { get; }
        private IDbConnection Connection { get; }

        private static AsyncLocal<Stack<BraveSequel>> BraveStack { get; set; }
            = new AsyncLocal<Stack<BraveSequel>>();

        private static string _connectionString;
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    throw new InvalidOperationException();
                }

                return _connectionString;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(_connectionString))
                {
                    throw new InvalidOperationException();
                }

                _connectionString = value;
            }
        }

        #endregion

        #region constructor

        private BraveSequel()
        {
            Connection = NewConnection();

            Connection
                .Open();

            Transaction = Connection
                .BeginTransaction(IsolationLevel);
        }

        #endregion

        #region public methods

        public void Commit()
        {
            Transaction
                .Commit();
        }

        public void Rollback()
        {
            Transaction
                .Rollback();
        }

        public static BraveSequel Current()
        {
            if (BraveStack.Value.Count == 0)
            {
                throw new InvalidOperationException();
            }

            return BraveStack
                .Value
                .Peek();
        }

        public IDbCommand CreateCommand()
        {
            return Connection
                .CreateCommand();
        }

        public static bool Active()
        {
            return BraveStack
                       .Value
                       .Count > 0;
        }

        #endregion

        #region private methods

        private IDbConnection NewConnection()
        {
            switch (Provider)
            {
                case SqlProvider.MsSql:
                    return new SqlConnection(ConnectionString);

                case SqlProvider.MySql:
                    return new MySqlConnection(ConnectionString);

                case SqlProvider.PostgreSql:
                    return new NpgsqlConnection(ConnectionString);

                case SqlProvider.Oracle:
                    return new OracleConnection(ConnectionString);

                case SqlProvider.None:
                default:
                    throw new InvalidOperationException();
            }
        }

        void IDisposable.Dispose()
        {
            try
            {
                Transaction.Dispose();
                Connection.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                BraveStack
                    .Value
                    .Pop();
            }
        }

        #endregion
    }
}