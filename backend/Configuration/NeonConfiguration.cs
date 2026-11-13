using Npgsql;

namespace FixItAI.Api.Configuration;
public static class NeonConfiguration
{
    // Neon CLI writes its local credentials in the repository root. Production uses environment variables.
    public static void LoadLocalEnvironment(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment()) return;
        var root = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, ".."));
        var file = Path.Combine(root, ".env.local");
        if (!File.Exists(file)) file = Path.Combine(root, ".env");
        if (!File.Exists(file)) return;
        var allowed = new HashSet<string>(StringComparer.Ordinal) { "DATABASE_URL", "NEON_AI_GATEWAY_BASE_URL", "NEON_AI_GATEWAY_TOKEN" };
        var values = new Dictionary<string, string?>();
        foreach (var line in File.ReadLines(file)) {
            var separator = line.IndexOf('=');
            if (separator < 1) continue;
            var key = line[..separator].Trim();
            if (!allowed.Contains(key) || !string.IsNullOrEmpty(builder.Configuration[key])) continue;
            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2 && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\''))) value = value[1..^1];
            values[key] = value;
        }
        builder.Configuration.AddInMemoryCollection(values);
    }

    public static string DatabaseConnection(IConfiguration config)
    {
        var databaseUrl = config["DATABASE_URL"];
        if (string.IsNullOrWhiteSpace(databaseUrl)) return config.GetConnectionString("FixItAI") ?? "";
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("postgres" or "postgresql") || string.IsNullOrEmpty(uri.Host))
            throw new InvalidOperationException("DATABASE_URL is not a valid PostgreSQL URL.");
        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2) throw new InvalidOperationException("DATABASE_URL must include database credentials.");
        return new NpgsqlConnectionStringBuilder {
            Host = uri.Host, Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]), Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.VerifyFull
        }.ConnectionString;
    }
}
