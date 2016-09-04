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
                PooledOdbcConnection pooledOdbcConnection = connections[connectionID];
                connections.Remove(connectionID);

                if (pooledOdbcConnection.OdbcConnection != null)
                    pooledOdbcConnection.OdbcConnection.Close();

                if (pooledOdbcConnection.OleDbConnection != null)
                    pooledOdbcConnection.OleDbConnection.Close();
            }
        }

        public PooledOdbcConnection GetConnection(string connectionID)
        {
            PooledOdbcConnection connection = null;

            lock (olock)
            {
                connection = connections[connectionID];
                DateTime connectionAutoCloseTime = DateTime.Now.AddMinutes(5);
                connection.ConnectionAutoCloseTime = connectionAutoCloseTime;
            }

            return connection;
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
                    try
                    {
                        if (abandonedConnection.OdbcConnection != null)
                            abandonedConnection.OdbcConnection.Close();

                        if (abandonedConnection.OleDbConnection != null)
                            abandonedConnection.OleDbConnection.Close();
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine(x.ToString());
                    }

                    connections.Remove(abandonedConnection.ID);
                    //Console.WriteLine("removing connection " + abandonedConnection.ID);
                }
            }
        }
    }
}
