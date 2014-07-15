using System;
using System.Net;

namespace Microsoft.Net.Http.Client.FunctionalTests
{
    public static class Utilities
    {
        private const int BasePort = 5001;
        private const int MaxPort = 10000;
        private static int NextPort = BasePort;
        private static object PortLock = new object();

        /// <summary>
        /// Create a server using a dynamically selected port.
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        public static HttpListener CreateServer(out string baseAddress)
        {
            lock (PortLock)
            {
                while (NextPort < MaxPort)
                {
                    var port = NextPort++;
                    baseAddress = "http://localhost:" + port + "/";
                    var listener = new HttpListener();
                    listener.Prefixes.Add(baseAddress);
                    try
                    {
                        listener.Start();
                        return listener;
                    }
                    catch (HttpListenerException)
                    {
                        listener.Close();
                    }
                }
                NextPort = BasePort;
            }
            throw new Exception("Failed to locate a free port.");
        }
    }
}