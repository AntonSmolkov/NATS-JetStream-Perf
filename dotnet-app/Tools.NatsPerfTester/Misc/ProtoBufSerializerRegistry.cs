using NATS.Client.Core;

namespace Tools.NatsPerfTester.Misc;

public class NatsProtoBufSerializerRegistry : INatsSerializerRegistry
{
    public INatsSerialize<T> GetSerializer<T>() => NatsProtoBufSerializer<T>.Default;

    public INatsDeserialize<T> GetDeserializer<T>() => NatsProtoBufSerializer<T>.Default;
}