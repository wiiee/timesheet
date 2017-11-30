namespace Web
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Platform.Context;
    using Service;
    using Service.Media;
    using Service.Other;
    using Service.Project;
    using Service.Scrum;
    using Service.User;
    using Web.Context;
    using Web.Extension.Middleware;
    using Microsoft.AspNetCore.Authentication.Cookies;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Platform.Setting.Setting.Instance.Init(Configuration);
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o => {
                o.LoginPath = new PathString("/Account/Login");
                o.LogoutPath = new PathString("/Account/LogOff");
                o.AccessDeniedPath = new PathString("/Home");
                o.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            services.AddSingleton<IContextRepository, WebContextRepository>();
            services.AddSingleton<ImgService>();
            services.AddSingleton<FeedbackService>();
            services.AddSingleton<ProjectService>();
            services.AddSingleton<TaskTemplateService>();
            services.AddSingleton<TimeSheetService>();
            services.AddSingleton<SprintService>();
            services.AddSingleton<DepartmentService>();
            services.AddSingleton<ProfileService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<UserTimeSheetStatusService>();
        }

        private void InitServiceFactory(IApplicationBuilder app)
        {
            var services = new Dictionary<string, IService>();

            AddService(services, app.ApplicationServices.GetService<ImgService>());
            AddService(services, app.ApplicationServices.GetService<FeedbackService>());
            AddService(services, app.ApplicationServices.GetService<ProjectService>());
            AddService(services, app.ApplicationServices.GetService<TaskTemplateService>());
            AddService(services, app.ApplicationServices.GetService<TimeSheetService>());
            AddService(services, app.ApplicationServices.GetService<SprintService>());
            AddService(services, app.ApplicationServices.GetService<DepartmentService>());
            AddService(services, app.ApplicationServices.GetService<ProfileService>());
            AddService(services, app.ApplicationServices.GetService<UserService>());
            AddService(services, app.ApplicationServices.GetService<UserTimeSheetStatusService>());

            ServiceFactory.Instance.Init(services);
        }

        private void AddService<T>(Dictionary<string, IService> services, T service)
            where T : IService
        {
            if (services != null)
            {
                services.Add(typeof(T).FullName, service);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            InitServiceFactory(app);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseResponseCaching();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSession();
            app.UseContext();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
