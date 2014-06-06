using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Net.Http.Client
{
    public static class RequestExtensions
    {
        public static string GetSchemeProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.Scheme");
        }

        public static void SetSchemeProperty(this HttpRequestMessage request, string scheme)
        {
            request.SetProperty("url.Scheme", scheme);
        }

        public static string GetHostProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.Host");
        }

        public static void SetHostProperty(this HttpRequestMessage request, string host)
        {
            request.SetProperty("url.Host", host);
        }

        public static int? GetPortProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<int?>("url.Port");
        }

        public static void SetPortProperty(this HttpRequestMessage request, int? port)
        {
            request.SetProperty("url.Port", port);
        }

        public static string GetPathAndQueryProperty(this HttpRequestMessage request)
        {
            return request.GetProperty<string>("url.PathAndQuery");
        }

        public static void SetPathAndQueryProperty(this HttpRequestMessage request, string pathAndQuery)
        {
            request.SetProperty("url.PathAndQuery", pathAndQuery);
        }

        public static T GetProperty<T>(this HttpRequestMessage request, string key)
        {
            object obj;
            if (request.Properties.TryGetValue(key, out obj))
            {
                return (T)obj;
            }
            return default(T);
        }

        public static void SetProperty<T>(this HttpRequestMessage request, string key, T value)
        {
            request.Properties[key] = value;
        }
    }
}
