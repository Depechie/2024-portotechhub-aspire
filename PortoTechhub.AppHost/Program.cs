var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .PublishAsContainer();

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var todosDbName = "Todos";
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", todosDbName)
    .WithBindMount("../config/postgres", "/docker-entrypoint-initdb.d")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var todosDb = postgres.AddDatabase(todosDbName);

var apiService = builder.AddProject<Projects.PortoTechhub_ApiService>("apiservice")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithReference(todosDb)
    .WaitFor(todosDb);

var workerService = builder.AddProject<Projects.PortoTechhub_WorkerService>("workerservice")
    .WithReference(messaging)
    .WaitFor(messaging);

builder.AddProject<Projects.PortoTechhub_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
