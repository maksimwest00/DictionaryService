using DictionaryService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DictionaryServiceDbContext>(sp => new DictionaryServiceDbContext(
    builder.Configuration.GetConnectionString("DictionaryServiceDb")!));

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DictionaryService");
    });
}

app.MapControllers();

app.Run();