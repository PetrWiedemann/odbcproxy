using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.pdynet.odbcproxy;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace net.pdnyet.odbcproxy
{
    [ServiceContract]
    public interface IOdbcProxyService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/connect",
            Method = "POST",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        [IncludeXmlDeclaration]
        ConnectResponse Connect(ConnectRequest request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/close",
            Method = "POST",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        [IncludeXmlDeclaration]
        CloseResponse Close(CloseRequest request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/select",
            Method = "POST",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        [IncludeXmlDeclaration]
        SelectResponse Select(SelectRequest request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/execute",
            Method = "POST",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        [IncludeXmlDeclaration]
        ExecuteResponse ExecuteCommand(ExecuteRequest request);
    }
}
