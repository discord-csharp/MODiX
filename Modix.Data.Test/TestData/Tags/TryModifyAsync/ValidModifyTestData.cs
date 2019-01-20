using System;

using Modix.Data.Models.Tags;

namespace Modix.Data.Test.TestData.Tags.TryModifyAsync
{
    internal class ValidModifyTestData : ModifyTestDataBase
    {
        /// <summary>
        /// A predicate that determines whether the tag has been changed by the operation.
        /// Arguments should be passed with the new tag's summary as the first argument and
        /// the old/original tag's summary as the second argument.
        /// </summary>
        public Func<TagSummary, TagSummary, bool> Predicate { get; set; }
    }
}
