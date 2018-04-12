using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Services.Promotions;

namespace Modix.Modules
{
    [Name("Promotions"), Summary("Manage promotion campaigns")]
    public class PromotionModule : ModuleBase
    {
        /// <summary>
        /// Total width (in characters) of the rating bar
        /// </summary>
        const int BarSize = 10;
        const char FullBar = '⣿';
        const char EmptyBar = '⣀';

        private PromotionService _service;

        public PromotionModule(PromotionService service)
        {
            _service = service;
        }

        [Command("campaigns"), Summary("List all active campaigns")]
        public async Task Campaigns()
        {
            var campaigns = await _service.GetCampaigns();

            EmbedBuilder embed = new EmbedBuilder();

            foreach (var campaign in campaigns.Where(d => d.Status == CampaignStatus.Active))
            {
                StringBuilder barBuilder = new StringBuilder();

                //Number of characters that should be "filled"
                int forSize = (int)Math.Round(campaign.VoteRatio * BarSize);

                barBuilder.Append($"{campaign.SentimentIcon}  `");

                for (int i = 0; i < forSize; i++)
                {
                    barBuilder.Append(FullBar);
                }

                for (int i = 0; i < BarSize - forSize; i++)
                {
                    barBuilder.Append(EmptyBar);
                }

                barBuilder.Append($"`  {campaign.TotalVotes} vote");

                if (campaign.TotalVotes > 1)
                {
                    barBuilder.Append("s");
                }

                barBuilder.AppendLine();

                embed.Fields.Add(new EmbedFieldBuilder
                {
                    Name = campaign.Username,
                    IsInline = false,
                    Value = barBuilder.ToString()
                });
            }

            embed.Footer = new EmbedFooterBuilder { Text = "Comment at https://modix.cisien.com/promotions" };

            await ReplyAsync("**Active Campaigns**", false, embed);
        }

        [Command("nominate"), Summary("Nominate the given user for promotion!")]
        public async Task Nominate([Summary("The user to nominate - must be unranked")] SocketGuildUser user, 
                                   [Remainder, Summary("A few words on their behalf")] string reason)
        {
            if (Context.User is SocketGuildUser socketGuildUser)
            {
                try
                {
                    await _service.CreateCampaign(user, reason);
                }
                catch (ArgumentException ex)
                {
                    await ReplyAsync($"Error: {ex.Message}");
                }
            }
        }

        [Command("approve"), Summary("Approve a user's campaign, promoting them")]
        public async Task Approve(SocketGuildUser user)
        {
            var campaign = (await _service.GetCampaigns()).First(d => d.UserId == user.Id);

            if (campaign == null)
            {
                await ReplyAsync($"Error: no campaign started for *{user.Nickname ?? user.Username}*");
                return;
            }

            await _service.ApproveCampaign(Context.User as SocketGuildUser, campaign);
        }

        [Command("deny"), Summary("Deny a user's campaign")]
        public async Task Deny(SocketGuildUser user)
        {
            var campaign = (await _service.GetCampaigns()).First(d => d.UserId == user.Id);

            if (campaign == null)
            {
                await ReplyAsync($"Error: no campaign started for *{user.Nickname ?? user.Username}*");
                return;
            }

            await _service.DenyCampaign(Context.User as SocketGuildUser, campaign);
        }

        [Command("reactivate"), Summary("Reactivate a user's campaign")]
        public async Task Reactivate(SocketGuildUser user)
        {
            var campaign = (await _service.GetCampaigns()).First(d => d.UserId == user.Id);

            if (campaign == null)
            {
                await ReplyAsync($"Error: no campaign started for *{user.Nickname ?? user.Username}*");
                return;
            }

            if (campaign.Status != CampaignStatus.Denied)
            {
                await ReplyAsync("Error: That campaign hasn't been denied.");
                return;
            }

            await _service.ActivateCampaign(Context.User as SocketGuildUser, campaign);
        }
    }
}
