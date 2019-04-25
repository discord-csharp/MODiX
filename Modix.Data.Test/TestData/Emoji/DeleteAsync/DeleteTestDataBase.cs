using Modix.Data.Models.Emoji;

namespace Modix.Data.Test.TestData.Emoji.DeleteAsync
{
    internal abstract class DeleteTestDataBase
    {
        public string TestName { get; set; }

        public EmojiSearchCriteria Criteria { get; set; }
    }
}
