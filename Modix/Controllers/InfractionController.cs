using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Moderation;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.RowboatImporter;
using Newtonsoft.Json;

namespace Modix.Controllers
{
    [Route("~/api/infractions")]
    public class InfractionController : ModixController
    {
        private IModerationService ModerationService { get; }
        private RowboatInfractionImporterService ImporterService { get; }

        public InfractionController(DiscordSocketClient client, IAuthorizationService modixAuth, IModerationService moderationService, RowboatInfractionImporterService importerService) : base(client, modixAuth)
        {
            ModerationService = moderationService;
            ImporterService = importerService;
        }

        [HttpGet]
        public async Task<IActionResult> Infractions()
        {
            var result = await ModerationService.SearchInfractionsAsync(new InfractionSearchCriteria
            {
                GuildId = UserGuild.Id
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> InfractionsForUser(ulong id)
        {
            var result = await ModerationService.SearchInfractionsAsync(new InfractionSearchCriteria
            {
                IsDeleted = false,
                IsRescinded = false,
                SubjectId = id,
                GuildId = UserGuild.Id
            });

            return Ok(result);
        }

        [HttpPut("import")]
        public Task<IActionResult> Import()
        {
            throw new NotImplementedException();

            //if (Request.Form.Files.Count == 0 || Request.Form.Files.First().ContentType != "application/json")
            //{
            //    return BadRequest("Must submit a JSON file");
            //}

            //int importCount = 0;

            //try
            //{
            //    using (var httpStream = Request.Form.Files.First().OpenReadStream())
            //    using (var streamReader = new StreamReader(httpStream))
            //    {
            //        var content = await streamReader.ReadToEndAsync();
            //        var loaded = JsonConvert.DeserializeObject<IEnumerable<RowboatInfraction>>(content);

            //        importCount = await ImporterService.ImportInfractionsAsync(loaded);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}

            //return Ok(importCount);
        }
    }
}
