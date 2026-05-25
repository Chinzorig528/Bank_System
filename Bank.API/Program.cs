using BankApi.Channels;
using BankApi.Hubs;
using BankInfrastructure.Data;
using BankInfrastructure.Interfaces;
using BankInfrastructure.Repositories;
using BankServices.Interfaces;
using BankServices.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IQueueService, QueueService>();

builder.Services.AddSingleton<QueueChannelService>();
builder.Services.AddHostedService<QueueWorker>();

builder.Services.AddSingleton<TransactionChannelService>();
builder.Services.AddHostedService<TransactionWorker>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7057",
                "http://localhost:7057",
                "http://localhost:5200",
                "https://localhost:7200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowBlazor");

app.MapControllers();

app.MapHub<CurrencyHub>("/currencyHub");

app.Run();