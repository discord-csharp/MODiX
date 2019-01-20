using System;
using System.Collections.Generic;

using Modix.Data.Test.TestData.Tags.TryModifyAsync;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Modifications
    {
        public static readonly IEnumerable<ExceptionModifyTestData> ExceptionModifications
            = new[]
            {
                new ExceptionModifyTestData()
                {
                    TestName = "Null name",
                    GuildId = 1,
                    TagName = null,
                    ModifiedByUserId = 1,
                    ModifyAction = _ => { },
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionModifyTestData()
                {
                    TestName = "Empty name",
                    GuildId = 1,
                    TagName = "",
                    ModifiedByUserId = 1,
                    ModifyAction = _ => { },
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionModifyTestData()
                {
                    TestName = "Whitespace name",
                    GuildId = 1,
                    TagName = " \r\n\t",
                    ModifiedByUserId = 1,
                    ModifyAction = _ => { },
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionModifyTestData()
                {
                    TestName = "Null action",
                    GuildId = 1,
                    TagName = "created",
                    ModifiedByUserId = 1,
                    ModifyAction = null,
                    ExceptionType = typeof(ArgumentNullException),
                },
            };

        public static readonly IEnumerable<NonexistentModifyTestData> NonexistentModifications
            = new[]
            {
                new NonexistentModifyTestData()
                {
                    TestName = "Nonexistent name",
                    GuildId = 1,
                    TagName = "NONEXISTENTNAME",
                    ModifiedByUserId = 1,
                    ModifyAction = _ => { },
                },
                new NonexistentModifyTestData()
                {
                    TestName = "Wrong guild",
                    GuildId = 42,
                    TagName = "created",
                    ModifiedByUserId = 1,
                    ModifyAction = _ => { },
                },
            };
        
        public static readonly IEnumerable<ValidModifyTestData> ValidModifications
            = new[]
            {
                new ValidModifyTestData()
                {
                    TestName = "Modify content in guild 1",
                    GuildId = 1,
                    TagName = "created",
                    ModifiedByUserId = 1,
                    ModifyAction = x => x.Content = "NewContent",
                    Predicate = (newTag, oldTag)
                        => newTag.Content == "NewContent"
                        && !(newTag.CreateAction is null)
                        && newTag.DeleteAction is null
                        && newTag.GuildId == oldTag.GuildId
                        && newTag.Id != oldTag.Id
                        && newTag.Name == oldTag.Name
                        && newTag.Uses == oldTag.Uses,
                },
                new ValidModifyTestData()
                {
                    TestName = "Modify content in guild 2",
                    GuildId = 2,
                    TagName = "sametagacrossguilds",
                    ModifiedByUserId = 3,
                    ModifyAction = x => x.Content = "NewContent",
                    Predicate = (newTag, oldTag)
                        => newTag.Content == "NewContent"
                        && !(newTag.CreateAction is null)
                        && newTag.DeleteAction is null
                        && newTag.GuildId == oldTag.GuildId
                        && newTag.Id != oldTag.Id
                        && newTag.Name == oldTag.Name
                        && newTag.Uses == oldTag.Uses,
                },
            };
    }
}
