
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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RSPP.Controllers;
using RSPP.Job;
using System.Threading.Tasks;
using System.Threading;

namespace RSPP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


       


        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
        public class CheckUserSessionAttribute : ActionFilterAttribute
        {
            //context.HttpContext.Session == null||!context.HttpContext.Session.TryGetValue("_sessionEmail", out byte[] Context.Session.GetString(AccountController.sessionEmail).ToString()
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                var user = context.HttpContext.Session.GetString(AccountController.sessionEmail).ToString();
                if (user == null)
                            
                {
                    context.Result =
                        new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Account",
                            action = "LogOff"
                        }));
                }
                base.OnActionExecuting(context);

                //HttpSessionStateBase session = filterContext.HttpContext.Session;
                //var user = session["User"];

                //if (((user == null) && (!session.IsNewSession)) || (session.IsNewSession))
                //{
                //    //send them off to the login page
                //    var url = new UrlHelper(filterContext.RequestContext);
                //    var loginUrl = url.Content("~/Account/LogOff");
                //    session.RemoveAll();
                //    session.Clear();
                //    session.Abandon();
                //    filterContext.HttpContext.Response.Redirect(loginUrl, true);
                //}
            }
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            //AddHostedService

           services.AddHostedService<PaymentConfirmationService>();

            services.AddDistributedMemoryCache();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/AccessDenied");

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();



            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);//You can set Time   
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                
            });
           
           
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

