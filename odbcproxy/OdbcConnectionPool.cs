using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Timers;

namespace net.pdynet.odbcproxy
{
    class OdbcConnectionPool
    {
        static readonly object slock = new object();
        static readonly object olock = new object();
        static OdbcConnectionPool instance = null;
        private Dictionary<String, PooledOdbcConnection> connections = null;
        private Timer closeAbandonedConnectionsTimer = null;

        public static OdbcConnectionPool Instance
        {
            get
            {
                lock (slock)
                {
                    if (instance == null)
                    {
                        instance = new OdbcConnectionPool();
                        instance.connections = new Dictionary<String, PooledOdbcConnection>();

                        Timer timer = new Timer();
                        timer.Interval = 10000;
                        timer.Elapsed += new ElapsedEventHandler(CloseAbandonedConnectionsHandler);
                        timer.AutoReset = false;
                        timer.Start();
                        instance.closeAbandonedConnectionsTimer = timer;
                    }

                    return instance;
                }
            }
        }

        public PooledOdbcConnection OdbcConnect(string connectionString, bool usingOleDb)
        {
            PooledOdbcConnection pooledOdbcConnection = null;

            OdbcConnection odbcConnection = null;
            OleDbConnection oleDbConnection = null;

            if (usingOleDb)
            {
                oleDbConnection = new OleDbConnection(connectionString);
                //oleDbConnection.ConnectionString = connectionString;
                oleDbConnection.Open();
            }
            else
            {
                odbcConnection = new OdbcConnection(connectionString);
                //odbcConnection.ConnectionString = connectionString;
                odbcConnection.Open();
            }

            lock (olock)
            {
                string connectionID = Guid.NewGuid().ToString();

                while (connections.ContainsKey(connectionID))
                    connectionID = Guid.NewGuid().ToString();

                DateTime connectionAutoCloseTime = DateTime.Now.AddMinutes(5);
                pooledOdbcConnection = new PooledOdbcConnection
                {
                    ID = connectionID,
                    OdbcConnection = odbcConnection,
                    OleDbConnection = oleDbConnection,
                    ConnectionAutoCloseTime = connectionAutoCloseTime
                };

                connections.Add(connectionID, pooledOdbcConnection);
            }

            return pooledOdbcConnection;
        }

        public void CloseConnection(string connectionID)
        {
            lock (olock)
            {
                if (connections.ContainsKey(connectionID))
                {
                    PooledOdbcConnection pooledOdbcConnection = connections[connectionID];
                    connections.Remove(connectionID);
                    CloseConnection(pooledOdbcConnection);
                }
                else
                    throw new ArgumentException("Connection ID (" + connectionID + ") not found in connection pool.");
            }
        }

        public PooledOdbcConnection GetConnection(string connectionID)
        {
            PooledOdbcConnection connection = null;

            lock (olock)
            {
                if (connections.ContainsKey(connectionID))
                {
                    connection = connections[connectionID];
                    DateTime connectionAutoCloseTime = DateTime.Now.AddMinutes(5);
                    connection.ConnectionAutoCloseTime = connectionAutoCloseTime;
                }
                else
                    throw new ArgumentException("Connection ID (" + connectionID + ") not found in connection pool.");
            }

            return connection;
        }

        public int GetActiveConnectionsCount()
        {
            int count = 0;

            lock (olock)
            {
                count = connections.Count;
            }

            return count;
        }

        public static string GetOdbcError(OdbcException e)
        {
            string errorMessages = "";

            for (int i = 0; i < e.Errors.Count; i++)
            {
                errorMessages += "Index #" + i + "\n" +
                                 "Message: " + e.Errors[i].Message + "\n" +
                                 "NativeError: " + e.Errors[i].NativeError.ToString() + "\n" +
                                 "Source: " + e.Errors[i].Source + "\n" +
                                 "SQL: " + e.Errors[i].SQLState + "\n";
            }

            return errorMessages;
        }

        private static void CloseAbandonedConnectionsHandler(object sender, EventArgs e)
        {
            OdbcConnectionPool instance = Instance;
            instance.CloseAbandonedConnections();
            instance.closeAbandonedConnectionsTimer.Start();
        }

        private void CloseAbandonedConnections()
        {
            lock (olock)
            {
                DateTime now = DateTime.Now;
                List<PooledOdbcConnection> abandonedConnections = connections
                    .Values
                    .Where(c => c.ConnectionAutoCloseTime <= now)
                    .ToList();

                foreach (var abandonedConnection in abandonedConnections)
                {
                    connections.Remove(abandonedConnection.ID);

                    try
                    {
                        CloseConnection(abandonedConnection);
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine(x.ToString());
                    }
                }
            }
        }

        private void CloseConnection(PooledOdbcConnection pooledOdbcConnection)
        {
            if (pooledOdbcConnection.OdbcConnection != null)
                pooledOdbcConnection.OdbcConnection.Close();

            if (pooledOdbcConnection.OleDbConnection != null)
                pooledOdbcConnection.OleDbConnection.Close();
        }
    }
}
