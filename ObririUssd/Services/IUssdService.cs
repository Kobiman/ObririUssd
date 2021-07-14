using ObririUssd.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace ObririUssd.Services
{
    public interface IUssdService
    {
        Task<UssdResponse> ProcessRequest(UssdRequest request);
    }
}