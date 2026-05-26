using CurrencyDisplayBlazor.Components;

// CurrencyDisplayBlazor application-ийн startup тохиргоо.
// Razor component, HttpClient, error handling болон endpoint mapping-ууд энд бүртгэгдэнэ.
var builder = WebApplication.CreateBuilder(args);

// Server-side interactive Razor component service-үүдийг бүртгэнэ.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API дуудлага хийхэд ашиглах HttpClient-ийг scoped lifetime-тай бүртгэнэ.
builder.Services.AddScoped(sp => new HttpClient());

var app = builder.Build();

// Production үед error page болон HSTS хамгаалалтыг идэвхжүүлнэ.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Олдоогүй route-уудыг not-found component руу шилжүүлнэ.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// HTTPS redirect болон antiforgery middleware-үүдийг ашиглана.
app.UseHttpsRedirection();

app.UseAntiforgery();

// Static asset болон үндсэн Razor component endpoint-уудыг map хийнэ.
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
