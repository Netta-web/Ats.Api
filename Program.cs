<<<<<<< HEAD
using Ats.Api.Data;
using Ats.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"CONNECTION STRING: {conn}");
builder.Services.AddControllers();

builder.Services.AddScoped<IJobsService, JobsService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
=======
using System.Text.Json.Serialization;
using Ats.Api.Data;
using Ats.Api.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var dbConn = new NpgsqlConnectionStringBuilder
{
    Host     = builder.Configuration["Database:Host"],
    Port     = int.Parse(builder.Configuration["Database:Port"] ?? "5432"),
    Database = builder.Configuration["Database:Name"],
    Username = builder.Configuration["Database:Username"],
    Password = builder.Configuration["Database:Password"]
};

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IJobsService, JobsService>();
builder.Services.AddScoped<IApplicationsService, ApplicationsService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConn.ToString()));
>>>>>>> 170b562 (Business logic check? Here we go!!!)

builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration["Redis:ConnectionString"]);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();

<<<<<<< HEAD
app.UseHttpsRedirection();
=======
>>>>>>> 170b562 (Business logic check? Here we go!!!)
app.MapControllers();

app.Run();
