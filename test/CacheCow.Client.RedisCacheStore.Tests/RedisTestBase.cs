using System;
using Testcontainers.Redis;
using Testcontainers.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace CacheCow.Client.RedisCacheStore.Tests;

public class RedisFixture(IMessageSink messageSink) : ContainerFixture<RedisBuilder, RedisContainer>(messageSink)
{
    protected override RedisBuilder Configure(RedisBuilder builder) => builder.WithImage("redis:7.0");
}

public abstract class RedisTestBase(RedisFixture fixture) : IClassFixture<RedisFixture>
{
    protected string ConnectionString
    {
        get
        {
            try
            {
                return fixture.Container.GetConnectionString();
            }
            catch (ArgumentException e) when (e.ParamName == "DockerEndpointAuthConfig")
            {
                throw new SkipException(e.Message, e);
            }
        }
    }
}
