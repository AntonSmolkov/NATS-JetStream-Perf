using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Tools.NatsPerfTester.Producer.Publishers;

public interface INatsPublisher
{
    public ValueTask PublishAsync<T>(string subject,
        T? data,
        NatsHeaders? headers = default,
        string? replyTo = default,
        INatsSerialize<T>? serializer = default,
        NatsJSPubOpts? opts = default,
        CancellationToken cancellationToken = default);
}