using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Modix.Data;

namespace Modix.Data.Migrations
{
    [DbContext(typeof(ModixContext))]
    [Migration("20170223201655_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("Modix.Data.Models.Ban", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<long>("CreatorId");

                    b.Property<long>("GuildId");

                    b.Property<string>("Reason");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Bans");
                });

            modelBuilder.Entity("Modix.Data.Models.GuildConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AdminRoleId");

                    b.Property<long>("GuildId");

                    b.Property<long>("ModeratorRoleId");

                    b.HasKey("Id");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("Modix.Data.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string[]>("Attachments");

                    b.Property<string>("AvatarId");

                    b.Property<string>("AvatarUrl");

                    b.Property<string>("Content");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Discriminator");

                    b.Property<int?>("DiscriminatorValue");

                    b.Property<string>("Game");

                    b.Property<long>("GuildId");

                    b.Property<bool>("IsBot");

                    b.Property<string>("Mention");

                    b.Property<long>("MessageId");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });
        }
    }
}
