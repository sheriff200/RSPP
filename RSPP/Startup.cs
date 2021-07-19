
//using BHP.Controllers.RequestProposal;
using RSPP.Helpers;
using RSPP.Models.DB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Rotativa.AspNetCore;
using System;

namespace RSPP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });




            //services.AddTransient<IHostedService, RequestService>();
            //services.AddTransient<IHostedService, RequestReminderService>();

            services.AddDistributedMemoryCache();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/AccessDenied");

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();



            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);//You can set Time   
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //ElpsServices._elpsAppEmail = Configuration.GetSection("ElpsKeys").GetSection("elpsAppEmail").Value.ToString();
            //ElpsServices._elpsBaseUrl = Configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString();
            //ElpsServices.public_key = Configuration.GetSection("ElpsKeys").GetSection("PK").Value.ToString();
            //ElpsServices._elpsAppKey = Configuration.GetSection("ElpsKeys").GetSection("elpsSecretKey").Value.ToString();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDbContext<RSPPdbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("RSPPConnectionString")));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/AccessDenied");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Hosting.IHostingEnvironment env2)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapRazorPages(
                //    //name: "default",
                //    pattern: "{controller=Company}/{action=ApplicationForm}/{id?}"
                //    );
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Company}/{action=ApplicationForm}/{id?}");
            });


            RotativaConfiguration.Setup(env2);
        }
    }
}

