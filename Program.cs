using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataToolKit.Data;
using DataToolKit.Areas.Identity.Data;


public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DataToolKitDbContextConnection") ?? throw new InvalidOperationException("Connection string 'DataToolKitDbContextConnection' not found.");

        builder.Services.AddDbContext<DataToolKitDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        builder.Services.AddDefaultIdentity<DataToolKitUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<DataToolKitDbContext>();

        // Add services to the container.
        builder.Services
            .AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication(); ;

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();

    }
}



