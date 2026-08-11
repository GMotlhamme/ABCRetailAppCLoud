using ABCRetailAppCLoud.Models;
using ABCRetailAppCLoud.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<AzureTableService>();
builder.Services.Configure<AzureBlobStorageConfigs>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.AddScoped<AzureBlobService>();
builder.Services.AddScoped<AzureQueueService>();
//builder.Services.AddScoped<AzurefileService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Order}/{action=Create}/{id?}")
    .WithStaticAssets();


app.Run();
