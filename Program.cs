using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using LabelVerify.Web.Options;
using LabelVerify.Web.Rules;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Compliance;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.Services.OCR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<IOcrService, AzureVisionOcrService>();
builder.Services.AddScoped<ILabelRule, BrandNameRule>();
builder.Services.AddScoped<LabelVerificationService>();
builder.Services.AddScoped<ILabelRule, GovernmentWarningRule>();
builder.Services.AddScoped<ILabelRule, AlcoholContentRule>();
builder.Services.AddScoped<ILabelRule, NetContentsRule>();
builder.Services.AddScoped<ILabelRule, ClassTypeRule>();
builder.Services.AddScoped<LabelFactExtractionService>();
builder.Services.AddScoped<LabelComparisonService>();
builder.Services.AddScoped<IColaPackageIngestionService, ColaPackageIngestionService>();
builder.Services.AddScoped<ApplicationFieldExtractionService>();
builder.Services.AddScoped<AzureDocumentOcrService>();
builder.Services.AddScoped<ColaPackageComparisonService>();
builder.Services.AddScoped<ComplianceReportService>();
builder.Services.AddScoped<PdfAuditReportGenerator>();
builder.Services.AddScoped<ReviewHistoryService>();
builder.Services.AddScoped<ReviewQueryService>();
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddScoped<ComplianceSummaryService>();
builder.Services.AddScoped<ReviewAuditLogService>();
builder.Services.AddScoped<DashboardAnalyticsService>();
builder.Services.AddScoped<AzureOpenAiSummaryService>();
builder.Services.AddScoped<MonthlyComplianceReportService>();
builder.Services.AddScoped<MonthlyCompliancePdfGenerator>();
builder.Services.AddScoped<RiskScoringService>();
builder.Services.AddScoped<ReviewBatchService>();
builder.Services.AddScoped<ColaReviewOrchestrator>();
builder.Services.AddScoped<AiReviewEnrichmentService>();
builder.Services.AddScoped<MyWorkQueueService>();
builder.Services.AddScoped<GovernmentWarningValidator>();
builder.Services.AddScoped<AlcoholContentValidator>();
builder.Services.AddScoped<SulfitesValidator>();
builder.Services.AddScoped<RecommendationEngine>();
builder.Services.AddScoped<ReviewerCommentaryService>();
builder.Services.AddScoped<GovernmentWarningLayoutValidator>();

builder.Services.Configure<AzureVisionOptions>(builder.Configuration.GetSection("AzureVision"));
builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));
builder.Services.Configure<AzureDocumentIntelligenceOptions>(builder.Configuration.GetSection("AzureDocumentIntelligence"));
builder.Services.Configure<AzureBlobStorageOptions>(options =>
{
    options.ConnectionString = builder.Configuration["AzureBlobStorageConnectionString"]
        ?? builder.Configuration["AzureBlobStorage:ConnectionString"] ?? string.Empty;
}); 
builder.Services.Configure<AzureOpenAiOptions>(builder.Configuration.GetSection("AzureOpenAI"));

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<AzureOpenAiSummaryService>(); 
builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    }
);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(120);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));
builder.Services.AddDefaultIdentity<LabelVerify.Web.Models.ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Reviews");
    options.Conventions.AuthorizePage("/ColaReview");
}); 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await db.Database.CanConnectAsync();
    }
    catch
    {
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization(); 
app.MapStaticAssets();
app.MapGet("/", context =>
{
    context.Response.Redirect("/Reviews/Dashboard");

    return Task.CompletedTask;
});
app.MapRazorPages().WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeed.SeedAsync(scope.ServiceProvider);
}

app.Run();