using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class ConfigurationActionRepository : RepositoryBase, IConfigurationActionRepository
    {
        /// <summary>
        /// Constructs a new <see cref="ConfigurationActionRepository"/> from the given dependencies.
        /// See <see cref="RepositoryBase"/> for details.
        /// </summary>
        public ConfigurationActionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<ConfigurationActionSummary> ReadAsync(long actionId)
            => ModixContext.ConfigurationActions.AsNoTracking()
                .Select(ConfigurationActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync(x => x.Id == actionId);
    }
}
