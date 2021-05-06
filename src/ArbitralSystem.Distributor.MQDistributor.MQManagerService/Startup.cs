using System;
using System.Reflection;
using ArbitralSystem.Distributor.Core.Services;
using ArbitralSystem.Distributor.MQDistributor.MQDomain;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Interfaces;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.Common;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.Consumers;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.Extensions;
using ArbitralSystem.Distributor.MQDistributor.MQPersistence;
using ArbitralSystem.Distributor.MQDistributor.MQPersistence.Repositories;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Messaging.Options;
using AutoMapper;
using GreenPipes;
using MassTransit;
using MassTransit.Context;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService
{
    internal class Startup
    {
        private const string DomainLayer = "ArbitralSystem.Distributor.MQDistributor.MQDomain";
        private const string PersistenceLayer = "ArbitralSystem.Distributor.MQDistributor.MQPersistence";
        private readonly IConfigurationRoot _configuration;
        
        public Startup(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var marketInfoConnectionInfo = _configuration.GetSection("Connectors:PublicMarketService")
                .Get<PublicMarketServiceConnectionInfo>();
            
            var distributorManagerOptions = _configuration.GetSection("DistributorSettings")
                .Get<DistributorManagerSettings>();
            
            services.AddMvcCore(config =>
                {
                    config.EnableEndpointRouting = false;
                    config.RespectBrowserAcceptHeader = true;
                })
                .AddApiExplorer()
                .AddControllersAsServices();
            
            services.AddAutoMapper(Assembly.GetExecutingAssembly(),
                AppDomain.CurrentDomain.Load(DomainLayer),
                AppDomain.CurrentDomain.Load(PersistenceLayer));
            
            services.AddMediatR(Assembly.GetExecutingAssembly(),
                AppDomain.CurrentDomain.Load(DomainLayer),
                AppDomain.CurrentDomain.Load(PersistenceLayer));
            
            services.AddMassTransit(x =>
                {
                    x.AddConsumer<ServerConsumer>();
                    x.AddConsumer<HeartBeatConsumer>();
                    x.AddConsumer<OrderBookDistributorConsumer>();
                    
                    MessageCorrelation.UseCorrelationId<IServerCreatedMessage>(o=>o.CorrelationId);
                    MessageCorrelation.UseCorrelationId<IServerDeletedMessage>(o=>o.CorrelationId);
                    MessageCorrelation.UseCorrelationId<IOrderBookDistributorStatusMessage>(o=>o.CorrelationId);
                    MessageCorrelation.UseCorrelationId<IHeartBeatOrderBookDistributorMessage>(o=>o.CorrelationId);
                    
                    x.UsingRabbitMq((context, cfg) =>
                        {
                           cfg.Host(distributorManagerOptions.MqHost);
                           
                           cfg.ReceiveEndpoint(Constants.Queues.MQManagerConsumer, e =>
                           {
                               e.UseMessageRetry(r => r.Interval(10, TimeSpan.FromSeconds(5)));
                               e.PrefetchCount = 1;
                               e.ConfigureConsumer<ServerConsumer>(context);
                               e.ConfigureConsumer<OrderBookDistributorConsumer>(context);
                           });
                           
                           cfg.ReceiveEndpoint(Constants.Queues.MQManagerHeartBeatConsumer, e =>
                           {
                               e.AutoDelete = true;
                               e.PrefetchCount = 1;
                               e.ConfigureConsumer<HeartBeatConsumer>(context);
                           });
                        });
                });
            services.AddMassTransitHostedService();
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
            
            services.AddVersionedApiExplorer();
            services.AddArbitralSystemDbContext(distributorManagerOptions.DatabaseConnection);
            services.AddArbitralSystemSwagger("Mq Distributor manager");

            services.AddScoped<PairInfoService>();
            services.AddPublicMarketInfo(marketInfoConnectionInfo);
            services.AddScoped<OrderBookDistributorDomainService>();
            services.AddScoped<IDistributorRepository,DistributorRepository>();
            services.AddScoped<IServerRepository,ServerRepository>();
            services.AddScoped<IHeartBeatRepository,HeartBeatRepository>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                });
            
            app.UseSerilogRequestLogging();
            //app.UseHttpsRedirection();
            app.UseMvc();
            
            MigrateDbContext(app);
        }
        
        private static void MigrateDbContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DistributorDbContext>())
                {
                    context.Database.Migrate();
                    context.SaveChanges();
                }
            }
        }
    }
}