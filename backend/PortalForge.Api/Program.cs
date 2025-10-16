using PortalForge.Api.Middleware;
using PortalForge.Application;
using PortalForge.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://krablab.pl", "http://krablab.pl", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure forwarded headers for reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Use custom error handling middleware (must be first)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Use forwarded headers (must be before other middleware)
app.UseForwardedHeaders();

// Configure PathBase if running behind reverse proxy at /portalforge/be
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for MVP development
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Swagger endpoint relative to current path
    options.SwaggerEndpoint("v1/swagger.json", "PortalForge API v1");
    options.RoutePrefix = "swagger"; // Access at /portalforge/be/swagger
});

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

app.Run();
