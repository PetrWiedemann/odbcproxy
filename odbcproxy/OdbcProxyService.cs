using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace net.pdynet.odbcproxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    class OdbcProxyService : IOdbcProxyService
    {

        #region IOdbcProxyService Members

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public ConnectResponse Connect(ConnectRequest request)
        {
            ConnectResponse response = new ConnectResponse();
            
            // Overeni, zda byl zadan Connection string;
            if (StringUtils.IsBlank(request.ConnectionString))
            {
                response.Error = "Blank connection string.";
            }
            else
            {
                // Otevreni ODBC.
                try
                {
                    PooledOdbcConnection pooledOdbcConnection = OdbcConnectionPool.Instance.OdbcConnect(request.ConnectionString, request.UsingOleDb);
                    response.ConnectionID = pooledOdbcConnection.ID;
                    response.Success = true;
                    response.ConnectionAutoCloseTime = pooledOdbcConnection.ConnectionAutoCloseTime;
                }
                catch (OdbcException x)
                {
                    response.Error = OdbcConnectionPool.GetOdbcError(x);
                }
                catch (Exception x)
                {
                    response.Error = x.ToString();
                }
            }

            return response;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public CloseResponse Close(CloseRequest request)
        {
            CloseResponse response = new CloseResponse();

            if (StringUtils.IsBlank(request.ConnectionID))
            {
                response.Error = "Blank connection ID.";
            }
            else
            {
                try
                {
                    OdbcConnectionPool.Instance.CloseConnection(request.ConnectionID);
                    response.Success = true;
                }
                catch (OdbcException x)
                {
                    response.Error = OdbcConnectionPool.GetOdbcError(x);
                }
                catch (Exception x)
                {
                    response.Error = x.ToString();
                }
            }

            return response;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public SelectResponse Select(SelectRequest request)
        {
            SelectResponse response = new SelectResponse();

            if (StringUtils.IsBlank(request.ConnectionID))
            {
                response.Error = "Blank connection ID.";
            }
            else if (StringUtils.IsBlank(request.Query))
            {
                response.Error = "Query is blank.";
            }
            else
            {
                try
                {
                    PooledOdbcConnection pooledOdbcConnection = OdbcConnectionPool.Instance.GetConnection(request.ConnectionID);

                    List<RowData> rows = new List<RowData>();
                    response.Rows = rows;

                    if (pooledOdbcConnection.OdbcConnection != null)
                    {
                        OdbcConnection connection = pooledOdbcConnection.OdbcConnection;

                        using (OdbcCommand command = new OdbcCommand(request.Query, connection))
                        {
                            using (OdbcDataReader reader = command.ExecuteReader(CommandBehavior.Default | CommandBehavior.KeyInfo))
                            {
                                if (request.ReturnSchemaTable)
                                    response.SchemaTable = getSchemaTable(reader);

                                while (reader.Read())
                                {
                                    RowData rowData = new RowData();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        rowData.Add(reader.GetName(i), getColumnValue(reader, i));
                                    }
                                    rows.Add(rowData);
                                }
                            }
                        }
                    }
                    else
                    {
                        OleDbConnection connection = pooledOdbcConnection.OleDbConnection;

                        using (OleDbCommand command = new OleDbCommand(request.Query, connection))
                        {
                            using (OleDbDataReader reader = command.ExecuteReader(CommandBehavior.Default | CommandBehavior.KeyInfo))
                            {
                                if (request.ReturnSchemaTable)
                                    response.SchemaTable = getSchemaTable(reader);

                                while (reader.Read())
                                {
                                    RowData rowData = new RowData();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        rowData.Add(reader.GetName(i), getColumnValue(reader, i));
                                    }
                                    rows.Add(rowData);
                                }
                            }
                        }
                    }

                    response.Success = true;
                    response.ConnectionAutoCloseTime = pooledOdbcConnection.ConnectionAutoCloseTime;
                }
                catch (OdbcException x)
                {
                    response.Error = OdbcConnectionPool.GetOdbcError(x);
                }
                catch (Exception x)
                {
                    response.Error = x.ToString();
                }
            }

            return response;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public ExecuteResponse ExecuteCommand(ExecuteRequest request)
        {
            ExecuteResponse response = new ExecuteResponse();

            if (StringUtils.IsBlank(request.ConnectionID))
            {
                response.Error = "Blank connection ID.";
            }
            else if (StringUtils.IsBlank(request.Query))
            {
                response.Error = "Query is blank.";
            }
            else
            {
                try
                {
                    PooledOdbcConnection pooledOdbcConnection = OdbcConnectionPool.Instance.GetConnection(request.ConnectionID);

                    if (pooledOdbcConnection.OdbcConnection != null)
                    {
                        OdbcConnection connection = pooledOdbcConnection.OdbcConnection;

                        using (OdbcCommand command = new OdbcCommand(request.Query, connection))
                        {
                            int affectedRows = command.ExecuteNonQuery();
                            response.AffectedRows = affectedRows;
                        }
                    }
                    else
                    {
                        OleDbConnection connection = pooledOdbcConnection.OleDbConnection;

                        using (OleDbCommand command = new OleDbCommand(request.Query, connection))
                        {
                            int affectedRows = command.ExecuteNonQuery();
                            response.AffectedRows = affectedRows;
                        }
                    }

                    response.Success = true;
                    response.ConnectionAutoCloseTime = pooledOdbcConnection.ConnectionAutoCloseTime;
                }
                catch (OdbcException x)
                {
                    response.Error = OdbcConnectionPool.GetOdbcError(x);
                }
                catch (Exception x)
                {
                    response.Error = x.ToString();
                }
            }

            return response;
        }

        public StatusResponse Status()
        {
            StatusResponse response = new StatusResponse();
            response.Now = DateTime.Now;
            response.ActiveConnections = OdbcConnectionPool.Instance.GetActiveConnectionsCount();
            return response;
        }

        #endregion

        private object getColumnValue(DbDataReader reader, int index)
        {
            object result = null;

            if (!reader.IsDBNull(index))
            {
                result = reader.GetValue(index);
            }

            return result;
        }

        private SchemaTable getSchemaTable(DbDataReader reader)
        {
            SchemaTable schemaTable = new SchemaTable();
            
            DataTable resultSchema = reader.GetSchemaTable();
            foreach (DataRow row in resultSchema.Rows)
            {
                SchemaColumn schemaColumn = new SchemaColumn();
                schemaTable.Add(schemaColumn);

                schemaColumn.ColumnName = getDataRowColumn(row, "ColumnName") as string;
                schemaColumn.ColumnOrdinal = Convert.ToInt32(getDataRowColumn(row, "ColumnOrdinal"));
                schemaColumn.ColumnSize = Convert.ToInt32(getDataRowColumn(row, "ColumnSize"));
                schemaColumn.NumericPrecision = Convert.ToInt32(getDataRowColumn(row, "NumericPrecision"));
                schemaColumn.NumericScale = Convert.ToInt32(getDataRowColumn(row, "NumericScale"));
                schemaColumn.ProviderType = Convert.ToInt32(getDataRowColumn(row, "ProviderType"));
                schemaColumn.IsLong = (getDataRowColumn(row, "IsLong") as bool?).GetValueOrDefault();
                schemaColumn.AllowDBNull = (getDataRowColumn(row, "AllowDBNull") as bool?).GetValueOrDefault();
                schemaColumn.IsReadOnly = (getDataRowColumn(row, "IsReadOnly") as bool?).GetValueOrDefault();
                schemaColumn.IsRowVersion = (getDataRowColumn(row, "IsRowVersion") as bool?).GetValueOrDefault();
                schemaColumn.IsUnique = (getDataRowColumn(row, "IsUnique") as bool?).GetValueOrDefault();
                schemaColumn.IsKey = (getDataRowColumn(row, "IsKey") as bool?).GetValueOrDefault();
                schemaColumn.IsAutoIncrement = (getDataRowColumn(row, "IsAutoIncrement") as bool?).GetValueOrDefault();
                schemaColumn.BaseTableName = getDataRowColumn(row, "BaseTableName") as string;
                schemaColumn.BaseColumnName = getDataRowColumn(row, "BaseColumnName") as string;

                ///////////////////////////
                object obj = getDataRowColumn(row, "DataType");
                Type t = Type.GetType(obj.ToString());
                XmlTypeMapping xmlMapping = getQualifiedNameForSystemType(t);
                schemaColumn.DataType = xmlMapping.XsdTypeName;

                /*
                Console.WriteLine("-------------------");
                Console.WriteLine(xmlMapping.ElementName);
                Console.WriteLine(xmlMapping.Namespace);
                Console.WriteLine(xmlMapping.TypeFullName);
                Console.WriteLine(xmlMapping.TypeName);
                Console.WriteLine(xmlMapping.XsdElementName);
                Console.WriteLine(xmlMapping.XsdTypeName);
                Console.WriteLine(xmlMapping.XsdTypeNamespace);
                */

                /*
                XmlSerializer xs = new XmlSerializer(t);
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    xs.Serialize(writer, obj);
                    writer.Close();
                }

                //Xmltype
                Console.WriteLine(sb.ToString());
                */
            }

            return schemaTable;
        }

        private object getDataRowColumn(DataRow row, string columnName)
        {
            object obj = null;

            try
            {
                obj = row[columnName];
            }
            catch { }

            return obj;
        }

        private XmlTypeMapping getQualifiedNameForSystemType(Type systemType)
        {
            SoapReflectionImporter sri = new SoapReflectionImporter();
            return sri.ImportTypeMapping(systemType);
        }

        private void writeXmlMapping()
        {
            // http://stackoverflow.com/questions/6767550/get-an-xml-datatype-from-a-net-type
            var mapping = (from XmlTypeCode cc in Enum.GetValues(typeof(XmlTypeCode))
                           let xt = XmlSchemaType.GetBuiltInSimpleType(cc)
                           where xt != null
                           group cc by xt.Datatype.ValueType into gg
                           select new { Type = gg.Key, XmlTypeCodes = gg.ToArray() })
                          .ToDictionary(m => m.Type, m => m.XmlTypeCodes);

            foreach (var key in mapping.Keys)
            {
                var val = mapping[key];
                Console.WriteLine(key + "=>");
                foreach (var it in val)
                {
                    var n = System.Enum.GetName(typeof(XmlTypeCode), it);
                    Console.WriteLine(n);
                }
            }
        }

        private static bool isFloat(Type type)
        {
            return (type == typeof(Int64) ||
                    type == typeof(UInt64) ||
                    type == typeof(Single) ||
                    type == typeof(Double) ||
                    type == typeof(Decimal));
        }

        private static bool isInt(Type type)
        {
            return (type == typeof(Byte) ||
                    type == typeof(SByte) ||
                    type == typeof(Int16) ||
                    type == typeof(UInt16) ||
                    type == typeof(Int32) ||
                    type == typeof(UInt32));
        }
    }
}
