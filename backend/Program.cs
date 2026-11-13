using FixItAI.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using FixItAI.Api.Services;
using FixItAI.Api.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

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
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
var googleConfigured = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);
if (googleConfigured) builder.Services.AddAuthentication().AddGoogle(options => {
    options.ClientId = googleClientId!;
    options.ClientSecret = googleClientSecret!;
    options.SignInScheme = IdentityConstants.ExternalScheme;
});
builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseExceptionHandler(handler => handler.Run(async context => {
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred. Please try again." });
}));
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/auth/providers", () => Results.Ok(new { google = googleConfigured }));
app.MapGet("/api/auth/google", (SignInManager<IdentityUser> signIn, string? origin) => {
    if (!googleConfigured) return Results.Problem("Google sign-in is not configured yet.", statusCode: 503);
    var configuredOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";
    var isLocal = app.Environment.IsDevelopment() && Uri.TryCreate(origin, UriKind.Absolute, out var u) && u.Scheme == "http" && u.Host == "localhost" && u.Port is >= 5173 and <= 5179 && u.AbsolutePath == "/" && string.IsNullOrEmpty(u.Query) && string.IsNullOrEmpty(u.Fragment);
    var returnOrigin = origin == configuredOrigin || isLocal ? origin! : configuredOrigin;
    var properties = signIn.ConfigureExternalAuthenticationProperties("Google", "/api/auth/google/complete");
    properties.Items["frontend_origin"] = returnOrigin;
    return Results.Challenge(properties, ["Google"]);
}).RequireRateLimiting("diagnostics");
app.MapGet("/api/auth/google/complete", async (SignInManager<IdentityUser> signIn, UserManager<IdentityUser> users) => {
    var info = await signIn.GetExternalLoginInfoAsync();
    if (info is null) return Results.Unauthorized();
    var origin = info.AuthenticationProperties is not null && info.AuthenticationProperties.Items.TryGetValue("frontend_origin", out var value) ? value ?? "http://localhost:5173" : "http://localhost:5173";
    var existing = await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
    if (existing is null) {
        var user = new IdentityUser { UserName = "google-" + info.ProviderKey, Email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value };
        var created = await users.CreateAsync(user);
        if (!created.Succeeded) return Results.Problem("Unable to create an account.", statusCode: 500);
        var linked = await users.AddLoginAsync(user, info);
        if (!linked.Succeeded) { await users.DeleteAsync(user); return Results.Problem("Unable to complete sign-in.", statusCode: 500); }
    }
    var result = await signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
    return result.Succeeded ? Results.Redirect(origin + "/history") : Results.Unauthorized();
}).RequireRateLimiting("diagnostics");
app.MapGet("/api/auth/me", async (System.Security.Claims.ClaimsPrincipal principal, UserManager<IdentityUser> users) => { var user = await users.GetUserAsync(principal); return user is null ? Results.Unauthorized() : Results.Ok(new { email = user.Email ?? "Google account" }); }).RequireAuthorization();
app.MapPost("/api/auth/logout", async (HttpContext context) => { await context.SignOutAsync(IdentityConstants.ApplicationScheme); return Results.NoContent(); }).RequireAuthorization();
app.MapControllers();
app.Run();
