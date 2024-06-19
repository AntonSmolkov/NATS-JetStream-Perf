using Serilog.Events;

namespace Tools.NatsPerfTester;

public class MainOptions
{
    [ConfigurationKeyName("NATS_URL")]
    public string NatsUrl { get; set; } = "nats://127.0.0.1:4222,nats://127.0.0.1:4223,nats://127.0.0.1:4224";
    
    [ConfigurationKeyName("NATS_TLS_SKIP_VERIFY")]
    public bool NatsTlsSkipVerify { get; set; } = true;
    
    [ConfigurationKeyName("NATS_TLS_CA_CERT_PATH")]
    public string? NatsTlsCaCertPath { get; set; }
    
    [ConfigurationKeyName("NATS_USERNAME")]
    public string NatsUserName { get; set; } = "user";
    
    [ConfigurationKeyName("NATS_PASSWORD")]
    public string NatsPassword { get; set; } = "password";
    
    [ConfigurationKeyName("NATS_PRODUCERS_ENABLED")]
    public bool NatsProducersEnabled { get; set; } = false;
    
    [ConfigurationKeyName("NATS_PRODUCERS_PARALLEL")]
    public uint NatsProducersParallel { get; set; } = 8;
    
    [ConfigurationKeyName("NATS_PRODUCERS_USE_DEDICATED_CONNECTIONS")]
    public bool NatsProducersUseDedicatedConnections { get; set; } = false;
    
    [ConfigurationKeyName("NATS_PRODUCERS_BATCH_SIZE")]
    public int NatsProducersBatchSize { get; set; } = 1;
    
    [ConfigurationKeyName("NATS_PRODUCERS_INCLUDE_MESSAGE_ID")]
    public bool NatsProducersIncludeMessageId { get; set; } = true;
   
    [ConfigurationKeyName("NATS_PRODUCERS_USE_CORE_INSTEAD_OF_JETSTREAMS")]
    public bool NatsProducersUseCoreInsteadOfJetStream { get; set; } = false;
    
    [ConfigurationKeyName("NATS_PRODUCERS_USE_SINGLE_SUBJECT")]
    public bool NatsProducersUseSingleSubject { get; set; } = false;
        
    [ConfigurationKeyName("NATS_CONSUMERS_ENABLED")]
    public bool NatsConsumersEnabled { get; set; } = false;
    
    [ConfigurationKeyName("NATS_CONSUMERS_SHARED")]
    public bool NatsConsumersShared { get; set; } = false;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_CREATE")]
    public bool NatsJetStreamsCreate { get; set; } = true;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_USE_IN_MEMORY_STORAGE")]
    public bool NatsJetStreamsUseInMemoryStorage { get; set; } = false;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_COMPRESSION_ENABLED")]
    public bool NatsJetStreamsCompressionEnabled { get; set; } = false;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_REPLICAS_COUNT")]
    public int NatsJetStreamsReplicasCount { get; set; } = 3;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_DEDUP_WINDOW_SEC")]
    public int NatsJetStreamsDedupWindowSec { get; set; } = 300;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_PARTITIONS_COUNT")]
    public uint NatsJetStreamsPartitionsCount { get; set; } = 32;
    
    [ConfigurationKeyName("NATS_JETSTREAMS_AKCS_ENABLED")]
    public bool NatsJetStreamsAcksEnabled { get; set; } = true;
    
    [ConfigurationKeyName("LOG_LEVEL")]
    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;
}