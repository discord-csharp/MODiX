# Data Modeling

## Purpose

1. Represent, in C# code, the structures of data the are passed between different layers of the application, and used to persist data within the [Data Storage](Data-Storage) layer
2. Allow for type-safe manipulation of data within the application
3. Allow for streamlined maintenance of the [Data Storage](Data-Storage) layer, supporting many independent deployed instances

## Methodology

### Entity Framework Core

[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) is the primary mechanism of Data Access used by MODiX. In short, it is:

A) A data modeling library
B) An SQL Translation/Building library
C) A set of designer tools for database maintenance

At least, this is what EF is in the context of how MODiX uses it.

The primary API of EF Core is `abstract class DbContext`, which when subclassed both models and allows interaction with a database. The primary API for modeling is the `ModelBuilder` class, which is exposed to `DbContext` classes through the `protected virtual OnModelCreating()` method. Additionally, some of the simpler aspects of modeling can be performed through `[Attribute]`-based annotations.

The task of modeling within EF Core involves writing "entity" classes, which represent the structure of records, within tables, and mapping them into the EF modeling system, either through attributes or `ModelBuilder` or some combination thereof. EF Core uses this information to build an `IModel` at runtime, that is then used to generate valid DDL and DML code to be executed by the database.

In MODiX, the `ModelBuilder` API is preferred for most things, especially for defining what tables exist (I.E. don't create `DbSet<T>` properties on `ModixContext`). Entity classes should also be named `XXXEntity` to help clarify the difference between classes serving as Entity models for EF Core, and classes serving as View Models or other purposes for the rest of the application.

E.G.
```cs
[Table("Infractions")]
public class InfractionEntity
{
    public long Id { get; set; }
    public ulong GuildId { get; set; }
    public InfractionType Type { get; set; }
    public string Reason { get; set; } = null!;
    public TimeSpan? Duration { get; set; }
    public ulong SubjectId { get; set; }
    public long CreateActionId { get; set; }
    public long? RescindActionId { get; set; }
    public long? DeleteActionId { get; set; }
    public long? UpdateActionId { get; set; }

    public virtual GuildUserEntity Subject { get; set; } = null!;
    public virtual ModerationActionEntity CreateAction { get; set; } = null!;
    public virtual ModerationActionEntity? RescindAction { get; set; }
    public virtual ModerationActionEntity? DeleteAction { get; set; }
    public virtual ModerationActionEntity? UpdateAction { get; set; }

    [OnModelCreating]
    internal static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<InfractionEntity>()
            .HasKey(x => x.Id)
            .ValueGeneratedOnAdd();

        modelBuilder
            .Entity<InfractionEntity>()
            .Property(x => x.GuildId)
            .HasConversion<long>();

        modelBuilder
            .Entity<InfractionEntity>()
            .Property(x => x.Type)
            .HasConversion<string>();

        modelBuilder
            .Entity<InfractionEntity>()
            .Property(x => x.Reason)
            .IsRequired();

        ...
    }
}
```

### View Models / Data Transfer Objects

View Models (VMs), as the name suggests, are model classes that are used for the purpose of "viewing" data. These are also sometimes called Data Transfer Objects (or DTOs). Such classes are modeled as part of the Data Modeling layer because they can directly improve the performance of EF Core. That is, EF Core can be used to query data from the database, in such a way that only the info needed to build the View Models is retrieved from the database. That is, a query can be build to retrieve View Model objects where only a subset of columns are `SELECT`ed, rather than all of the columns defined upon that entity's table. EF can also automatically generate necessary `JOIN` clauses between different tables, and assemble the resulting values together in .NET.

E.G.
```cs
public class InfractionSummary
{
    public long Id { get; set; }
    public ulong GuildId { get; set; }
    public InfractionType Type { get; set; }
    public string Reason { get; set; } = null!;
    public TimeSpan? Duration { get; set; }
    public GuildUserBrief Subject { get; set; } = null!;
    public ModerationActionBrief CreateAction { get; set; } = null!;
    public ModerationActionBrief? RescindAction { get; set; }
    public ModerationActionBrief? DeleteAction { get; set; }
    public DateTimeOffset? Expires { get; set; }

    ...

    [ExpansionExpression]
    internal static readonly Expression<Func<InfractionEntity, InfractionSummary>> FromEntityProjection
        = entity => new InfractionSummary()
        {
            Id = entity.Id,
            GuildId = entity.GuildId,
            Type = entity.Type,
            Reason = entity.Reason,
            Duration = entity.Duration,
            Subject = entity.Subject.Project(GuildUserBrief.FromEntityProjection),
            CreateAction = entity.CreateAction.Project(ModerationActionBrief.FromEntityProjection),
            RescindAction = (entity.RescindAction == null)
                ? null
                : entity.RescindAction.Project(ModerationActionBrief.FromEntityProjection),
            DeleteAction = (entity.DeleteAction == null)
                ? null
                : entity.DeleteAction.Project(ModerationActionBrief.FromEntityProjection),
            Expires = entity.CreateAction.Created + entity.Duration
        };
}
```

### Migrations

Migrations are another bit of functionality driven by EF Core. `Migration` is an `abstract class` provided by EF Core that represents structural changes to a database. It includes code, defined through the EF Core `MigrationBuilder` API, which can perform (or revert) the actual changes upon a target database. Defining migrations explicitly, within the codebase, supports deployment of MODiX to any number of independent instances, hosted by independent users.

Migrations should generally be scaffolded (initially) by the EF Designer Tools (I.E. `dotnet ef migrations add MyMigration`). The code files generated by this command (and the modifications made to `Modix.Data.Migrations.ModixContextModelSnapshot`) should then be reviewed manually, and if applicable, changes should be made manually to the `Migration.Up()` and `Migration.Down()` methods to prevent data loss.

E.G.
```cs
public partial class AddInfractionMessagePromotionCampaignNavigationToGuildUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Messages_ChannelId",
            table: "Messages",
            column: "ChannelId");

        migrationBuilder.AddForeignKey(
            name: "FK_Messages_GuildChannels_ChannelId",
            table: "Messages",
            column: "ChannelId",
            principalTable: "GuildChannels",
            principalColumn: "ChannelId",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Messages_GuildUsers_GuildId_AuthorId",
            table: "Messages",
            columns: new[] { "GuildId", "AuthorId" },
            principalTable: "GuildUsers",
            principalColumns: new[] { "GuildId", "UserId" },
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Messages_GuildChannels_ChannelId",
            table: "Messages");

        migrationBuilder.DropForeignKey(
            name: "FK_Messages_GuildUsers_GuildId_AuthorId",
            table: "Messages");

        migrationBuilder.DropIndex(
            name: "IX_Messages_ChannelId",
            table: "Messages");
    }
}
```

### Testing

Testing is really not applicable to the Data Modeling layer, since model classes should contain no logic.
