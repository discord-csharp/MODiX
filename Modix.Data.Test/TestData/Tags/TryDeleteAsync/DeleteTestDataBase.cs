namespace Modix.Data.Test.TestData.Tags.TryDeleteAsync
{
    internal abstract class DeleteTestDataBase
    {
        public string TestName { get; set; }

        public ulong GuildId { get; set; }

        public string TagName { get; set; }

        public ulong DeletedByUserId { get; set; }
    }
}
