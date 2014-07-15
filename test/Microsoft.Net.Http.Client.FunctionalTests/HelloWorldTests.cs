using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Net.Http.Client.FunctionalTests
{
    public class HelloWorldTests
    {
        [Fact]
        public async Task GetWithNoBody()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler()))
                {
                    var requestTask = client.GetAsync(baseAddress);
                    
                    var serverContext = await acceptTask;
                    serverContext.Response.Headers.Add("CustomHeader", "CustomValue");
                    serverContext.Response.Close();
                    
                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("OK", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.True(response.Headers.Contains("CustomHeader"));
                        Assert.Equal("CustomValue", response.Headers.GetValues("CustomHeader").First());
                        Assert.Equal(0, response.Content.Headers.ContentLength);
                        Assert.NotNull(response.Content);
                        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        [Fact]
        public async Task GetWithContentLengthResponseBody()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler()))
                {
                    var requestTask = client.GetAsync(baseAddress);

                    var serverContext = await acceptTask;
                    serverContext.Response.ContentLength64 = 100;
                    serverContext.Response.Close(new byte[100], true);

                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("OK", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.Equal(100, response.Content.Headers.ContentLength);
                        Assert.NotNull(response.Content);
                        Assert.Equal(new byte[100], await response.Content.ReadAsByteArrayAsync());
                    }
                }
            }
        }
        
        [Fact]
        public async Task GetWithEmptyChunkedResponseBody()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler()))
                {
                    var requestTask = client.GetAsync(baseAddress);

                    var serverContext = await acceptTask;
                    serverContext.Response.SendChunked = true;
                    serverContext.Response.Close();

                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("OK", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.True(response.Headers.TransferEncodingChunked.HasValue);
                        Assert.True(response.Headers.TransferEncodingChunked.Value);
                        Assert.False(response.Content.Headers.Contains("Content-Length"));
                        Assert.NotNull(response.Content);
                        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        [Fact]
        public async Task GetWithChunkedResponseBody()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler()))
                {
                    var requestTask = client.GetAsync(baseAddress);

                    var serverContext = await acceptTask;
                    serverContext.Response.OutputStream.Write(new byte[10], 0, 10);
                    serverContext.Response.OutputStream.Write(new byte[10], 0, 10);
                    serverContext.Response.Close();

                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("OK", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.True(response.Headers.TransferEncodingChunked.HasValue);
                        Assert.True(response.Headers.TransferEncodingChunked.Value);
                        Assert.False(response.Content.Headers.Contains("Content-Length"));
                        Assert.NotNull(response.Content);
                        Assert.Equal(new byte[20], await response.Content.ReadAsByteArrayAsync());
                    }
                }
            }
        }
    }
}
