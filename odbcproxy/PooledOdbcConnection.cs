using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data.OleDb;

namespace net.pdynet.odbcproxy
{
    public class PooledOdbcConnection
    {
        public string ID { get; set; }
        public OdbcConnection OdbcConnection { get; set; }
        public OleDbConnection OleDbConnection { get; set; }
        public DateTime ConnectionAutoCloseTime { get; set; }
    }
}
