using BankApi.Channels;
using BankApi.Hubs;
using BankInfrastructure.Data;
using BankInfrastructure.Interfaces;
using BankInfrastructure.Repositories;
using BankServices.Interfaces;
using BankServices.Services;
using Microsoft.EntityFrameworkCore;

// Bank API application-ийн startup тохиргоо.
// Энд database, service, background worker, SignalR, Swagger, CORS болон endpoint mapping-ууд бүртгэгдэнэ.
var builder = WebApplication.CreateBuilder(args);

// SQLite database context-ийг connection string-ээр бүртгэнэ.
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Queue domain-ийн repository болон service dependency injection-д бүртгэгдэнэ.
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IQueueService, QueueService>();

// Queue дуудах хүсэлтүүдийг channel-оор worker руу дамжуулна.
builder.Services.AddSingleton<QueueChannelService>();
builder.Services.AddHostedService<QueueWorker>();

// Дансны transaction хүсэлтүүдийг channel-оор worker руу дамжуулна.
builder.Services.AddSingleton<TransactionChannelService>();
builder.Services.AddHostedService<TransactionWorker>();

// Controller API болон Swagger documentation-ийг идэвхжүүлнэ.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Валютын ханш realtime шинэчлэгдэхэд SignalR ашиглана.
builder.Services.AddSignalR();

// API-г сүлжээний бусад төхөөрөмжөөс 5092 port дээр хандах боломжтой болгоно.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5092);
});

// Teller app болон Blazor display-ээс API болон SignalR руу хандах CORS зөвшөөрөл.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7057",
                "http://localhost:7057",
                "http://localhost:5200",
                "https://localhost:7200",
                "http://192.168.88.6:5092",
                "http://192.168.88.6:5084"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Development болон local туршилтад API endpoint-уудыг Swagger дээр харуулна.
app.UseSwagger();
app.UseSwaggerUI();

// Дээр бүртгэсэн CORS policy-г pipeline-д ашиглана.
app.UseCors("AllowBlazor");

// Controller endpoint-уудыг map хийнэ.
app.MapControllers();

// Валютын realtime hub endpoint.
app.MapHub<CurrencyHub>("/currencyHub");

app.Run();
