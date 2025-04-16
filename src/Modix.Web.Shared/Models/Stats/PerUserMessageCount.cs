namespace Modix.Web.Shared.Models.Stats;

public record PerUserMessageCount(string Username, string Discriminator, int Rank, int MessageCount, bool IsCurrentUser);
