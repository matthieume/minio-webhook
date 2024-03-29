﻿using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using minio_webhook.Services.Minio;
using minio_webhook.Services.Webhooks;

namespace minio_webhook
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddHealthChecks();

            services.AddHangfire(x => x.UseLiteDbStorage());

            services.Configure<MinioSettings>(Configuration.GetSection("MinioSettings"));
            services.AddSingleton<Services.Minio.Minio>();

            // Register all webhooks here
            services.AddSingleton<IWebhook, ThumbnailGeneratorWebHook>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJobManager, Services.Minio.Minio minio)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                WorkerCount = 1
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new Hangfire.Dashboard.IDashboardAuthorizationFilter[] { } // All access from everywere to dashbord (usefull for docker usage)
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHealthChecks("/hc");
            });

            recurringJobManager.AddOrUpdate("CheckBucketRegistration",
                            () => minio.CheckBucketRegistration(),
                            "*/5 * * * *");
        }
    }
}
