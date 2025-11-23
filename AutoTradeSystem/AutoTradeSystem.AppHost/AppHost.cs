var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQConnection = builder.AddConnectionString("my-rabbit");

builder.AddProject<Projects.AutoTradeSystem>("autotradesystem")
    .WithReference(rabbitMQConnection);

builder.Build().Run();
