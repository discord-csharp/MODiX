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
using Kitsu.Manga;
using Kitsu;

namespace Modix.Modules
{
    [Name("Anime"), Summary("Various weeb-related commands")]
    public class AnimeModule : ModuleBase
    {
        [Command("anime search"), Summary("Searches the Kitsu anime database")]
        public async Task SearchAnime([Remainder]string query)
        {
            try
            {
                var result = await Anime.GetAnimeAsync(query);

                var found = result.Data
                    .Where(d=>!d.Attributes.Nsfw && d.Attributes.AgeRating != "R18")
                    .Select(d => WeebDataAbstraction.FromAnime(d));

                await ReplyWithEmbed(query, found);
            }
            catch (NoDataFoundException) //Why does this package throw raw exceptions
            {
                await ReplyAsync($"Nothing found for **{query}**");
            }
        }

        [Command("manga search"), Summary("Searches the Kitsu manga database")]
        public async Task SearchManga([Remainder]string query)
        {
            try
            {
                var result = await Manga.GetMangaAsync(query);

                var found = result.Data
                    .Where(d => d.Attributes.AgeRating != "R18")
                    .Select(d => WeebDataAbstraction.FromManga(d));

                await ReplyWithEmbed(query, found);
            }
            catch (NoDataFoundException) //Why does this package throw raw exceptions
            {
                await ReplyAsync($"Nothing found for **{query}**");
            }
        }

        public async Task ReplyWithEmbed(string query, IEnumerable<WeebDataAbstraction> found)
        {
            var first = found.First();

            string ratingString = "";

            if (!String.IsNullOrWhiteSpace(first.Rating))
            {
                ratingString = $"Rating: {first.Rating}\n";
            }

            var embed = new EmbedBuilder()
                .WithAuthor($"{first.Title} - {first.Type}", "", first.Url)
                .WithDescription($"{ratingString}{first.Synopsis}\n\nAlso see:")
                .WithThumbnailUrl(first.PosterThumbnail);

            foreach (var entry in found.Skip(1).Take(2))
            {
                embed.AddInlineField(entry.Title, $"[⇒ Kitsu]({entry.Url})");
            }

            await ReplyAsync($"Results for **{query}**", false, embed);
        }

        public class WeebDataAbstraction
        {
            public enum KitsuResponseType
            {
                Anime,
                Manga
            }

            public KitsuResponseType Type { get; set; }

            public string Title { get; set; }
            public string MediaKind { get; set; }
            public string Rating { get; set; }
            public string Synopsis { get; set; }
            public string Slug { get; set; }
            public string PosterThumbnail { get; set; }
            public string Url => $"https://kitsu.io/{Type.ToString().ToLower()}/{Slug}";

            public static WeebDataAbstraction FromAnime(AnimeDataModel anime)
            {
                return new WeebDataAbstraction
                {
                    Type = KitsuResponseType.Anime,
                    MediaKind = anime.Type,
                    Title = anime.Attributes.CanonicalTitle,
                    Synopsis = anime.Attributes.Synopsis.TruncateTo(300),
                    Rating = anime.Attributes.AverageRating,
                    PosterThumbnail = anime.Attributes.PosterImage.Medium,
                    Slug = anime.Attributes.Slug
                };
            }

            public static WeebDataAbstraction FromManga(MangaDataModel manga)
            {
                return new WeebDataAbstraction
                {
                    Type = KitsuResponseType.Manga,
                    MediaKind = manga.Type,
                    Title = manga.Attributes.CanonicalTitle,
                    Synopsis = manga.Attributes.Synopsis.TruncateTo(300),
                    Rating = manga.Attributes.AverageRating,
                    PosterThumbnail = manga.Attributes.PosterImage.Medium,
                    Slug = manga.Attributes.Slug
                };
            }
        }
    }
}
