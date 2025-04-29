var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PHC_HighCounsel_Bot>("phc-highcounsel-bot");

builder.Build().Run();
