using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Cat.APIs
{
    public interface ICatApi
    {
        /// <summary>
        /// Fetches a random cat image URL
        /// </summary>
        /// <returns></returns>
        Task<string> Fetch(CancellationToken cancellationToken = default);
    }
}