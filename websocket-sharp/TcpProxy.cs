using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;


namespace WebSocketSharp
{
    public class TcpProxy
    {
        //http://www.replicator.org/journal/201011170043-net-connecting-a-tcpclient-through-an-http-proxy-with-authentication
        public static TcpClient connectViaHTTPProxy(string targetHost, int targetPort, string httpProxyHost, int httpProxyPort, string proxyUserName, string proxyPassword)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttp,
                Host = httpProxyHost,
                Port = httpProxyPort
            };

            var proxyUri = uriBuilder.Uri;

            var request = WebRequest.Create(
                "http://" + targetHost + ":" + targetPort + "/websocket");

            var webProxy = new WebProxy(proxyUri);

            request.Proxy = webProxy;
            request.Method = "CONNECT";

            var credentials = new NetworkCredential(
                proxyUserName, proxyPassword);

            webProxy.Credentials = credentials;

            var response = request.GetResponse();

            var responseStream = response.GetResponseStream();
            Debug.Assert(responseStream != null);

            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var rsType = responseStream.GetType();
            var connectionProperty = rsType.GetProperty("Connection", Flags);

            var connection = connectionProperty.GetValue(responseStream, null);
            var connectionType = connection.GetType();
            var networkStreamProperty = connectionType.GetProperty("NetworkStream", Flags);

            var networkStream = networkStreamProperty.GetValue(connection, null);
            var nsType = networkStream.GetType();
            var socketProperty = nsType.GetProperty("Socket", Flags);
            var socket = (Socket)socketProperty.GetValue(networkStream, null);

            return new TcpClient { Client = socket };
        }

    }
}
