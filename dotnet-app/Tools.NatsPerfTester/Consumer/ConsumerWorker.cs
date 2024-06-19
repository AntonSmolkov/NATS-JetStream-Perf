using System.Diagnostics;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Prometheus;
using Tools.NatsPerfTester.Misc;
using Tools.NatsPerfTester.Dtos;

namespace Tools.NatsPerfTester.Consumer;

public class ConsumerWorker : BackgroundService, IAsyncDisposable
{
    private readonly ILogger<ConsumerWorker> _logger;
    private readonly NatsConnection _natsConnection;

    private readonly MainOptions _mainOptions;
    private readonly NatsJSContext _jsContext;
    
    private readonly Stopwatch _sw;
    private readonly Counter _consumersReceivedTotal;

    public ConsumerWorker(IOptions<MainOptions> mainOptions, ILoggerFactory loggerFactory, ILogger<ConsumerWorker> logger)
    {
        _logger = logger;
        _mainOptions = mainOptions.Value;
        var opts = new NatsOpts
        {
            Url = _mainOptions.NatsUrl,
            LoggerFactory = loggerFactory,
            SerializerRegistry = new NatsProtoBufSerializerRegistry(),
            Name = "NATS UDL consumer",
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
        
        _natsConnection = new NatsConnection(opts);
        _jsContext = new NatsJSContext(_natsConnection);
        _sw = new Stopwatch();
        
        _consumersReceivedTotal = Metrics.CreateCounter(
            "udl_lt_consumers_js_received_total",
            "Total number of received and acknowledged messages from JetStreams",
            labelNames: ["consumerIndex"]);
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _sw.Start();
        for (var i = 0; i < _mainOptions.NatsJetStreamsPartitionsCount; i++)
        {
            var consumerIndex = i;
            _ = Task.Run(() => StartConsumerAsync(consumerIndex, stoppingToken), stoppingToken);
        }

        _ = Task.Run(() => StartReportingAsync(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private async Task StartConsumerAsync(int consumerIndex, CancellationToken cancellationToken)
    {
        var consumerName = _mainOptions.NatsConsumersShared ? 
            $"udl-consumer-{consumerIndex}": 
            $"udl-consumer-{Guid.NewGuid().ToString("N")[..5]}-{Environment.MachineName}-{consumerIndex}";
        
        try
        {
            var consumerCreateRetryInterval = TimeSpan.FromSeconds(5);
            
            INatsJSConsumer consumer = null!;
            var consumerCreated = false;
            
            while (!cancellationToken.IsCancellationRequested && !consumerCreated)
            {
                try
                {
                    // new ConsumerConfig must be created for each retry otherwise unrelated error occurs (wrong durable name) 
                    var consumerConfig = new ConsumerConfig()
                    {
                        DurableName = consumerName,
                        Name = consumerName,
                        AckPolicy = ConsumerConfigAckPolicy.Explicit,
                        MaxAckPending = 1,
                        AckWait = TimeSpan.FromMinutes(10),
                        InactiveThreshold = TimeSpan.FromHours(3),
                        FilterSubject = ">"
                    };
                    consumer = await _jsContext.CreateOrUpdateConsumerAsync($"udl-js-partition-{consumerIndex}", consumerConfig, cancellationToken);
                }
                catch (NatsJSApiException e) when (e.Error.Code == 404)
                {
                    _logger.LogWarning("Can't create consumer {ConsumerName}. The stream for this consumer is not created yet. Going to retry in {RetryInterval}", consumerName, consumerCreateRetryInterval);
                    await Task.Delay(consumerCreateRetryInterval, cancellationToken);
                    continue;
                }
                consumerCreated = true;
            }
            
        
            _logger.LogInformation("Consumer {ConsumerName} created", consumerName);
            
            await foreach (var msg in consumer.ConsumeAsync(NatsProtoBufSerializer<UdlValue>.Default, null, cancellationToken))
            {
                _consumersReceivedTotal.WithLabels([$"{consumerIndex}"]).Inc();
                await msg.AckAsync(null, cancellationToken);
            }
        }
        catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Consumer {ConsumerName}. An error has occured while consuming messages", consumerName);
        }
        
        _logger.LogInformation("Consumer {ConsumerName} finished", consumerName);
    }

    private async Task StartReportingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
            
            if (_sw.ElapsedMilliseconds == 0)
                continue;
           
            var elapsedSeconds = _sw.ElapsedMilliseconds / 1000;
            
            var consumedTotal = _consumersReceivedTotal
                .GetAllLabelValues()
                .Select(labels => _consumersReceivedTotal.WithLabels(labels).Value).Sum();
            
            var consumedPerSecond = consumedTotal / elapsedSeconds;
            _logger.LogInformation("Consuming rate: {ConsumedPerSecond} values per second. (Elapsed sec: {ElapsedSeconds}. Consumed: {ConsumedTotal})", consumedPerSecond, elapsedSeconds, consumedTotal);
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _natsConnection.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}