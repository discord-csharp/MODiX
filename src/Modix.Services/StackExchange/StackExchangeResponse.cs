using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Modix.Services.StackExchange
{
    public class StackExchangeResponse
    {
        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }

        [JsonPropertyName("quota_max")]
        public int QuotaMax { get; set; }

        [JsonPropertyName("quota_remaining")]
        public int QuotaRemaining { get; set; }

        [JsonPropertyName("items")]
        public ICollection<StackExchangeResponseItem> Items { get; set; }
    }

    public class StackExchangeResponseItem
    {
        [JsonPropertyName("tags")]
        public ICollection<string> Tags { get; set; }

        [JsonPropertyName("owner")]
        public StackExchangeResponseOwner Owner { get; set; }

        [JsonPropertyName("is_answered")]
        public bool IsAnswered { get; set; }

        [JsonPropertyName("view_count")]
        public int ViewCount { get; set; }

        [JsonPropertyName("protected_date")]
        public int ProtectedDate { get; set; }

        [JsonPropertyName("accepted_answer_id")]
        public int AcceptedAnswerId { get; set; }

        [JsonPropertyName("answer_count")]
        public int AnswerCount { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("last_activity_date")]
        public long LastActivityDate { get; set; }

        [JsonPropertyName("creation_date")]
        public long CreationDate { get; set; }

        [JsonPropertyName("last_edit_date")]
        public long LastEditDate { get; set; }

        [JsonPropertyName("question_id")]
        public int QuestionId { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class StackExchangeResponseOwner
    {
        [JsonPropertyName("reputation")]
        public int Reputation { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("user_type")]
        public string UserType { get; set; }

        [JsonPropertyName("accept_rate")]
        public int AcceptRate { get; set; }

        [JsonPropertyName("profile_image")]
        public string ProfileImage { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }
    }
}
