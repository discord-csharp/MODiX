namespace Modix.Data.Test.TestData.Tags.TryIncrementAsync
{
    internal abstract class IncrementTestDataBase
    {
        public string TestName { get; set; } = null!;

        public ulong GuildId { get; set; }

        public string? TagName { get; set; }
    }
}
