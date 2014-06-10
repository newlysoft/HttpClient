using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Net.Security;

namespace Microsoft.Net.Http.Client
{
    public class ManagedHandler : HttpMessageHandler
    {
        public ManagedHandler()
        {

        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            cancellationToken.ThrowIfCancellationRequested();

            ProcessUrl(request);
            ProcessHostHeader(request);
            request.Headers.ConnectionClose = true; // TODO: Connection re-use is not supported.

            if (request.Method != HttpMethod.Get)
            {
                throw new NotImplementedException(request.Method.Method);
            }

            ApmStream transport = null;
            TcpClient client = new TcpClient();
            try
            {
                await client.ConnectAsync(request.GetHostProperty(), request.GetPortProperty().Value);
                transport = new ApmStreamWrapper(client.GetStream());

                if (string.Equals("https", request.GetSchemeProperty(), StringComparison.OrdinalIgnoreCase))
                {
                    SslStream sslStream = new SslStream(transport);
                    await sslStream.AuthenticateAsClientAsync(request.GetHostProperty());
                    transport = sslStream;
                }
            }
            catch (SocketException sox)
            {
                ((IDisposable)client).Dispose();
                throw new HttpRequestException("Request failed", sox);
            }

            var bufferedReadStream = new BufferedReadStream(transport);
            var connection = new HttpConnection(bufferedReadStream);
            return await connection.SendAsync(request, cancellationToken);
        }

        // Data comes from either the request.RequestUri or from the request.Properties
        private void ProcessUrl(HttpRequestMessage request)
        {
            string scheme = request.GetSchemeProperty();
            if (string.IsNullOrWhiteSpace(scheme))
            {
                if (!request.RequestUri.IsAbsoluteUri)
                {
                    throw new InvalidOperationException("Missing URL Scheme");
                }
                scheme = request.RequestUri.Scheme;
                request.SetSchemeProperty(scheme);
            }
            if (!(scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                  || scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Only HTTP or HTTPS are supported, not: " + request.RequestUri.Scheme);
            }

            string host = request.GetHostProperty();
            if (string.IsNullOrWhiteSpace(host))
            {
                if (!request.RequestUri.IsAbsoluteUri)
                {
                    throw new InvalidOperationException("Missing URL Scheme");
                }
                host = request.RequestUri.DnsSafeHost;
                request.SetHostProperty(host);
            }

            int? port = request.GetPortProperty();
            if (!port.HasValue)
            {
                if (!request.RequestUri.IsAbsoluteUri)
                {
                    throw new InvalidOperationException("Missing URL Scheme");
                }
                port = request.RequestUri.Port;
                request.SetPortProperty(port);
            }

            string pathAndQuery = request.GetPathAndQueryProperty();
            if (string.IsNullOrWhiteSpace(pathAndQuery))
            {
                if (request.RequestUri.IsAbsoluteUri)
                {
                    pathAndQuery = request.RequestUri.PathAndQuery;
                }
                else
                {
                    pathAndQuery = "/" + request.RequestUri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
                }
                request.SetPathAndQueryProperty(pathAndQuery);
            }
        }

        private void ProcessHostHeader(HttpRequestMessage request)
        {
            if (string.IsNullOrWhiteSpace(request.Headers.Host))
            {
                string host = request.GetHostProperty();
                int port = request.GetPortProperty().Value;
                if (host.Contains(':'))
                {
                    // IPv6
                    host = '[' + host + ']';
                }

                request.Headers.Host = host + ":" + port.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
