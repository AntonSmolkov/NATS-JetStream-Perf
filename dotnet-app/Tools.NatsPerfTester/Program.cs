using Prometheus;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Tools.NatsPerfTester;
using Tools.NatsPerfTester.Consumer;
using Tools.NatsPerfTester.Producer;

var builder = WebApplication.CreateBuilder(args);
var mainOptions = new MainOptions();
builder.Configuration.Bind(mainOptions);
builder.Services.Configure<MainOptions>(builder.Configuration);

var logFormatter  =  new ExpressionTemplate("[{UtcDateTime(@t):dd-MM-yyyy HH:mm:ss.fff} {#if @l='Verbose'}Trace{#else if @l='Fatal'}Critical{#else}{@l}{#end} ({SourceContext})] {@m}\n{@x}", theme: TemplateTheme.Code);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(writeTo => writeTo.Console(restrictedToMinimumLevel: mainOptions.LogLevel, formatter: logFormatter), bufferSize: 10_000, blockWhenFull: false)
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddMetrics();
builder.Services.AddHealthChecks();

if (mainOptions.NatsProducersEnabled)
    builder.Services.AddHostedService<ProducerWorker>();

if (mainOptions.NatsConsumersEnabled)
    builder.Services.AddHostedService<ConsumerWorker>();

var app = builder.Build();
app.UseMetricServer();
app.MapHealthChecks("/health/liveness");
app.MapHealthChecks("/health/readiness");
app.Run();
