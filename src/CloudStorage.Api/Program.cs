using CloudStorage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CloudStorageDbContext>(options =>
{
   var connectionString = builder.Configuration.GetConnectionString("CloudStorage");

   options.UseNpgsql(connectionString);
});

var app = builder.Build();


