using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Tools.NatsPerfTester.Producer.Publishers;

public class CoreNatsPublisher(INatsConnection connection) : INatsPublisher
{
    public ValueTask PublishAsync<T>(
        string subject,
        T? data,
        NatsHeaders? headers = default,
        string? replyTo = default,
        INatsSerialize<T>? serializer = default,
        NatsJSPubOpts? opts = default,
        CancellationToken cancellationToken = default) => 
        connection.PublishAsync(subject, data: data, opts: opts, serializer: serializer, cancellationToken: cancellationToken);
}