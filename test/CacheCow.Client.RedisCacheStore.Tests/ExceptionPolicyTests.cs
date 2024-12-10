using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CacheCow.Common;
using Xunit;

namespace CacheCow.Client.RedisCacheStore.Tests
{
    public class ExceptionPolicyTests
    {
        [Fact]
        public async Task IfNotThrowThenDoesNot()
        {
            var s = new RedisStore("NoneExisting", throwExceptions: false);
            var k = new CacheKey("https://google.com/", new string[0]);
            var r = new HttpResponseMessage(HttpStatusCode.Accepted);
            await s.AddOrUpdateAsync(k, r);
            var r2 = await s.GetValueAsync(k);
            var removed = await s.TryRemoveAsync(k);
            Assert.Null(r2);
            Assert.False(removed);
        }
    }
}
