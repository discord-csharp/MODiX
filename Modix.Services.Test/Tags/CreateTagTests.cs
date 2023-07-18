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

            db.Set<TagEntity>().Add(new Data.Models.Tags.TagEntity
            {
                Name = "modix",
                Content = "some content",
            });

            db.SaveChanges();

            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, "modix", string.Empty));
        }

        [Test]
        public async Task CreateTagAsync_TagAlreadyExistsWithDifferentCasing_Throws()
        {
            (var autoMocker, var sut, var db) = GetSut();

            db.Set<TagEntity>().Add(new Data.Models.Tags.TagEntity
            {
                Name = "modix",
                Content = "some content",
            });

            db.SaveChanges();

            await Should.ThrowAsync<ArgumentException>(() => sut.CreateTagAsync(0, 0, "MODiX", string.Empty));
        }
    }
}
