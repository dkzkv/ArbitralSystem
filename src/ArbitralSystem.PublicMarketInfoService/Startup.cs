using System;
using System.Reflection;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.PublicMarketInfoService.Common.Auth;
using ArbitralSystem.PublicMarketInfoService.Extensions;
using ArbitralSystem.PublicMarketInfoService.Persistence;
using ArbitralSystem.PublicMarketInfoService.Jobs;
using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace ArbitralSystem.PublicMarketInfoService
{
    internal class AvailableExchangesProvider
    {
        private readonly Exchange[] _exchanges;
        public AvailableExchangesProvider(Exchange[] exchanges)
        {
            _exchanges = exchanges;
        }
        
        public Exchange[] Get()
        {
            return _exchanges;
        }
    }
    
    [UsedImplicitly]
    internal class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private const string DomainLayer = "ArbitralSystem.PublicMarketInfoService.Domain";
        private const string PersistenceLayer = "ArbitralSystem.PublicMarketInfoService.Persistence";

        public Startup(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(config =>
                {
                    config.EnableEndpointRouting = false;
                    config.RespectBrowserAcceptHeader = true;
                })
                .AddApiExplorer()
                .AddCors()
                .AddControllersAsServices()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly( Assembly.GetExecutingAssembly() ));;

            services.AddAutoMapper(Assembly.GetExecutingAssembly(),
                AppDomain.CurrentDomain.Load(DomainLayer),
                AppDomain.CurrentDomain.Load(PersistenceLayer));
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
            
            services.AddControllers();
            services.AddVersionedApiExplorer();
            services.AddArbitralSystemDbContext(_configuration[SettingsNames.DatabaseConnection]);
            services.AddArbitralSystemSwagger("Public market-info service");
            services.AddHangfire(configuration => configuration
                .UseSqlServerStorage(_configuration[SettingsNames.DatabaseConnection]));

            services.AddArbitralSystemServices(_configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {new HangFireAuthorizationFilter()}
            });
            app.UseHangfireServer();
            
            if(_configuration[SettingsNames.PairInfosCron] is var pairInfoCron &&  !string.IsNullOrEmpty(pairInfoCron))
                RecurringJob.AddOrUpdate<PairInfoUpdaterJob>("Pair-info-update", x => x.Execute(), pairInfoCron);
            
            if(_configuration[SettingsNames.PairPricesCron] is var pairPriceCron &&  !string.IsNullOrEmpty(pairPriceCron))
                RecurringJob.AddOrUpdate<PairPricesJob>("Pair-prices-save", x => x.Execute(), pairPriceCron);
            
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                });
            
            app.UseSerilogRequestLogging();
            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseMvc();

            MigrateDbContext(app);
        }

        private static void MigrateDbContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<PublicMarketInfoBdContext>())
                {
                    context.Database.Migrate();
                    context.SaveChanges();
                }
            }
        }
    }
}