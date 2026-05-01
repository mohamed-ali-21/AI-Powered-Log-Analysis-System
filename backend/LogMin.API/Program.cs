using LogMin.Infrastructure.Abstractions.Intelligence;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Application.Services;
using LogMin.Infrastructure.ExternalServices;
using LogMin.Infrastructure.Persistence;
using LogMin.Infrastructure.Repositories;
using LogMin.Worker.BackgroundServices;
using LogMin.Worker.Configuration;
using LogMin.Worker.LogIntelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("LogMindDb")
    ?? throw new InvalidOperationException("Connection string 'LogMindDb' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure();
        sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    }));

builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IIssueAnalysisRepository, IssueAnalysisRepository>();

builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();
builder.Services.AddScoped<IIssueGroupingService, IssueGroupingService>();
builder.Services.AddScoped<IIssueAiAnalysisOrchestrator, IssueAiAnalysisOrchestrator>();
builder.Services.AddScoped<ILogQueryService, LogQueryService>();
builder.Services.AddScoped<IIssueQueryService, IssueQueryService>();
builder.Services.AddScoped<IIssueAnalysisQueryService, IssueAnalysisQueryService>();

builder.Services.Configure<LogProcessingOptions>(
    builder.Configuration.GetSection(LogProcessingOptions.SectionName));
builder.Services.Configure<IssueAnalysisOptions>(
    builder.Configuration.GetSection(IssueAnalysisOptions.SectionName));

builder.Services.AddSingleton<LogIntelligenceWorker>();
builder.Services.AddScoped<ILogAnalyzer, LogIntelligenceAnalyzer>();

builder.Services.AddSingleton(sp =>
{
    var opts = sp.GetRequiredService<IOptions<IssueAnalysisOptions>>().Value;
    var agent = opts.GetDefaultAgent();
    return new IssueAnalysisAgentClientSettings
    {
        AppName = agent.AppName,
        UserId = agent.UserId,
        ListAppsPath = agent.ListAppsPath,
        RunPath = agent.RunPath,
        MaxRetries = agent.MaxRetries
    };
});

builder.Services.AddHttpClient<IIssueAnalysisAgentClient, IssueAnalysisAgentClient>((sp, client) =>
{
    var agent = sp.GetRequiredService<IOptions<IssueAnalysisOptions>>().Value.GetDefaultAgent();
    client.BaseAddress = new Uri(agent.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(agent.RequestTimeoutSeconds);
});

builder.Services.AddHealthChecks();

builder.Services.AddHostedService<LogProcessingBackgroundService>();
builder.Services.AddHostedService<IssueAnalysisBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
