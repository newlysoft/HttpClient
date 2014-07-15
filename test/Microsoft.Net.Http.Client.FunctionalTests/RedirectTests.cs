using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Net.Http.Client.FunctionalTests
{
    public class RedirectTests
    {
        [Fact]
        public async Task RedirectWithRedirectsDisabled_ReturnsRedirectResponse()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler() { RedirectMode = RedirectMode.None }))
                {
                    var requestTask = client.GetAsync(baseAddress);

                    var serverContext = await acceptTask;
                    serverContext.Response.Redirect("/Foo");
                    serverContext.Response.Close();

                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                        Assert.Equal("Found", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.True(response.Headers.Contains("Location"));
                        Assert.Equal("/Foo", response.Headers.GetValues("Location").First());
                        Assert.Equal(0, response.Content.Headers.ContentLength);
                        Assert.NotNull(response.Content);
                        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        [Fact]
        public async Task Redirect_Redirected()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler()))
                {
                    var requestTask = client.GetAsync(baseAddress);

                    var serverContext = await acceptTask;
                    Assert.Equal("/", serverContext.Request.Url.AbsolutePath);
                    serverContext.Response.Redirect("/Foo");
                    serverContext.Response.Close();

                    serverContext = await listener.GetContextAsync();
                    Assert.Equal("/Foo", serverContext.Request.Url.AbsolutePath);
                    serverContext.Response.Close();

                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("OK", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.Equal(0, response.Content.Headers.ContentLength);
                        Assert.NotNull(response.Content);
                        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        [Fact]
        public async Task RedirectLimitHit_RedirectsStop()
        {
            string baseAddress;
            using (var listener = Utilities.CreateServer(out baseAddress))
            {
                var acceptTask = listener.GetContextAsync();

                using (HttpClient client = new HttpClient(new ManagedHandler() { MaxAutomaticRedirects = 2 }))
                {
                    var requestTask = client.GetAsync(baseAddress);

                    var serverContext = await acceptTask;
                    Assert.Equal("/", serverContext.Request.Url.AbsolutePath);
                    serverContext.Response.Redirect("/1");
                    serverContext.Response.Close();

                    serverContext = await listener.GetContextAsync();
                    Assert.Equal("/1", serverContext.Request.Url.AbsolutePath);
                    serverContext.Response.Redirect("/2");
                    serverContext.Response.Close();

                    serverContext = await listener.GetContextAsync();
                    Assert.Equal("/2", serverContext.Request.Url.AbsolutePath);
                    serverContext.Response.Redirect("/3");
                    serverContext.Response.Close();

                    using (var response = await requestTask)
                    {
                        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
                        Assert.Equal("Found", response.ReasonPhrase);
                        Assert.Equal(new Version(1, 1), response.Version);
                        Assert.True(response.Headers.Contains("Location"));
                        Assert.Equal("/3", response.Headers.GetValues("Location").First());
                        Assert.Equal(0, response.Content.Headers.ContentLength);
                        Assert.NotNull(response.Content);
                        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }
    }
}