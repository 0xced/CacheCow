using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CacheCow.Client.Headers;
using CacheCow.Common;
using Xunit;
using Xunit.Abstractions;

namespace CacheCow.Client.RedisCacheStore.Tests
{
    /// <summary>
    /// READ!! ------------------ These tests require Docker running and access to internet
    /// </summary>

    public class IntegrationTests(RedisFixture fixture, ITestOutputHelper testOutput) : RedisTestBase(fixture)
    {
        private const string CacheableResource1 = "https://ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js";
        private const string CacheableResource2 = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.2.min.js";
        private const string MaxAgeZeroResource = "https://google.com/";

        [SkippableFact]
        public async Task AddItemTest()
        {
            var client = new HttpClient(new CachingHandler(new RedisStore(ConnectionString))
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var httpResponseMessage2 = await client.GetAsync(CacheableResource1);
            Assert.True(httpResponseMessage2.Headers.GetCacheCowHeader().RetrievedFromCache);
        }

        [SkippableFact]
        public async Task ExceptionTest()
        {
            var client = new HttpClient(new CachingHandler(new RedisStore(ConnectionString, throwExceptions: false))
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var httpResponseMessage2 = await client.GetAsync(CacheableResource1);
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage2.StatusCode);
        }

        [SkippableFact]
        public async Task GetValue()
        {
            var redisStore = new RedisStore(ConnectionString);
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
            var redisStore = new RedisStore(ConnectionString);
            testOutput.WriteLine((await redisStore.GetValueAsync(new CacheKey("http://google.com", new string[0])))?.ToString() ?? "");
        }

        [SkippableFact]
        public async Task WorksWithMaxAgeZeroAndStillStoresIt()
        {
            var redisStore = new RedisStore(ConnectionString);
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
