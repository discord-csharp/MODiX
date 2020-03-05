# Data Access Modeling

## Purpose

1. Define, and parameterize, the Data Access operations that the application performs
2. Isolate SQL building logic, to make queries easier to debug and optimize
3. Allow testing of Data Access logic, without the need to model realistic business scenarios
4. Allow testing of [Business Logic](Business-Logic), without the need to comprehensively model fake data

## Methodology

### Repositories

There are many definitions of the "Repository Pattern" in the programming world, but for the sake of MODiX, a "Repository" is just a container for Data Access operations, which are modeled and implemented as methods upon the repository. Repositories do not impose restrictions upon what kinds of operations the Business Logic layer can perform, nor do they serve as a definition of all operations that are possible. They simply define all distinct operations that exist. Yes, Business Logic should seek to reuse existing repository methods whenever possible, but developers are ALWAYS free to add new operations where existing operations are not optimal, or do not completely serve the needs of the Business Logic layer.

Operations defined within repositories should be business-agnostic whenever possible. Each operation should generally be classified as one of the basic types of CRUD operations (CREATE, READ, UPDATE, or DELETE), and should also generally be named as such. Repository operations can perform validation, or perform many inner operations (such as multiple CREATEs, DELETEs, or combinations thereof), when appropriate to maintain data integrity. However, these should always serve the interest of "data" integrity, rather than "business" integrity.

Repositories, like Business Services, should always be defined through an `interface`, to support mockability during consumer testing.

E.G.
```cs
public class InfractionRepository
    : ModerationActionEventRepositoryBase,
        IInfractionRepository
{
    ...

    public async Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(
            InfractionSearchCriteria searchCriteria,
            IEnumerable<SortingCriteria>? sortingCriteria = null)
        => await ModixContext.Infractions.AsNoTracking()
            .FilterBy(searchCriteria)
            .AsExpandable()
            .Select(InfractionSummary.FromEntityProjection)
            .SortBy(sortingCriteria, InfractionSummary.SortablePropertyMap)
            .ToArrayAsync();

    ...
}
```

```cs
public class InfractionSummary
{
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

```cs
internal static class InfractionQueryableExtensions
{
    public static IQueryable<InfractionEntity> FilterBy(this IQueryable<InfractionEntity> query, InfractionSearchCriteria criteria)
        => query
            .FilterBy(
                predicate:  x => x.Id == criteria.Id,
                criteria:   criteria?.Id != null)
            .FilterBy(
                predicate:  x => x.GuildId == criteria!.GuildId,
                criteria:   criteria?.GuildId != null)
            .FilterBy(
                predicate:  x => criteria!.Types.Contains(x.Type),
                criteria:   criteria?.Types?.Any() ?? false)
            ...
}
```

### Transactions

I'm not going to bother actually describing the `IRepositoryTransaction` system, because it's trash. It was built to serve a synchronization need, and does accomplish that, but it's unintuitive and clunky compared to `System.Transactions`, which is what we need to move to.

Suffice it to say that operations within the Business Logic or Data Access layers should be wrapped within an appropriate transaction whenever they A) perform multiple Data Access operations that depend upon one another (like the insertion of parent and child records), or B) perform READ operations whose results affect subsequent WRITE operations.

### Testing

MODiX utilizes the EF Core InMemory database provider to perform testing. This provider simulates the presence of a real underlying database provider, by maintaining entities within in-memory collections. This provider does have several drawbacks, though. It does not support raw SQL queries at all, and does not simulate a "Relational" database. This last bit is important, because it means that constraints enforced by a real RDBMS, such as Foriegn Key constraints, are not enforced by the InMemory provider. Thus there are categories of bugs that cannot be caught through use of the InMemory provider.

Testing of repository operations generally involves A) modeling fake data to be setup within an InMemory database, B) defining the parameters of operations to be executed upon these fake data entities, and C) defining the expected results for each set of parameters.

E.G.
```cs
[TestFixture]
public class DesignatedChannelMappingRepositoryTests
{
    private static (ModixContext, DesignatedChannelMappingRepository) BuildTestContext()
    {
        var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
        {
            x.Users.AddRange(Users.Entities.Clone());
            x.GuildUsers.AddRange(GuildUsers.Entities.Clone());
            x.GuildChannels.AddRange(GuildChannels.Entities.Clone());
            x.DesignatedChannelMappings.AddRange(DesignatedChannelMappings.Entities.Clone());
            x.ConfigurationActions.AddRange(ConfigurationActions.Entities.Where(y => !(y.DesignatedChannelMappingId is null)).Clone());
        });

        var uut = new DesignatedChannelMappingRepository(modixContext);

        return (modixContext, uut);
    }

    ...

    public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaTestCases
        = DesignatedChannelMappings.Searches
            .Where(x => x.resultIds.Any())
            .Select(x => new TestCaseData(x.criteria)
                .SetName($"{{m}}({x.name})"));

    ...    

    [TestCaseSource(nameof(ValidSearchCriteriaTestCases))]
    public async Task AnyAsync_DesignatedChannelMappingsExist_ReturnsTrue(
        DesignatedChannelMappingSearchCriteria criteria)
    {
        (var modixContext, var uut) = BuildTestContext();

        var result = await uut.AnyAsync(criteria);

        result.ShouldBeTrue();
    }
    
    ...
}
```

```cs
public static class DesignatedChannelMappings
{
    public static readonly IEnumerable<DesignatedChannelMappingEntity> Entities
        = new[]
        {
            new DesignatedChannelMappingEntity() {  Id = 1, GuildId = 1,    Type = DesignatedChannelType.MessageLog,    ChannelId = 1,  CreateActionId = 6,     DeleteActionId = 7      },
            new DesignatedChannelMappingEntity() {  Id = 2, GuildId = 2,    Type = DesignatedChannelType.ModerationLog, ChannelId = 3,  CreateActionId = 8,     DeleteActionId = null   },
            new DesignatedChannelMappingEntity() {  Id = 3, GuildId = 1,    Type = DesignatedChannelType.ModerationLog, ChannelId = 2,  CreateActionId = 9,     DeleteActionId = null   },
            new DesignatedChannelMappingEntity() {  Id = 4, GuildId = 2,    Type = DesignatedChannelType.PromotionLog,  ChannelId = 3,  CreateActionId = 10,    DeleteActionId = null   }
        };

    ...

    public static IEnumerable<(string name, DesignatedChannelMappingSearchCriteria? criteria, long[] resultIds)> Searches
        = new[]
        {
            /*  name,                   criteria,                                                                                       resultIds                   */
            (   "Null Criteria",        null,                                                                                           new long[] { 1, 2, 3, 4 }   ),
            (   "Empty Criteria",       new DesignatedChannelMappingSearchCriteria(),                                                   new long[] { 1, 2, 3, 4 }   ),
            (   "Id Valid(1)",          new DesignatedChannelMappingSearchCriteria() { Id = 1 },                                        new long[] { 1 }            ),
            (   "Id Valid(2)",          new DesignatedChannelMappingSearchCriteria() { Id = 2 },                                        new long[] { 2 }            ),
            (   "Id Invalid",           new DesignatedChannelMappingSearchCriteria() { Id = 5 },                                        new long[] { }              ),
            (   "GuildId Valid(1)",     new DesignatedChannelMappingSearchCriteria() { GuildId = 1 },                                   new long[] { 1, 3 }         ),
            (   "GuildId Valid(2)",     new DesignatedChannelMappingSearchCriteria() { GuildId = 2 },                                   new long[] { 2, 4 }         ),
            (   "GuildId Invalid",      new DesignatedChannelMappingSearchCriteria() { GuildId = 3 },                                   new long[] { }              ),
            (   "ChannelId Valid(1)",   new DesignatedChannelMappingSearchCriteria() { ChannelId = 1 },                                 new long[] { 1 }            ),
            (   "ChannelId Valid(2)",   new DesignatedChannelMappingSearchCriteria() { ChannelId = 3 },                                 new long[] { 2, 4 }         ),
            (   "ChannelId Invalid",    new DesignatedChannelMappingSearchCriteria() { ChannelId = 4 },                                 new long[] { }              ),
            (   "Type Valid(1)",        new DesignatedChannelMappingSearchCriteria() { Type = DesignatedChannelType.MessageLog },       new long[] { 1 }            ),
            (   "Type Valid(2)",        new DesignatedChannelMappingSearchCriteria() { Type = DesignatedChannelType.ModerationLog },    new long[] { 2, 3 }         ),
            (   "Type Invalid",         new DesignatedChannelMappingSearchCriteria() { Type = DesignatedChannelType.Unmoderated },      new long[] { }              ),
            (   "CreatedById Valid(1)", new DesignatedChannelMappingSearchCriteria() { CreatedById = 1 },                               new long[] { 1, 3 }         ),
            (   "CreatedById Valid(2)", new DesignatedChannelMappingSearchCriteria() { CreatedById = 2 },                               new long[] { 4 }            ),
            (   "CreatedById Invalid",  new DesignatedChannelMappingSearchCriteria() { CreatedById = 4 },                               new long[] { }              ),
            (   "IsDeleted Valid(1)",   new DesignatedChannelMappingSearchCriteria() { IsDeleted = true },                              new long[] { 1 }            ),
            (   "IsDeleted Valid(2)",   new DesignatedChannelMappingSearchCriteria() { IsDeleted = false },                             new long[] { 2, 3, 4 }      )
        };

    ...
}
```

See [Testing](Testing) for more information.
