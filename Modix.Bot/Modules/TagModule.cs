using System.Threading.Tasks;

using Discord.Commands;

using Modix.Services.Tags;

namespace Modix.Bot.Modules
{
    [Name("Tags")]
    [Summary("Use and maintain tags.")]
    [Group("tag")]
    public class TagModule : ModuleBase
    {
        public TagModule(ITagService tagService)
        {
            TagService = tagService;
        }

        [Command("create")]
        [Summary("Creates a new tag.")]
        public async Task CreateTagAsync(
            [Summary("The name that will be used to invoke the tag.")]
                string name,
            [Remainder]
            [Summary("The message that will be displayed when the tag is invoked.")]
                string content)
        {
            await TagService.CreateTagAsync(Context.Guild.Id, Context.User.Id, name, content);
            await Context.AddConfirmation();
        }

        [Command]
        [Summary("Invokes a tag so that the message associated with the tag will be displayed.")]
        public async Task UseTagAsync(
            [Summary("The name that will be used to invoke the tag.")]
                string name)
            => await TagService.UseTagAsync(Context.Guild.Id, Context.Channel.Id, name);

        [Command("update")]
        [Summary("Updates the contents of a tag.")]
        public async Task ModifyTagAsync(
            [Summary("The name that is used to invoke the tag.")]
                string name,
            [Remainder]
            [Summary("The new message that will be displayed when the tag is invoked.")]
                string newContent)
        {
            await TagService.ModifyTagAsync(Context.Guild.Id, Context.User.Id, name, newContent);
            await Context.AddConfirmation();
        }

        [Command("delete")]
        [Alias("remove")]
        [Summary("Deletes a tag.")]
        public async Task DeleteTagAsync(
            [Summary("The name that is used to invoke the tag.")]
                string name)
        {
            await TagService.DeleteTagAsync(Context.Guild.Id, Context.User.Id, name);
            await Context.AddConfirmation();
        }

        protected ITagService TagService { get; }
    }
}
