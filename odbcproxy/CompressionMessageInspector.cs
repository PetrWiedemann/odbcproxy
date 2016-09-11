﻿using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;

namespace net.pdynet.odbcproxy
{
    public class CompressionEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CompressionMessageInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public class CompressionMessageInspector : IDispatchMessageInspector
    {
        //static readonly Regex jsonContentTypes = new Regex(@"[application|text]\/json");
        //static readonly Regex xmlContentTypes = new Regex(@"[application|text]\/xml");

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            bool shouldCompressResponse = false;

            object propObj;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out propObj))
            {
                var prop = (HttpRequestMessageProperty)propObj;
                /*
                var accept = prop.Headers[HttpRequestHeader.Accept];
                if (accept != null)
                {
                    if (jsonContentTypes.IsMatch(accept))
                    {
                        WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
                    }
                    else if (xmlContentTypes.IsMatch(accept))
                    {
                        WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Xml;
                    }
                }
                */
                var acceptEncoding = prop.Headers[HttpRequestHeader.AcceptEncoding];
                if (acceptEncoding != null)
                {
                    shouldCompressResponse = acceptEncoding
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToLower())
                        .Contains("gzip");
                }
            }

            return shouldCompressResponse;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var useGzip = (bool)correlationState;
            
            if (useGzip)
            {
                // Add property to be used by encoder
                HttpResponseMessageProperty resp;
                object respObj;

                if (!reply.Properties.TryGetValue(HttpResponseMessageProperty.Name, out respObj))
                {
                    resp = new HttpResponseMessageProperty();
                    reply.Properties.Add(HttpResponseMessageProperty.Name, resp);
                }
                else
                {
                    resp = (HttpResponseMessageProperty)respObj;
                }

                resp.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            }
        }
    }
}
