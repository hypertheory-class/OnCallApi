using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.


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