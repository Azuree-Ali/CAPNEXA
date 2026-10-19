using CAPNEXAAPI.DataAccess;
using CAPNEXAAPI;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCookiePolicy(new CookiePolicyOptions
{
    // Let the auth handlers set their protocol cookies to SameSite=None when needed.
    MinimumSameSitePolicy = SameSiteMode.Unspecified,
    OnAppendCookie = context =>
    {
        if (context.CookieName.StartsWith(".AspNetCore.Correlation.", StringComparison.OrdinalIgnoreCase))
            context.CookieOptions.IsEssential = true;
    }
});

app.UseCors("Frontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();
