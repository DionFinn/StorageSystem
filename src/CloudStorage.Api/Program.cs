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
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CloudStorageDbContext>();

    try
    {
        await db.Database.CanConnectAsync();
        Console.WriteLine("Database Connection Successful");
        app.Run();
    } catch (Exception e)
    {
        Console.WriteLine(e);
        Console.WriteLine("Database Connection Failed");
    }

}
