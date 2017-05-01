using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Modix.Data;

namespace Modix.Data.Migrations
{
    [DbContext(typeof(ModixContext))]
    [Migration("20170430010305_AddChannelLimits")]
    partial class AddChannelLimits
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

                    b.Property<int?>("GuildId");

                    b.Property<string>("Reason");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Bans");
                });

            modelBuilder.Entity("Modix.Data.Models.ChannelLimit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ChannelId");

                    b.Property<int?>("GuildId");

                    b.Property<string>("ModuleName");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("ChannelLimits");
                });

            modelBuilder.Entity("Modix.Data.Models.DiscordGuild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ConfigId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<long>("DiscordId");

                    b.Property<string>("Name");

                    b.Property<int>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("ConfigId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Modix.Data.Models.DiscordMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string[]>("Attachments");

                    b.Property<int?>("AuthorId");

                    b.Property<string>("Content");

                    b.Property<int?>("DiscordGuildId");

                    b.Property<long>("DiscordId");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("DiscordGuildId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Modix.Data.Models.DiscordUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AvatarUrl");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<long>("DiscordId");

                    b.Property<bool>("IsBot");

                    b.Property<string>("Nickname");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Modix.Data.Models.GuildConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AdminRoleId");

                    b.Property<long>("GuildId");

                    b.Property<long>("ModeratorRoleId");

                    b.HasKey("Id");

                    b.ToTable("GuildConfig");
                });

            modelBuilder.Entity("Modix.Data.Models.Ban", b =>
                {
                    b.HasOne("Modix.Data.Models.DiscordGuild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId");
                });

            modelBuilder.Entity("Modix.Data.Models.ChannelLimit", b =>
                {
                    b.HasOne("Modix.Data.Models.DiscordGuild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId");
                });

            modelBuilder.Entity("Modix.Data.Models.DiscordGuild", b =>
                {
                    b.HasOne("Modix.Data.Models.GuildConfig", "Config")
                        .WithMany()
                        .HasForeignKey("ConfigId");

                    b.HasOne("Modix.Data.Models.DiscordUser", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Modix.Data.Models.DiscordMessage", b =>
                {
                    b.HasOne("Modix.Data.Models.DiscordUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId");

                    b.HasOne("Modix.Data.Models.DiscordGuild", "DiscordGuild")
                        .WithMany()
                        .HasForeignKey("DiscordGuildId");
                });
        }
    }
}
