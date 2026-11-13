using FixItAI.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using FixItAI.Api.Services;
using FixItAI.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);
NeonConfiguration.LoadLocalEnvironment(builder);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDataProtection();
builder.Services.AddHttpClient<DiagnosticService>(client => { client.Timeout = TimeSpan.FromSeconds(45); });
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("diagnostics", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(NeonConfiguration.DatabaseConnection(builder.Configuration)));
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseExceptionHandler(handler => handler.Run(async context => {
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred. Please try again." });
}));
app.UseCors("Frontend");
app.UseRateLimiter();
app.MapControllers();
app.Run();
