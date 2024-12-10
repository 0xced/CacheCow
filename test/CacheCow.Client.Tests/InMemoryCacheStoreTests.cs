using CacheCow.Client.Tests.Helper;
using CacheCow.Common;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CacheCow.Client.Tests
{

    public class InMemoryCacheStoreTests
    {
        private const string DummyUrl = "http://myserver/api/dummy";

        [Fact]
        public async Task CanStore()
        {
            var store = new InMemoryCacheStore();
            await store.AddOrUpdateAsync(new CacheKey(DummyUrl, new string[0]),
                ResponseHelper.GetOkMessage());

            var r = await store.GetValueAsync(new CacheKey(DummyUrl, new string[0]));
            Assert.NotNull(r);
        }
    }
}
