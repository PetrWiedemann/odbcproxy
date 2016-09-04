using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

namespace net.pdynet.odbcproxy
{
    class XmlDeclarationMessageFormatter : IDispatchMessageFormatter
    {
        private IDispatchMessageFormatter formatter;
        public XmlDeclarationMessageFormatter(IDispatchMessageFormatter formatter)
        {
            this.formatter = formatter;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            formatter.DeserializeRequest(message, parameters);
        }

        public Message SerializeReply(MessageVersion messageVersion, Object[] parameters, Object result)
        {
            var message = formatter.SerializeReply(messageVersion, parameters, result);
            return new XmlDeclarationMessage(message);
        }
    }
}
