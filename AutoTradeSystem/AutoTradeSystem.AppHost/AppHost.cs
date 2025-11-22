var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AutoTradeSystem>("autotradesystem");

builder.Build().Run();
