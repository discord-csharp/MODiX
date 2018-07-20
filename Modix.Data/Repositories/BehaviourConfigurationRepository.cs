using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;

namespace Modix.Data.Repositories
{
    public interface IBehaviourConfigurationRepository
    {
        Task<IEnumerable<BehaviourConfiguration>> GetBehaviours();
    }

    public class BehaviourConfigurationRepository : IBehaviourConfigurationRepository
    {
        private readonly ModixContext _modixContext;

        public BehaviourConfigurationRepository(ModixContext modixContext)
        {
            _modixContext = modixContext;
        }

        public async Task<IEnumerable<BehaviourConfiguration>> GetBehaviours()
        {
            return await _modixContext.BehaviourConfigurations.ToListAsync();
        }
    }
}
