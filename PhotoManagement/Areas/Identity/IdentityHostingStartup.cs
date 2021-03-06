using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoManagement.Areas.Identity.Data;
using PhotoManagement.Data;

[assembly: HostingStartup(typeof(PhotoManagement.Areas.Identity.IdentityHostingStartup))]
namespace PhotoManagement.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<PhotoManagementContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("PhotoManagementContextConnection")));

                services.AddDefaultIdentity<PhotoManagementUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddEntityFrameworkStores<PhotoManagementContext>();
            });
        }
    }
}