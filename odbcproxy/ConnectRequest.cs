using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace net.pdynet.odbcproxy
{
    [DataContract(Namespace = "")]
    public class ConnectRequest
    {
        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public bool UsingOleDb { get; set; }
    }
}
