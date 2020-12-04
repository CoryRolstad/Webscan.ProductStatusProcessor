using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Webscan.ProductStatusProcessor.Models;
using Webscan.Scanner;

namespace Webscan.ProductStatusProcessor.Services
{
    public class ProductQueryService : IProductQueryService
    {
        IServiceProvider _serviceProvider;
        IWebScannerService _webScannerService;
        ILogger<ProductQueryService> _logger;
        public ProductQueryService(IServiceProvider serviceProvider, IWebScannerService webScannerService, ILogger<ProductQueryService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(serviceProvider)} cannot be null");
            _webScannerService = webScannerService ?? throw new ArgumentNullException($"{nameof(webScannerService)} cannot be null");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)} cannot be null");
        }

        public async Task<bool> IsProductInStock(StatusCheck statusCheck)
        {

            try
            {

                _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Querying {statusCheck.Name}");

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(statusCheck.Url)
                };

                //string html = await _webScannerService.GetDocument(request);
                string html = await _webScannerService.GetDocumentStealth(request);
                string xPathContentString = await _webScannerService.GetXpathText(html, statusCheck.XPath);

                _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: Querying {statusCheck.Name} Complete");

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
                _logger.LogCritical($"Xpath was not found in document: {e.Message}");
                return false;
            }catch(Exception e)
            {
                _logger.LogCritical($"{statusCheck.Name} ERROR: {e.Message}");
                return false;
            }

        }

    }
}
