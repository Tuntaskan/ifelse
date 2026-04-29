using ifelse.Data;
using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//    .AddNegotiate();

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = options.DefaultPolicy;
//});

builder.Services.AddRazorPages();


// DATABASE CONNECTION
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();


// WAJIB untuk Authentication
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();