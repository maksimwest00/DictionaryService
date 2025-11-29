using DictionaryService.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependincies(builder.Configuration);

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