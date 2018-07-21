﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Modix.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    [DbContext(typeof(ModixContext))]
    partial class ModixContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Modix.Data.Models.BehaviourConfiguration", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Category")
                    .IsRequired();

                b.Property<string>("Key")
                    .IsRequired();

                b.Property<string>("Value")
                    .IsRequired();

                b.HasKey("Id");

                b.ToTable("BehaviourConfigurations");
            });

            modelBuilder.Entity("Modix.Data.Models.Core.ClaimMappingEntity", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Claim")
                    .IsRequired();

                b.Property<long>("CreateActionId");

                b.Property<long>("GuildId");

                b.Property<long?>("RescindActionId");

                b.Property<long?>("RoleId");

                b.Property<string>("Type")
                    .IsRequired();

                b.Property<long?>("UserId");

                b.HasKey("Id");

                b.HasIndex("CreateActionId")
                    .IsUnique();

                b.HasIndex("RescindActionId")
                    .IsUnique();

                b.ToTable("ClaimMappings");
            });

            modelBuilder.Entity("Modix.Data.Models.Core.ConfigurationActionEntity", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<long?>("ClaimMappingId");

                b.Property<DateTimeOffset>("Created");

                b.Property<long>("CreatedById");

                b.Property<string>("Type")
                    .IsRequired();

                b.HasKey("Id");

                b.HasIndex("ClaimMappingId");

                b.HasIndex("CreatedById");

                b.ToTable("ConfigurationActions");
            });

            modelBuilder.Entity("Modix.Data.Models.Core.UserEntity", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Discriminator")
                    .IsRequired();

                b.Property<DateTimeOffset>("FirstSeen");

                b.Property<DateTimeOffset>("LastSeen");

                b.Property<string>("Nickname");

                b.Property<string>("Username")
                    .IsRequired();

                b.HasKey("Id");

                b.ToTable("Users");
            });

            modelBuilder.Entity("Modix.Data.Models.Moderation.InfractionEntity", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<long>("CreateActionId");

                b.Property<DateTimeOffset>("Created");

                b.Property<TimeSpan?>("Duration");

                b.Property<long?>("RescindActionId");

                b.Property<long>("SubjectId");

                b.Property<string>("Type")
                    .IsRequired();

                b.HasKey("Id");

                b.HasIndex("CreateActionId")
                    .IsUnique();

                b.HasIndex("RescindActionId")
                    .IsUnique();

                b.HasIndex("SubjectId");

                b.ToTable("Infractions");
            });

            modelBuilder.Entity("Modix.Data.Models.Moderation.ModerationActionEntity", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<DateTimeOffset>("Created");

                b.Property<long>("CreatedById");

                b.Property<long?>("InfractionId");

                b.Property<string>("Reason")
                    .IsRequired();

                b.Property<string>("Type")
                    .IsRequired();

                b.HasKey("Id");

                b.HasIndex("CreatedById");

                b.HasIndex("InfractionId");

                b.ToTable("ModerationActions");
            });

            modelBuilder.Entity("Modix.Data.Models.Moderation.ModerationConfigEntity", b =>
            {
                b.Property<long>("GuildId")
                    .ValueGeneratedOnAdd();

                b.Property<DateTimeOffset>("Created");

                b.Property<long>("MuteRoleId");

                b.HasKey("GuildId");

                b.ToTable("ModerationConfigs");
            });

            modelBuilder.Entity("Modix.Data.Models.Promotion.PromotionCampaignEntity", b =>
            {
                b.Property<long>("PromotionCampaignId")
                    .ValueGeneratedOnAdd();

                b.Property<long>("PromotionForId");

                b.Property<DateTimeOffset>("StartDate");

                b.Property<int>("Status");

                b.HasKey("PromotionCampaignId");

                b.HasIndex("PromotionForId");

                b.ToTable("PromotionCampaigns");
            });

            modelBuilder.Entity("Modix.Data.Models.Promotion.PromotionCommentEntity", b =>
            {
                b.Property<long>("PromotionCommentId")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Body");

                b.Property<DateTimeOffset>("PostedDate");

                b.Property<long?>("PromotionCampaignId");

                b.Property<int>("Sentiment");

                b.HasKey("PromotionCommentId");

                b.HasIndex("PromotionCampaignId");

                b.ToTable("PromotionComments");
            });

            modelBuilder.Entity("Modix.Data.Models.Moderation.NoteEntity", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Username");

                b.Property<string>("Message");

                b.Property<string>("RecordedBy");

                b.Property<DateTime>("RecordedTime");

                b.Property<decimal>("UserId")
                    .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                b.HasKey("Id");

                b.ToTable("Notes");
            });

            modelBuilder.Entity("Modix.Data.Models.Core.ClaimMappingEntity", b =>
            {
                b.HasOne("Modix.Data.Models.Core.ConfigurationActionEntity", "CreateAction")
                    .WithOne("CreatedClaimMapping")
                    .HasForeignKey("Modix.Data.Models.Core.ClaimMappingEntity", "CreateActionId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Modix.Data.Models.Core.ConfigurationActionEntity", "RescindAction")
                    .WithOne("RescindedClaimMapping")
                    .HasForeignKey("Modix.Data.Models.Core.ClaimMappingEntity", "RescindActionId");
            });

            modelBuilder.Entity("Modix.Data.Models.Core.ConfigurationActionEntity", b =>
            {
                b.HasOne("Modix.Data.Models.Core.ClaimMappingEntity", "ClaimMapping")
                    .WithMany()
                    .HasForeignKey("ClaimMappingId");

                b.HasOne("Modix.Data.Models.Core.UserEntity", "CreatedBy")
                    .WithMany()
                    .HasForeignKey("CreatedById")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("Modix.Data.Models.Moderation.InfractionEntity", b =>
            {
                b.HasOne("Modix.Data.Models.Moderation.ModerationActionEntity", "CreateAction")
                    .WithOne("CreatedInfraction")
                    .HasForeignKey("Modix.Data.Models.Moderation.InfractionEntity", "CreateActionId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Modix.Data.Models.Moderation.ModerationActionEntity", "RescindAction")
                    .WithOne("RescindedInfraction")
                    .HasForeignKey("Modix.Data.Models.Moderation.InfractionEntity", "RescindActionId");

                b.HasOne("Modix.Data.Models.Core.UserEntity", "Subject")
                    .WithMany()
                    .HasForeignKey("SubjectId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("Modix.Data.Models.Moderation.ModerationActionEntity", b =>
            {
                b.HasOne("Modix.Data.Models.Core.UserEntity", "CreatedBy")
                    .WithMany()
                    .HasForeignKey("CreatedById")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Modix.Data.Models.Moderation.InfractionEntity", "Infraction")
                    .WithMany()
                    .HasForeignKey("InfractionId");
            });

            modelBuilder.Entity("Modix.Data.Models.Promotion.PromotionCampaignEntity", b =>
            {
                b.HasOne("Modix.Data.Models.Core.UserEntity", "PromotionFor")
                    .WithMany()
                    .HasForeignKey("PromotionForId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("Modix.Data.Models.Promotion.PromotionCommentEntity", b =>
            {
                b.HasOne("Modix.Data.Models.Promotion.PromotionCampaignEntity", "PromotionCampaign")
                    .WithMany("Comments")
                    .HasForeignKey("PromotionCampaignId");
            });
#pragma warning restore 612, 618
        }
    }
}

