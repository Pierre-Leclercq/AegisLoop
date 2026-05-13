using AegisLoop.Infrastructure;
using AegisLoop.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("AegisLoopDb")
    ?? "Data Source=aegisloop.db";
builder.Services.AddInfrastructure(connectionString);

// Worker service d'ingestion
builder.Services.AddHostedService<IngestionWorker>();

// Logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var host = builder.Build();
host.Run();