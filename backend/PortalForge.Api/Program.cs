using Hangfire;
using Hangfire.PostgreSql;
using PortalForge.Api.Middleware;
using PortalForge.Application;
using PortalForge.Infrastructure;
using PortalForge.Infrastructure.BackgroundJobs;
using PortalForge.Infrastructure.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// EventLog is Windows-only, removed for Linux/Docker compatibility

// Add services to the container.

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Hangfire with PostgreSQL storage
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(connectionString)));

// Add Hangfire server
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register seeders
builder.Services.AddScoped<DefaultRequestTemplatesSeeder>();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                  "https://krablab.pl",
                  "http://krablab.pl",
                  "http://localhost:3000",
                  "http://localhost:3001",
                  "https://83.168.107.39",
                  "http://83.168.107.39")
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

// Seed default request templates
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DefaultRequestTemplatesSeeder>();
    await seeder.SeedAsync();
}

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

// Configure Hangfire Dashboard (accessible at /hangfire)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
    DashboardTitle = "PortalForge Background Jobs"
});

// Register recurring jobs with cron schedules
RecurringJob.AddOrUpdate<UpdateVacationAllowancesJob>(
    "update-vacation-allowances",
    job => job.ExecuteAsync(),
    "0 0 1 1 *",  // January 1st, 00:00 (UTC)
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<ExpireCarriedOverVacationJob>(
    "expire-carried-over-vacation",
    job => job.ExecuteAsync(),
    "59 23 30 9 *",  // September 30th, 23:59 (UTC)
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<SendCarriedOverVacationRemindersJob>(
    "send-vacation-reminders",
    job => job.ExecuteAsync(),
    "0 0 1 9 *",  // September 1st, 00:00 (UTC)
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<CheckApprovalDeadlinesJob>(
    "check-approval-deadlines",
    job => job.ExecuteAsync(),
    "0 9 * * *",  // Daily at 9:00 AM (UTC)
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<UpdateVacationStatusesJob>(
    "update-vacation-statuses",
    job => job.ExecuteAsync(default),
    "1 0 * * *",  // Daily at 00:01 AM (UTC)
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<SendVacationRemindersJob>(
    "send-vacation-reminders-daily",
    job => job.ExecuteAsync(),
    "0 8 * * *",  // Daily at 8:00 AM (UTC) - before work hours
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
