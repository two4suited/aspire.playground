var builder = DistributedApplication.CreateBuilder(args);

// Add TypeSpec API project - runs once to generate API specs then stops
var typespecApi = builder.AddJavaScriptApp("typespec-api", "../typespec-api", "build");

// Add Azure Cosmos DB with Linux-based preview emulator and Data Explorer
#pragma warning disable ASPIRECOSMOSDB001
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataExplorer();
    });
#pragma warning restore ASPIRECOSMOSDB001

// Add database and container matching TypeSpec Store model
var db = cosmos.AddCosmosDatabase("storedb");
var stores = db.AddContainer("stores", "/id");

// Add seeder as an executable that runs once after the database is ready
var seeder = builder.AddExecutable(
    name: "seed-database",
    command: "dotnet",
    workingDirectory: "../AspireApp.DataSeeder",
    args: ["run"])
    .WithReference(cosmos)
    .WaitFor(cosmos);

builder.Build().Run();
