using BankApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<QueueService>();

var app = builder.Build();

// Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

Queue<string> queue = new Queue<string>();
int counter = 1;

app.MapPost("/ticket", () =>
{
    var number = $"A{counter++:000}";
    queue.Enqueue(number);

    return Results.Ok(number);
});

app.MapGet("/next", () =>
{
    if (queue.Count == 0)
        return Results.Ok("EMPTY");

    return Results.Ok(queue.Dequeue());
});

app.Run();
app.Run();