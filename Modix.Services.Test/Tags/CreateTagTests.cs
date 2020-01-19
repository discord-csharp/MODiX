using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Services.Tags;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Modix.Data.Models.Tags;

namespace Modix.Services.Test.Tags
{
    [TestFixture]
    public class CreateTagTests
    {
        private static (AutoMocker autoMocker, TagService sut, ModixContext db) GetSut()
        {
            var autoMocker = new AutoMocker();

            var options = new DbContextOptionsBuilder<ModixContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

            var context = new ModixContext(options.Options);

            autoMocker.Use(context);

            var sut = autoMocker.CreateInstance<TagService>();

            return (autoMocker, sut, context);
        }

        [Test]
        public async Task CreateTagAsync_NameIsNotProvided_Throws()
        {
            (_, var sut, _) = GetSut();
            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, null, string.Empty));
        }

        [Test]
        public async Task CreateTagAsync_ContentIsNotProvided_Throws()
        {
            (_, var sut, _) = GetSut();
            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, string.Empty, null));
        }

        [Test]
        public async Task CreateTagAsync_NameEndsWithPunctuation_Throws()
        {
            (_, var sut, _) = GetSut();
            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, "MODiX.", string.Empty));
        }

        [Test]
        public async Task CreateTagAsync_TagAlreadyExistsWithSameCasing_Throws()
        {
            (var autoMocker, var sut, var db) = GetSut();

            db.Tags.Add(new Data.Models.Tags.TagEntity
            {
                Name = "modix"
            });

            db.SaveChanges();

            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, "modix", string.Empty));
        }

        [Test]
        public async Task CreateTagAsync_TagAlreadyExistsWithDifferentCasing_Throws()
        {
            (var autoMocker, var sut, var db) = GetSut();

            db.Tags.Add(new Data.Models.Tags.TagEntity
            {
                Name = "modix"
            });

            db.SaveChanges();

            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, "MODiX", string.Empty));
        }

        [Test]
        public async Task CreateTagAsync_ValidTag_InsertsTag()
        {
            (_, var sut, var db) = GetSut();

            await sut.CreateTagAsync(1, 1, "MODiX", "Content");

            db.Tags.Count().ShouldBe(1);

            var tag = db.Tags.Single();

            tag.GuildId.ShouldBe((ulong)1);
            tag.OwnerUserId.ShouldBe((ulong)1);
            tag.Name.ShouldBe("modix");
            tag.Content.ShouldBe("Content");
        }

        [Test]
        public async Task CreateTagAsync_ValidTag_InsertsCreateAction()
        {
            (_, var sut, var db) = GetSut();

            await sut.CreateTagAsync(1, 1, "MODiX", "Content");

            db.TagActions.Count().ShouldBe(1);

            var action = db.TagActions.Single();

            action.GuildId.ShouldBe((ulong)1);
            action.CreatedById.ShouldBe((ulong)1);
            action.Type.ShouldBe(TagActionType.TagCreated);
        }

        [Test]
        public async Task CreateTagAsync_ValidTag_LinksTagToCreateAction()
        {
            (_, var sut, var db) = GetSut();

            await sut.CreateTagAsync(1, 1, "MODiX", "Content");

            var tag = db.Tags.Single();

            tag.CreateAction.ShouldNotBeNull();
        }
    }
}
