namespace Modix.Services.Animals
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAnimalApi
    {
        /// <summary>
        /// Fetches a random image specified by the invoking module
        /// </summary>
        /// <returns></returns>
        Task<Response> FetchAsync(MediaType mediaType, CancellationToken cancellationToken = default);
    }
}