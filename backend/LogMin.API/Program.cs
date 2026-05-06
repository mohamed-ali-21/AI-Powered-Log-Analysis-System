using LogMin.Application.Services;
using LogMin.Application.Services.Abstruction;
using LogMin.Infrastructure.Abstractions.Intelligence;
using LogMin.Infrastructure.Abstractions.Persistence;
using LogMin.Infrastructure.Persistence;
using LogMin.Infrastructure.Repositories;
using LogMin.Worker.BackgroundServices;
using LogMin.Worker.Configuration;
using LogMin.Worker.Intelligence;
using LogMin.Worker.LogIntelligence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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
builder.Services.AddScoped<IAgentSettingRepository, AgentSettingRepository>();

builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();
builder.Services.AddScoped<IIssueGroupingService, IssueGroupingService>();
builder.Services.AddScoped<IIssueAiAnalysisOrchestrator, IssueAiAnalysisOrchestrator>();
builder.Services.AddScoped<ILogQueryService, LogQueryService>();
builder.Services.AddScoped<IIssueQueryService, IssueQueryService>();
builder.Services.AddScoped<IIssueAnalysisQueryService, IssueAnalysisQueryService>();
builder.Services.AddScoped<IAgentSettingsService, AgentSettingsService>();

builder.Services.Configure<LogProcessingOptions>(
    builder.Configuration.GetSection(LogProcessingOptions.SectionName));
builder.Services.Configure<IssueAnalysisOptions>(
    builder.Configuration.GetSection(IssueAnalysisOptions.SectionName));
builder.Services.Configure<IssueAnalysisAgentOptions>(
    builder.Configuration.GetSection(IssueAnalysisAgentOptions.SectionName));

builder.Services.AddSingleton<LogIntelligenceWorker>();
builder.Services.AddScoped<ILogAnalyzer, LogIntelligenceAnalyzer>();

builder.Services.AddScoped<IIssueAnalysisAgent, IssueAnalysisAgent>();

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
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
