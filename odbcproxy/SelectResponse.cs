using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;

namespace net.pdynet.odbcproxy
{
    [DataContract(Namespace = "")]
    public class SelectResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public DateTime ConnectionAutoCloseTime { get; set; }

        [DataMember]
        public SchemaTable SchemaTable { get; set; }

        [DataMember]
        public List<RowData> Rows { get; set; }
    }

    [CollectionDataContract(Namespace = "",
        Name = "Row",
        ItemName = "Column",
        KeyName = "Header",
        ValueName = "Value")]
    public class RowData : Dictionary<string, object> { }

    [CollectionDataContract(Namespace = "",
        Name = "SchemaTable",
        ItemName = "SchemaColumn")]
    public class SchemaTable : List<SchemaColumn> { }

    [DataContract(Namespace = "")]
    public class SchemaColumn
    {
        [DataMember]
        public string ColumnName { get; set; }

        [DataMember]
        public int? ColumnOrdinal { get; set; }

        [DataMember]
        public int? ColumnSize { get; set; }

        [DataMember]
        public int? NumericPrecision { get; set; }

        [DataMember]
        public int? NumericScale { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public int? ProviderType { get; set; }

        [DataMember]
        public bool IsLong { get; set; }

        [DataMember]
        public bool AllowDBNull { get; set; }

        [DataMember]
        public bool IsReadOnly { get; set; }

        [DataMember]
        public bool IsRowVersion { get; set; }

        [DataMember]
        public bool IsUnique { get; set; }

        [DataMember]
        public bool IsKey { get; set; }

        [DataMember]
        public bool IsAutoIncrement { get; set; }

        [DataMember]
        public string BaseTableName { get; set; }

        [DataMember]
        public string BaseColumnName { get; set; }
    }
}
