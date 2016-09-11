using System.ServiceModel;
using System.ServiceModel.Web;

namespace net.pdynet.odbcproxy
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

        [OperationContract]
        [WebGet(UriTemplate = "/status",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        [IncludeXmlDeclaration]
        StatusResponse Status();
    }
}
