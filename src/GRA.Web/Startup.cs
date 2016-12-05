﻿using AutoMapper;
using GRA.Controllers.RouteConstraint;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GRA.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Configuration[ConfigurationKey.DefaultSiteName] = "The Great Reading Adventure";
            Configuration[ConfigurationKey.DefaultPageTitle] = "Great Reading Adventure";
            Configuration[ConfigurationKey.DefaultSitePath] = "gra";
            Configuration[ConfigurationKey.DefaultFooter] = "This site is running the open source <a href=\"http://www.greatreadingadventure.com/\">Great Reading Adventure</a> software developed by the <a href=\"https://mcldaz.org/\">Maricopa County Library District</a> with support by the <a href=\"http://www.azlibrary.gov/\">Arizona State Library, Archives and Public Records</a>, a division of the Secretary of State, and with federal funds from the <a href=\"http://www.imls.gov/\">Institute of Museum and Library Services</a>.";
            Configuration[ConfigurationKey.InitialAuthorizationCode] = "gra4adminmagic";
            if (env.IsDevelopment())
            {
                Configuration[ConfigurationKey.DefaultCSSqlServer] = DefaultConnectionString.SqlServer;
                Configuration[ConfigurationKey.DefaultCSSQLite] = DefaultConnectionString.SQLite;
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddSession(_ =>
            {
                _.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            services.TryAddSingleton(_ => Configuration);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();
            services.AddMvc();

            services.AddAuthorization(options =>
            {
                foreach (var permisisonName in Enum.GetValues(typeof(Domain.Model.Permission)))
                {
                    options.AddPolicy(permisisonName.ToString(),
                        _ => _.RequireClaim(ClaimType.Permission, permisisonName.ToString()));
                }
            });

            // path validator
            services.AddScoped<Controllers.Base.ISitePathValidator, Controllers.Validator.SitePathValidator>();

            // service facades
            services.AddScoped<Controllers.ServiceFacade.Controller, Controllers.ServiceFacade.Controller>();
            services.AddScoped<Data.ServiceFacade.Repository, Data.ServiceFacade.Repository>();

            // database
            services.AddScoped<Data.Context, Data.SqlServer.SqlServerContext>();
            //services.AddScoped<Data.Context, Data.SQLite.SQLiteContext>();

            // utilities
            services.AddScoped<Domain.Service.Abstract.IUserContextProvider, Controllers.UserContextProvider>();
            services.AddScoped<Security.Abstract.IPasswordHasher, Security.PasswordHasher>();

            // services
            services.AddScoped<Domain.Service.ActivityService, Domain.Service.ActivityService>();
            services.AddScoped<Domain.Service.AuthenticationService, Domain.Service.AuthenticationService>();
            services.AddScoped<Domain.Service.ChallengeService, Domain.Service.ChallengeService>();
            services.AddScoped<Domain.Service.InitialSetupService, Domain.Service.InitialSetupService>();
            services.AddScoped<Domain.Service.MailService, Domain.Service.MailService>();
            services.AddScoped<Domain.Service.SampleDataService, Domain.Service.SampleDataService>();
            services.AddScoped<Domain.Service.SiteLookupService, Domain.Service.SiteLookupService>();
            services.AddScoped<Domain.Service.SiteService, Domain.Service.SiteService>();
            services.AddScoped<Domain.Service.UserService, Domain.Service.UserService>();

            // repositories
            services.AddScoped<Domain.Repository.IAuthorizationCodeRepository, Data.Repository.AuthorizationCodeRepository>();
            services.AddScoped<Domain.Repository.IBookRepository, Data.Repository.BookRepository>();
            services.AddScoped<Domain.Repository.IBranchRepository, Data.Repository.BranchRepository>();
            services.AddScoped<Domain.Repository.IChallengeRepository, Data.Repository.ChallengeRepository>();
            services.AddScoped<Domain.Repository.IChallengeTaskRepository, Data.Repository.ChallengeTaskRepository>();
            services.AddScoped<Domain.Repository.IMailRepository, Data.Repository.MailRepository>();
            services.AddScoped<Domain.Repository.IPointTranslationRepository, Data.Repository.PointTranslationRepository>();
            services.AddScoped<Domain.Repository.IProgramRepository, Data.Repository.ProgramRepository>();
            services.AddScoped<Domain.Repository.IRecoveryTokenRepository, Data.Repository.RecoveryTokenRepository>();
            services.AddScoped<Domain.Repository.IRoleRepository, Data.Repository.RoleRepository>();
            services.AddScoped<Domain.Repository.ISiteRepository, Data.Repository.SiteRepository>();
            services.AddScoped<Domain.Repository.ISystemRepository, Data.Repository.SystemRepository>();
            services.AddScoped<Domain.Repository.IUserLogRepository, Data.Repository.UserLogRepository>();
            services.AddScoped<Domain.Repository.IUserRepository, Data.Repository.UserRepository>();

            services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.ApplicationServices.GetService<Data.Context>().Migrate();

            app.UseStaticFiles();

            app.UseSession();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = Controllers.Authentication.SchemeGRACookie,
                LoginPath = new PathString("/SignIn/"),
                AccessDeniedPath = new PathString("/"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: null,
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: null,
                    template: "{sitePath}/{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new
                    {
                        sitePath = new SiteRouteConstraint(app.ApplicationServices.GetRequiredService<Controllers.Base.ISitePathValidator>())
                    });
                routes.MapRoute(
                    name: null,
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
