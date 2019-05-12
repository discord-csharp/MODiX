using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Services.ResponseEmbed
{
    public interface IResponseEmbedService
    {
        /// <summary>
        /// Builds an embed around the specified message and sends it to the specified channel
        /// </summary>
        Task BuildResponseMessage();
    }
    public class ResponseEmbedService : IResponseEmbedService
    {
    }
}
