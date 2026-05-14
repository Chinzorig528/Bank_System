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

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();