using DataAccess.Data;
using Law_Model.Models;
using Law_Model.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Law_Model.Static_file.Static_datas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AplicationDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Configure Identity with enum-based Role
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 10;
    options.Password.RequireNonAlphanumeric  = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AplicationDB>()
.AddDefaultTokenProviders();

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy("LawerlOnly", policy => policy.RequireRole(UserRole.Lawyer.ToString()));
    options.AddPolicy("ClientOnly", policy => policy.RequireRole(UserRole.Client.ToString()));
});



builder.Services.AddRazorPages();
var app = builder.Build();


//we are going to seed the database 
using (var scope = app.Services.CreateScope())
{
    await SeedRolesAndAdmin.Initialize(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Client_Area}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
