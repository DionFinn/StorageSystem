using CloudStorage.Infrastructure.Persistence;
using CloudStorage.Core.Interfaces;
using CloudStorage.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CloudStorageDbContext>(options =>
{
   var connectionString = builder.Configuration.GetConnectionString("CloudStorage");
   if(connectionString == null)
    {
        throw new Exception("CloudStorage Connection String has not been configured");
    } 

   options.UseNpgsql(connectionString);
});

builder.Services.AddControllers();

string? root = builder.Configuration["Storage:RootPath"];

if(string.IsNullOrEmpty(root))
{
    throw new Exception("Storage:RootPath has not been configured in appsettings.json");
}

builder.Services.AddScoped<IFileStorage>(sp => new LocalFileStorage(root));

var app = builder.Build();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CloudStorageDbContext>();

    try
    {
        await db.Database.CanConnectAsync();
        Console.WriteLine("Database Connection Successful");
    } catch (Exception e)
    {
        throw new Exception("Database Connection Failed", e);
    }
}

app.Run();
