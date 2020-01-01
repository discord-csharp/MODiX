namespace Modix.Data.Test.TestData.Tags.ReadSummaryAsync
{
    internal abstract class ReadTestDataBase
    {
        public string TestName { get; set; } = null!;

        public ulong GuildId { get; set; }

        public string TagName { get; set; } = null!;
    }
}
