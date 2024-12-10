using System.Net;
using Xunit;
using NATS.Client;
using CacheCow.Client.Headers;
using CacheCow.Common;
using Xunit.Abstractions;

namespace CacheCow.Client.NatsKeyValueCacheStore.Tests
{
    public class IntegrationTests(NatsFixture fixture, ITestOutputHelper testOutput) : IClassFixture<NatsFixture>
    {

        private const string CacheableResource1 = "https://ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js";
        private const string CacheableResource2 = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.2.min.js";
        private const string MaxAgeZeroResource = "https://google.com/";
        private const string BucketName = "vavavoom";

        private NatsKeyValueStore GetKeyValueStore()
        {
            var options = ConnectionFactory.GetDefaultOptions();
            try
            {
                options.Url = fixture.Container.GetConnectionString();
            }
            catch (ArgumentException e) when (e.ParamName == "DockerEndpointAuthConfig")
            {
                throw new SkipException(e.Message, e);
            }
            return new NatsKeyValueStore(BucketName, options);
        }

        [SkippableFact(typeof(HttpRequestException))]
        public async Task AddItemTest()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var httpResponseMessage2 = await client.GetAsync(CacheableResource1);
            Assert.True(httpResponseMessage2.Headers.GetCacheCowHeader().RetrievedFromCache);
        }

        [SkippableFact(typeof(HttpRequestException))]
        public async Task ExceptionTest()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var httpResponseMessage2 = await client.GetAsync(CacheableResource1);
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage2.StatusCode);
        }

        [SkippableFact(typeof(HttpRequestException))]
        public async Task GetValue()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var response = await redisStore.GetValueAsync(new CacheKey(CacheableResource1, new string[0]));
            Assert.NotNull(response);
        }

        [SkippableFact]
        public async Task TestConnectivity()
        {
            var redisStore = GetKeyValueStore();
            HttpResponseMessage responseMessage = null;
            var httpResponseMessage = await redisStore.GetValueAsync(new CacheKey("http://google.com", new string[0]));
            testOutput.WriteLine(httpResponseMessage?.ToString() ?? "");
        }

        [SkippableFact(typeof(HttpRequestException))]
        public async Task WorksWithMaxAgeZeroAndStillStoresIt()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler(),
                DefaultVaryHeaders = new string[0]
            });

            var httpResponseMessage = await client.GetAsync(MaxAgeZeroResource);
            var key = new CacheKey(MaxAgeZeroResource, new string[0]);
            var response = await redisStore.GetValueAsync(key);
            Assert.NotNull(response);
        }
    }
}


