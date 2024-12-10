﻿using System;
using System.Threading.Tasks;
using CacheCow.Client.Headers;
using Xunit;
using Xunit.Abstractions;

namespace CacheCow.Client.FileCacheStore.Tests
{
    /// <summary>
    /// Simple test of caching a request to disk
    /// </summary>
    public class TestFileStore
    {
        private const string CacheableResource1 = "https://ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js";

        private readonly ITestOutputHelper _output;

        public TestFileStore(ITestOutputHelper output)
        {
            _output = output;
        }

        private void log(string s)
        {
            _output.WriteLine(s);
        }


        /// <summary>
        /// Test caching
        /// </summary>
        [Fact]
        public async Task TestDisk()
        {
            var fs = new FileStore("cache");
            await fs.ClearAsync();

            var client = fs.CreateClient();
            Assert.True(fs.IsEmpty());
            log("Querying...");
            var uri = new Uri(CacheableResource1);
            var response = await client.GetAsync(uri);
            Assert.False(fs.IsEmpty());
            var cachedResponse = await client.GetAsync(uri);


            Assert.Contains("did-not-exist=true", response.Headers.GetCacheCowHeader().ToString());
            var cacheHeader = cachedResponse.Headers.GetCacheCowHeader().ToString();
            Assert.Contains("did-not-exist=false;retrieved-from-cache=true", cacheHeader);

            await fs.ClearAsync();
            Assert.True(fs.IsEmpty());
        }

        /// <summary>
        ///
        /// </summary>
        [Fact]
        public async Task TestLoad404()
        {
            var fs = new FileStore("cache");
            await fs.ClearAsync();
            var client = fs.CreateClient();


            var response = await client.GetAsync(new Uri("https://www.openstreetmap.org/non-existin-page"));

            Assert.Equal("NotFound", "" + response.StatusCode);
        }
    }
}
