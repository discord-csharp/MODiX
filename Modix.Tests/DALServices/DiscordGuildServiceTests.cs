using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Services;
using Xunit;
using Xunit.Sdk;

namespace Modix.Tests.DALServices
{
    public class DiscordGuildServiceTests
    {
        [Fact]
        public void Adds_To_DB()
        {
            var options = new DbContextOptionsBuilder<ModixContext>()
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .Options;

            // Run the test against one instance of the context
            using (var context = new ModixContext(options))
            {
                var service = new DiscordGuildService(context);
                service.AddAsync(new IGuild()
                {
                    Id = 3
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = new ModixContext(options))
            {
                Assert.AreEqual(1, context.Blogs.Count());
                Assert.AreEqual("http://sample.com", context.Blogs.Single().Url);
            }
        }
    }
}
