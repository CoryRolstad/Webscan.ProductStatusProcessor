using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Webscan.ProductStatusProcessor.Models;
using Webscan.ProductStatusProcessor.Services;
using Webscan.Scanner;

namespace Webscan.ProductStatusProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));

                    services.AddWebScannerService(configuration.GetSection("WebscannerSettings").Get<WebScannerSettings>());

                    services.AddSingleton<IProductQueryService, ProductQueryService>(); 

                    services.AddHostedService<ProductStatusWorker>();
                });
    }
}
