using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel;
using NDesk.Options;
using System.ServiceProcess;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace net.pdynet.odbcproxy
{
    [System.ComponentModel.DesignerCategory("")]
    class OdbcProxy : ServiceBase
    {
        //http://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml?&page=128
        int port = 47115;

        //WebServiceHost sh = null;
        ServiceHost sh = null;

        static void Main(string[] args)
        {
            OdbcProxy odbcProxy = new OdbcProxy();
            if (Environment.UserInteractive)
            {
                odbcProxy.StartInteractive(args);
            }
            else
            {
                ServiceBase.Run(odbcProxy);
            }
        }

        public OdbcProxy()
        {
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            StartWebService(args);
        }

        protected override void OnStop()
        {
            if (sh != null)
                sh.Close();

            base.OnStop();
        }

        private Binding GetBinding()
        {
            CustomBinding custom = new CustomBinding(new WebHttpBinding());
            for (int i = 0; i < custom.Elements.Count; i++)
            {
                if (custom.Elements[i] is WebMessageEncodingBindingElement)
                {
                    WebMessageEncodingBindingElement webBE = (WebMessageEncodingBindingElement)custom.Elements[i];
                    custom.Elements[i] = new GZipMessageEncodingBindingElement(webBE);
                }
                else if (custom.Elements[i] is TransportBindingElement)
                {
                    ((TransportBindingElement)custom.Elements[i]).MaxReceivedMessageSize = int.MaxValue;
                }
            }

            return custom;
        }

        private void StartWebService(string[] args)
        {
            var options = new OptionSet()
                {
                    {"p|port=", "Bind to port. Default is 47115.", v => Int32.TryParse(v, out port)}
                };

            options.Parse(args);

            UriBuilder ub = new UriBuilder("http://localhost");
            ub.Port = port;
            ub.Path = "odbcproxy";

            //sh = new WebServiceHost(typeof(OdbcProxyService), ub.Uri);
            sh = new ServiceHost(typeof(OdbcProxyService), ub.Uri);

            ServiceEndpoint ep = sh.AddServiceEndpoint(typeof(IOdbcProxyService), GetBinding(), "");
            ep.EndpointBehaviors.Add(new WebHttpBehavior { HelpEnabled = true, AutomaticFormatSelectionEnabled = false });
            ep.EndpointBehaviors.Add(new CompressionEndpointBehavior());

            sh.Open();
        }

        private void StartInteractive(string[] args)
        {
            try
            {
                StartWebService(args);

                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();

                sh.Close();
            }
            catch (CommunicationException cex)
            {
                Console.WriteLine(cex.ToString());
                if (sh != null)
                    sh.Abort();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
        }
    }
}
