using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
namespace ApiGatewayDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:8090");
                })
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("ocelot.json", true, true);
            })
            .ConfigureLogging((hostingContext, loggingBuilder) => {

                loggingBuilder.ClearProviders();
                // now register providers you need
                //loggingBuilder.AddDebug();
                //loggingBuilder.AddEventSourceLogger();
                //loggingBuilder.AddProvider
                //if (hostingContext.HostingEnvironment.IsDevelopment())
                //{
                //    loggingBuilder.AddConsole();
                //}
            })
            ;
    }
}
