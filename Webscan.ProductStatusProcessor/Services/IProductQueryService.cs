using System.Threading.Tasks;
using Webscan.ProductStatusProcessor.Models;

namespace Webscan.ProductStatusProcessor.Services
{
    public interface IProductQueryService
    {
        Task<bool> IsProductInStock(StatusCheck statusCheck);
    }
}