using LabelVerify.Web.Rules;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.Services.OCR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IOcrService, MockOcrService>();
builder.Services.AddScoped<ILabelRule, BrandNameRule>();
builder.Services.AddScoped<LabelVerificationService>();
builder.Services.AddScoped<ILabelRule, GovernmentWarningRule>();
builder.Services.AddScoped<ILabelRule, AlcoholContentRule>();
builder.Services.AddScoped<ILabelRule, NetContentsRule>();
builder.Services.AddScoped<ILabelRule, ClassTypeRule>();

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
app.MapRazorPages()
   .WithStaticAssets();
app.Run();