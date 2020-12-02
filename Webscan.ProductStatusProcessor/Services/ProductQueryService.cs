using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Webscan.ProductStatusProcessor.Models;
using Webscan.Scanner;

namespace Webscan.ProductStatusProcessor.Services
{
    public class ProductQueryService : IProductQueryService
    {
        IServiceProvider _serviceProvider;
        IWebScannerService _webScannerService;
        public ProductQueryService(IServiceProvider serviceProvider, IWebScannerService webScannerService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(serviceProvider)} cannot be null");
            _webScannerService = webScannerService ?? throw new ArgumentNullException($"{nameof(webScannerService)} cannot be null");
        }

        public async Task<bool> IsProductInStock(StatusCheck statusCheck)
        {

            try
            {
                //ILogger<ProductQueryService> _logger = scope.ServiceProvider.GetRequiredService<ILogger<ProductQueryService>>();
                //_logger.LogInformation($"{DateTime.Now}: Querying {statusCheck.Name}");

                Console.WriteLine($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Querying {statusCheck.Name}");

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(statusCheck.Url)
                };

                string html = await _webScannerService.GetDocument(request);
                string xPathContentString = await _webScannerService.GetXpathText(html, statusCheck.XPath);

                Console.WriteLine($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Querying {statusCheck.Name} Complete");

                // Check to see if failure string is the same as retreived 
                if (statusCheck.XPathContentFailureString != xPathContentString)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }catch(ArgumentNullException e)
            {
                Console.WriteLine($"Xpath was not found in document: {e.Message}");
                return false;
            }catch(Exception e)
            {
                Console.WriteLine($"error occured! {e.Message}");
                return false;
            }

        }

    }
}
