using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace net.pdynet.odbcproxy
{
    [DataContract(Namespace = "")]
    public class CloseResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Error { get; set; }
    }
}
