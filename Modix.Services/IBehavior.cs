using System.Threading.Tasks;

namespace Modix.Services
{
    public interface IBehavior
    {
        Task StartAsync();

        Task StopAsync();
    }
}
