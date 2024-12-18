﻿using System.Net;
using Xunit;
using NATS.Client;
using CacheCow.Client.Headers;
using CacheCow.Common;

namespace CacheCow.Client.NatsKeyValueCacheStore.Tests
{
    public class IntegrationTests(NatsFixture fixture) : IClassFixture<NatsFixture>
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

        [SkippableFact]
        public async void AddItemTest()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var httpResponseMessage2 = await client.GetAsync(CacheableResource1);
            Assert.True(httpResponseMessage2.Headers.GetCacheCowHeader().RetrievedFromCache.Value);
        }

        [SkippableFact]
        public async void ExceptionTest()
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

        [SkippableFact]
        public async void GetValue()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler()
            });

            var httpResponseMessage = await client.GetAsync(CacheableResource1);
            var response = redisStore.GetValueAsync(new CacheKey(CacheableResource1, new string[0])).Result;
            Assert.NotNull(response);
        }

        [SkippableFact]
        public void TestConnectivity()
        {
            var redisStore = GetKeyValueStore();
            HttpResponseMessage responseMessage = null;
            Console.WriteLine(redisStore.GetValueAsync(new CacheKey("http://google.com", new string[0])).Result);
        }

        [SkippableFact]
        public void WorksWithMaxAgeZeroAndStillStoresIt()
        {
            var redisStore = GetKeyValueStore();
            var client = new HttpClient(new CachingHandler(redisStore)
            {
                InnerHandler = new HttpClientHandler(),
                DefaultVaryHeaders = new string[0]
            });

            var httpResponseMessage = client.GetAsync(MaxAgeZeroResource).Result;
            var key = new CacheKey(MaxAgeZeroResource, new string[0]);
            var response = redisStore.GetValueAsync(key).Result;
            Assert.NotNull(response);
        }
    }
}


