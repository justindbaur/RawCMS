﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using RawCMS.Library.Core;
using RawCMS.Library.Core.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace RawCMS
{
    public class Startup
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private AppEngine appEngine;

        public Startup(IWebHostEnvironment env)
        {

            this.loggerFactory = new LoggerFactory();
            var path = ApplicationLogger.GetConfigPath(env.EnvironmentName);
            NLog.LogManager.LoadConfiguration(path);
            this.logger = this.loggerFactory.CreateLogger<Startup>();
            logger.LogInformation($"Starting RawCMS, environment={env.EnvironmentName}");

            ApplicationLogger.SetLogFactory(loggerFactory);

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        //This method gets called by the runtime.Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            appEngine.InvokeConfigure(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            appEngine.RegisterPluginsMiddlewares(app);

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "RawCms API V1");
            });

           

            
        }

        //This method gets called by the runtime.Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddCors(opt => opt.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader();
                p.AllowAnyMethod();
                p.AllowAnyOrigin();
            }));

            var builder  = services.AddRazorPages();

            var ass = new List<Assembly>();

            var pluginPath = Configuration.GetValue<string>("PluginPath");
            logger.LogInformation($"loading plugins from {pluginPath}");
            Console.WriteLine($"loading plugins from {pluginPath}");
            List<Assembly> allAssembly = AssemblyHelper.GetAllAssembly();
            File.AppendAllLines("C:\\temp\\Assemblies.txt", allAssembly.Select(x =>  (x.Location + ";" + x.FullName + ";") ).ToArray());
            var assLib = Path.Combine(AppContext.BaseDirectory, @"RawCms.Views.dll");
            AssemblyLoadContext.Default.LoadFromAssemblyPath(assLib);

          
          

            ReflectionManager rm = new ReflectionManager(allAssembly);

            appEngine = AppEngine.Create(
               pluginPath,
               loggerFactory.CreateLogger<AppEngine>(),
               rm, services, Configuration);

            appEngine.InvokeConfigureServices(ass, builder, services, Configuration);

            foreach (var a in ass.Distinct())
            {
                builder.AddApplicationPart(a).AddControllersAsServices();
            }

            services.AddControllersWithViews()
                .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });

            //Invoke appEngine after service configuration

            appEngine.InvokePostConfigureServices(services);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Web API", Version = "v1" });
                c.IgnoreObsoleteProperties();
                c.IgnoreObsoleteActions();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.CustomSchemaIds(t => t.FullName);
            });
        }
    }
}