using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace net.pdynet.odbcproxy
{
    [DataContract(Namespace = "")]
    public class ExecuteRequest
    {
        [DataMember]
        public string ConnectionID { get; set; }

        [DataMember]
        public string Query { get; set; }
    }
}
