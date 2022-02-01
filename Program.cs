using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var readinessCheck = new ReadinessHealthCheck(builder.Environment.ContentRootPath);
// Add services to the container.
builder.Services.AddHealthChecks()
    .AddCheck<BasicHealthCheck>("liveness")
    .AddCheck<BasicHealthCheck>("startup")
    .AddCheck("readiness", readinessCheck);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "liveness"
});

app.UseHealthChecks("/healthz/startup", new HealthCheckOptions
{
    Predicate = check => check.Name == "startup"
});

app.UseHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "readiness"
});



app.MapGet("/", async (IWebHostEnvironment env) =>
{

    var file = Path.Combine(env.ContentRootPath, "Data", "schedule.json");

    var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

    using var reader = new StreamReader(fileStream);
    var response = await JsonSerializer.DeserializeAsync<Dictionary<string, OnCallEntry>>(reader.BaseStream);
    fileStream.Close();

    var addy = Dns.GetHostName();
    app.Logger.LogInformation($"Reading Data from {file} at {addy}");
    return Results.Ok(response);

});

app.Run();

public record OnCallEntry(string name, string phone);


public class BasicHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}

public class ReadinessHealthCheck : IHealthCheck
{
    private readonly string _contentRootPath;

    public ReadinessHealthCheck(string contentRootPath)
    {
        _contentRootPath = contentRootPath;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_contentRootPath, "Data", "schedule.json");
        if(File.Exists(filePath))
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        } else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}