using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ECommerceCsvImport.Services;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// чтобы русский текст в HTML не превращался в коды &#x...;
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Товары хранятся в памяти, поэтому singleton.
// С AddScoped репозиторий создавался бы заново на каждый запрос и товары пропадали бы.
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<IProductImportService, ProductImportService>();

var app = builder.Build();

// русский формат чисел (пробел между разрядами, запятая в дробях)
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
