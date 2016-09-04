using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace net.pdynet.odbcproxy
{
    class XmlDeclarationMessage : Message
    {
        private Message message;
        public XmlDeclarationMessage(Message message)
        {
            this.message = message;
        }

        public override MessageHeaders Headers
        {
            get { return message.Headers; }
        }

        protected override void OnWriteBodyContents(System.Xml.XmlDictionaryWriter writer)
        {
            // WCF XML serialization doesn't support emitting XML DOCTYPE, you need to roll up your own here.
            writer.WriteStartDocument();
            message.WriteBodyContents(writer);
        }


        public override MessageProperties Properties
        {
            get { return message.Properties; }
        }

        public override MessageVersion Version
        {
            get { return message.Version; }
        } 
    }
}
