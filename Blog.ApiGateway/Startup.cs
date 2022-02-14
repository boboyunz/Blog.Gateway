using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Nacos;
using Ocelot.ServiceDiscovery;
//using Blog.Core.OcelotMiddleware;
using Blog.Core.Extensions;
using Blog.Core.Common;
using Blog.Core.Middlewares;
//using Nacos.AspNetCore.V2;
using Nacos.V2;
using Nacos.V2.DependencyInjection;
using ApiGateway.Helper;
using Microsoft.Extensions.Logging;
using Blog.Core.Common.LogHelper;
using Blog.Core.Common.Helper;
using Swashbuckle.AspNetCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ApiGatewayDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCacheSetup();
            services.AddSingleton(new Appsettings(Configuration));
            //services.AddSqlsugarSetup();
           // services.AddDbSetup();
            services.AddCorsSetup();

            services.AddNacosV2Config(Configuration, null, "nacosConfig");
            services.AddNacosV2Naming(Configuration, null, "nacos");
            services.AddHostedService<ApiGateway.Helper.OcelotConfigurationTask>();

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ApiGateway", new OpenApiInfo { Title = "网关服务", Version = "v1" });
            });



            services.AddAuthentication_JWTSetup();
            services.AddOcelot().AddNacosDiscovery();
        }
        // 注意在Program.CreateHostBuilder，添加Autofac服务工厂
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory)
        {
            Appsettings.Env = env;
            loggerFactory.AddDelegateLoggerCom();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }           
            // CORS跨域
            app.UseCors(Appsettings.app(new string[] { "Startup", "Cors", "PolicyName" }));

            var apis = new List<string> { "baseapi" };
            app.UseMvc().UseSwagger().UseSwaggerUI(options =>
            {
                apis.ForEach(m =>
                {
                    options.SwaggerEndpoint($"/{m}/swagger.json", m);
                    options.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("CATS.ApiGateway.index.html");
                });
            });

            app.UseJwtTokenAuth();
            app.UseOcelot().Wait();
         
        }
    }
    public static class Extensions
    {
        public static void UseIfNotNull(this IApplicationBuilder builder,
            Func<HttpContext, Func<Task>, Task> middleware)
        {
            if (middleware != null)
            {
                builder.Use(middleware);
            }
        }
    }
}
