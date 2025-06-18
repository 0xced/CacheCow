using Testcontainers.Nats;
using Testcontainers.Xunit;
using Xunit.Abstractions;

namespace CacheCow.Client.NatsKeyValueCacheStore.Tests;

public class NatsFixture(IMessageSink messageSink) : ContainerFixture<NatsBuilder, NatsContainer>(messageSink)
{
    protected override NatsBuilder Configure(NatsBuilder builder) => builder.WithImage("nats:2.9");
}
