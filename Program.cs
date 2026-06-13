using LabelVerify.Web.Rules;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.Services.OCR;
using LabelVerify.Web.Options;
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

builder.Services.Configure<AzureVisionOptions>(builder.Configuration.GetSection("AzureVision"));
builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));
builder.Services.Configure<AzureDocumentIntelligenceOptions>(builder.Configuration.GetSection("AzureDocumentIntelligence"));

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();