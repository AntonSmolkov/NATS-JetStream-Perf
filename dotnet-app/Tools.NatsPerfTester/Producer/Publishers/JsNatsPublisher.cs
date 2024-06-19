using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Tools.NatsPerfTester.Producer.Publishers;

public class JsNatsPublisher(INatsJSContext jsContext) : INatsPublisher
{
    public async ValueTask PublishAsync<T>(
        string subject,
        T? data,
        NatsHeaders? headers = default,
        string? replyTo = default,
        INatsSerialize<T>? serializer = default,
        NatsJSPubOpts? opts = default,
        CancellationToken cancellationToken = default) => 
        await jsContext.PublishAsync(subject, data: data, opts: opts, serializer: serializer, cancellationToken: cancellationToken);
}