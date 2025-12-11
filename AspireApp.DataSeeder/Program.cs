using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var host = builder.Build();

// Get Cosmos DB connection from configuration
var connectionString = builder.Configuration.GetConnectionString("cosmos");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: Cosmos DB connection string not found.");
    return;
}

Console.WriteLine("Starting database seeding...");

using var cosmosClient = new CosmosClient(connectionString);

// Get the database and container
var database = cosmosClient.GetDatabase("storedb");
var container = database.GetContainer("stores");

// Sample store data matching the TypeSpec schema
var stores = new[]
{
    new
    {
        id = "store-001",
        name = "Downtown Tech Store",
        address = new
        {
            street = "123 Main Street",
            city = "Seattle",
            state = "WA",
            zip = "98101"
        }
    },
    new
    {
        id = "store-002",
        name = "Northgate Electronics",
        address = new
        {
            street = "456 North Ave",
            city = "Portland",
            state = "OR",
            zip = "97201"
        }
    },
    new
    {
        id = "store-003",
        name = "Eastside Gadgets",
        address = new
        {
            street = "789 East Boulevard",
            city = "San Francisco",
            state = "CA",
            zip = "94102"
        }
    },
    new
    {
        id = "store-004",
        name = "Westfield Tech Hub",
        address = new
        {
            street = "321 West Road",
            city = "Austin",
            state = "TX",
            zip = "73301"
        }
    },
    new
    {
        id = "store-005",
        name = "Central Computing",
        address = new
        {
            street = "555 Central Square",
            city = "Denver",
            state = "CO",
            zip = "80201"
        }
    }
};

// Seed each store
foreach (var store in stores)
{
    try
    {
        await container.UpsertItemAsync(store, new PartitionKey(store.id));
        Console.WriteLine($"✓ Seeded store: {store.name} (ID: {store.id})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Failed to seed store {store.id}: {ex.Message}");
    }
}

Console.WriteLine($"\nSeeding complete! Added {stores.Length} stores.");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
