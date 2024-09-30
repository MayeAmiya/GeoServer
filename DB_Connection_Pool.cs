using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GeoServer
{
    internal class DB_Connection_Pool
    {
        private readonly string connectionString;
        private readonly Stack<MySqlConnection> connectionPool;
        private readonly int maxConnections;

        public DB_Connection_Pool(string connectionString, int maxConnections = 100)
        {
            this.connectionString = connectionString;
            this.maxConnections = maxConnections;
            connectionPool = new Stack<MySqlConnection>(maxConnections);
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < maxConnections; i++)
            {
                var connection = new MySqlConnection(connectionString);
                connectionPool.Push(connection);
            }
        }

        public MySqlConnection GetConnection()
        {
            if (connectionPool.Count > 0)
            {
                return connectionPool.Pop();
            }
            throw new InvalidOperationException("No available connections in the pool.");
        }

        public void ReleaseConnection(MySqlConnection connection)
        {
            connectionPool.Push(connection);
        }
    }
}
