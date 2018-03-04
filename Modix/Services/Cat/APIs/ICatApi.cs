using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Cat.APIs
{
    public interface ICatApi
    {
        /// <summary>
        /// Fetches a random cat image
        /// </summary>
        /// <returns></returns>
        Task<CatResponse> Fetch(CatMediaType type, CancellationToken cancellationToken = default);
    }
}