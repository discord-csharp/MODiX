namespace Modix.Services.Notes
{
    using System.Threading.Tasks;
    using Discord;

    public interface INoteCreatorService
    {
        Task<bool> Create(IGuildUser user, string message, string recordedBy);
    }
}