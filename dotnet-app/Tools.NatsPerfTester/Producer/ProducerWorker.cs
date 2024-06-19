using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Hashing;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Prometheus;
using Tools.NatsPerfTester.Misc;
using Tools.NatsPerfTester.Producer.Publishers;
using Tools.NatsPerfTester.Dtos;

namespace Tools.NatsPerfTester.Producer;

public class ProducerWorker : BackgroundService, IAsyncDisposable 
{
    private readonly ILogger<ProducerWorker> _logger;
    private readonly MainOptions _mainOptions;
    private readonly Stopwatch _sw;
    private readonly Counter _producersPublishedTotal;
    private readonly NatsOpts _natsOpts;
    private readonly NatsConnection _natsSharedConnection;
    private readonly ConcurrentBag<NatsConnection> _natsConnections = new();

    public ProducerWorker(IOptions<MainOptions> mainOptions, ILoggerFactory loggerFactory, ILogger<ProducerWorker> logger)
    {
        _logger = logger;
        _mainOptions = mainOptions.Value;
        _natsOpts = new NatsOpts
        {
            Url = _mainOptions.NatsUrl,
            LoggerFactory = loggerFactory,
            SerializerRegistry = new NatsProtoBufSerializerRegistry(),
            Name = "NATS UDL producer",
            TlsOpts = new NatsTlsOpts()
            {
                InsecureSkipVerify = _mainOptions.NatsTlsSkipVerify,
                CaFile = _mainOptions.NatsTlsSkipVerify ? null : _mainOptions.NatsTlsCaCertPath
            },
            AuthOpts = new NatsAuthOpts()
            {
                Username = _mainOptions.NatsUserName,
                Password = _mainOptions.NatsPassword
            }
        };
        _natsSharedConnection = new NatsConnection(_natsOpts);
        _natsConnections.Add(_natsSharedConnection);
        
        _sw = new Stopwatch();
        
        _producersPublishedTotal = Metrics.CreateCounter(
            "udl_lt_producers_js_published_total",
            "Total number of published messages to JetStreams");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_mainOptions.NatsJetStreamsCreate)
        {
            for (var i = 0; i < _mainOptions.NatsJetStreamsPartitionsCount; i++)
            {
                var streamName = $"udl-js-partition-{i}";
                
                var streamConfig = new StreamConfig(streamName, [$"*.at-least-once.{i}"])
                {
                    Compression = _mainOptions.NatsJetStreamsCompressionEnabled ? StreamConfigCompression.S2 : StreamConfigCompression.None,
                    Storage = _mainOptions.NatsJetStreamsUseInMemoryStorage ? StreamConfigStorage.Memory : StreamConfigStorage.File,
                    NumReplicas = _mainOptions.NatsJetStreamsReplicasCount,
                    DuplicateWindow = TimeSpan.FromSeconds(_mainOptions.NatsJetStreamsDedupWindowSec),
                    NoAck = !_mainOptions.NatsJetStreamsAcksEnabled
                };
                try
                {
                    var jsContext = new NatsJSContext(_natsSharedConnection);
                    await jsContext.CreateStreamAsync(streamConfig, cancellationToken: stoppingToken);
                }
                catch (NatsJSApiException e) when(e.Error.ErrCode == 10058)
                {
                }
                
            }
        }
        _sw.Start();
        for (var i = 0; i < _mainOptions.NatsProducersParallel; i++)
        {
            var producerIndex = i;
            _ = Task.Run(() => GenerateAndPublishMessagesAsync(producerIndex, stoppingToken), stoppingToken);
        }

        _ = Task.Run(() => StartReportingAsync(stoppingToken), stoppingToken);
    }

    private async Task GenerateAndPublishMessagesAsync(int producerIndex, CancellationToken cancellationToken)
    {
        NatsConnection natsConnection;
        if (_mainOptions.NatsProducersUseDedicatedConnections)
        {
            natsConnection = new NatsConnection(_natsOpts);
            _natsConnections.Add(natsConnection);
        }
        else
        {
            natsConnection = _natsSharedConnection;
        }
        
        INatsPublisher natsPublisher = _mainOptions.NatsProducersUseCoreInsteadOfJetStream
            ? new CoreNatsPublisher(natsConnection)
            : new JsNatsPublisher(new NatsJSContext(natsConnection));
        
        _logger.LogInformation("Producer {ProducerIndex} started", producerIndex);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var propertyId = Guid.NewGuid();
                var propertyIdStr = propertyId.ToString();
                string msgSubject;
                if (_mainOptions.NatsProducersUseSingleSubject)
                {
                    var partition = Random.Shared.NextInt64(0, _mainOptions.NatsJetStreamsPartitionsCount);
                    msgSubject = $"single.at-least-once.{partition}";   
                }
                else
                {
                    var partition = Crc32.HashToUInt32(propertyId.ToByteArray()) % _mainOptions.NatsJetStreamsPartitionsCount;
                    msgSubject = $"{propertyId}.at-least-once.{partition}";
                }

                var natsOpts = _mainOptions.NatsProducersIncludeMessageId
                    ? new NatsJSPubOpts() { MsgId = msgSubject }
                    : null;
                
                var udlValue = new UdlValue()
                {
                    PropertyId = propertyIdStr,
                    Value = new TypedValue() { Double = Random.Shared.NextDouble() },
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    QualityType = UdlValueQualityType.Good,
                    QualityStatus = UdlValueQualityStatus.StubStatus1
                };

                if (_mainOptions.NatsProducersBatchSize <= 1)
                {
                    await natsPublisher.PublishAsync(msgSubject,
                        data: udlValue,
                        opts: natsOpts,
                        serializer: NatsProtoBufSerializer<UdlValue>.Default,
                        cancellationToken: cancellationToken);
                    
                    _producersPublishedTotal.Inc();
                }
                else
                {
                    var batchTasks = new List<Task>(_mainOptions.NatsProducersBatchSize);
                    
                    for (var i = 0; i < _mainOptions.NatsProducersBatchSize; i++)
                    {
                        batchTasks.Add(natsPublisher.PublishAsync(msgSubject,
                            data: udlValue,
                            opts: natsOpts,
                            serializer: NatsProtoBufSerializer<UdlValue>.Default,
                            cancellationToken: cancellationToken).AsTask());
                    }
                    
                    await Task.WhenAll(batchTasks);
                    _producersPublishedTotal.Inc(_mainOptions.NatsProducersBatchSize);
                }
            }
            catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
            {
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Producer {ProducerIndex}. An error has occured while generating and publishing message", producerIndex);
            }
        }
        
        _logger.LogInformation("Producer {ProducerIndex} finished", producerIndex);
    }

    private async Task StartReportingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
            
            if (_sw.ElapsedMilliseconds == 0)
                continue;
            
            var elapsedSeconds = _sw.ElapsedMilliseconds / 1000;
            var producedPerSecond = _producersPublishedTotal.Value / elapsedSeconds;
            _logger.LogInformation("Producing rate: {ProducedPerSecond} values per second. (Elapsed sec: {ElapsedSeconds}. Produced: {ProducedTotal})", producedPerSecond, elapsedSeconds, _producersPublishedTotal.Value);
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var natsConnection in _natsConnections)
        {
            await natsConnection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}