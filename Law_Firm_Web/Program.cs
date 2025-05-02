using DATA.Repositories.Appointment_repo;
using DATA.Repositories.Client_repo;
using DATA.Repositories.Document_repo;
using DATA.Repositories.Lawyer_repo;
using DATA.Repositories.LegalCase_repo;
using DataAccess.Data;
using Law_Model.Models;
using Law_Model.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Law_Model.Static_file.Static_datas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();



builder.Services.AddDbContext<AplicationDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add Scoped Services
builder.Services.AddScoped<IClient_Service, Client_Service>();
builder.Services.AddScoped<ILawyer_Service, Lawyer_Service>();
builder.Services.AddScoped<ILegalCase_Service, LegalCase_Service>(); // Add Scoped Services<ILegalCase_Service>
builder.Services.AddScoped<IDocument_Service, Document_Service>(); // Add Scoped Services<ILawyer_Service>
builder.Services.AddScoped<IAppointment_Service, Appointment_Service>(); // Add Scoped Services<IClient_Service>



// Configure Identity with enum-based Role
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 10;


})
.AddEntityFrameworkStores<AplicationDB>()
.AddDefaultTokenProviders().AddDefaultUI();




// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy("LawerOnly", policy => policy.RequireRole(UserRole.Lawyer.ToString()));
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
app.MapRazorPages();

// Area route should come first
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route comes after
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
