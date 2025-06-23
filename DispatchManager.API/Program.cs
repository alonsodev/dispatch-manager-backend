using DispatchManager.API.Middleware;
using DispatchManager.Application;
using DispatchManager.Infrastructure;
using DispatchManager.Infrastructure.HealthChecks;
using DispatchManager.Infrastructure.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Dispatch Manager API",
        Version = "v1",
        Description = "API para manejar ordenes de despacho con cálculo de distancia y estimación de costo"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add Application Services
builder.Services.AddApplicationServices();

// Add Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Background Services
builder.Services.AddHostedService<EmailBackgroundService>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("cache");

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatch Manager API V1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

// Initialize Database
try
{
    await app.Services.InitializeDatabaseAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the database");
    throw;
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatch Manager API V1");
        c.RoutePrefix = string.Empty; // Swagger UI en la raíz
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Health Checks endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();