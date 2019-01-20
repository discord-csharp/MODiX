using System;

using Modix.Data.Models.Tags;

namespace Modix.Data.Test.TestData.Tags.TryModifyAsync
{
    internal abstract class ModifyTestDataBase
    {
        public string TestName { get; set; }

        public ulong GuildId { get; set; }

        public string TagName { get; set; }

        public ulong ModifiedByUserId { get; set; }

        public Action<TagMutationData> ModifyAction { get; set; }
    }
}
