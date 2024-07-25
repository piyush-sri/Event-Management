using EventQR.EF;
using EventQR.Models.Acc;
using EventQR.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null)
    .AddRazorRuntimeCompilation(); ;
builder.Services.AddSession();


builder.Services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("EventQrDBConStr")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformer>();
builder.Services.AddScoped<IEventOrganizer, EventOrganizer>();
builder.Services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();

builder.Services.AddHttpClient("EventQrClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7098");
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
// app.MapControllerRoute(name: "default",pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapControllerRoute(name: "default",pattern: "{controller=Account}/{action=Login}/{id?}");

 app.MapControllerRoute(name: "autoLogin", pattern: "{controller=Account}/{action=AutoLogin}/{id?}");

app.Run();
