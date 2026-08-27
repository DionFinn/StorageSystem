using CloudStorage.Infrastructure.Persistence;
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CloudStorageDbContext>();

    

    try
    {
        await db.Database.CanConnectAsync();
        Console.WriteLine("Database Connection Successful");
    } catch (Exception e)
    {
        Console.WriteLine(e);
    }

}
