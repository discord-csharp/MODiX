using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord.Commands;
using Discord;
using Modix.Services.Utilities;
using System.Text.RegularExpressions;
using Kitsu.Anime;

namespace Modix.Modules
{
    [Group("anime"), Name("Anime"), Summary("Various weeb-related commands")]
    public class AnimeModule : ModuleBase
    {
        [Command("search"), Summary("Searches the Kitsu anime database")]
        public async Task Search([Remainder]string query)
        {
            IEnumerable<AnimeDataModel> found = null;

            try
            {
                var result = await Anime.GetAnimeAsync(query);
                found = result.Data.Where(d=>!d.Attributes.Nsfw);
            }
            catch (Exception) //Why does this package throw raw exceptions
            {
                await ReplyAsync($"Nothing found for **{query}**");
                return;
            }

            var first = found.First();

            string synopsis = first.Attributes.Synopsis.TruncateTo(300);

            var embed = new EmbedBuilder()
                .WithTitle($"**{first.Attributes.CanonicalTitle}** - {first.Type}")
                .WithDescription($"Rating: {first.Attributes.AverageRating}\n{synopsis} **[More]({GetUrl(first)})**\n\nAlso see:")
                .WithThumbnailUrl(first.Attributes.PosterImage.Medium);
            
            foreach (var anime in found.Skip(1).Take(4))
            {
                embed.AddInlineField(anime.Attributes.CanonicalTitle, $"[↪ Kitsu]({GetUrl(anime)})");
            }

            await ReplyAsync($"Results for **{query}**", false, embed);
        }

        private string GetUrl(AnimeDataModel anime) => $"https://kitsu.io/anime/{anime.Attributes.Slug}";
    }
}
