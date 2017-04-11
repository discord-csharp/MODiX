using Newtonsoft.Json;
using System.Collections.Generic;

namespace Modix.Services.StackExchange
{
    [JsonObject]
    public class StackExchangeResponse
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("quota_max")]
        public int QuotaMax { get; set; }

        [JsonProperty("quota_remaining")]
        public int QuotaRemaining { get; set; }

        [JsonProperty("items")]
        public ICollection<StackExchangeResponseItem> Items { get; set; }
    }

    [JsonObject]
    public class StackExchangeResponseItem
    {
        [JsonProperty("tags")]
        public ICollection<string> Tags { get; set; }

        [JsonProperty("owner")]
        public StackExchangeResponseOwner Owner { get; set; }

        [JsonProperty("is_answered")]
        public bool IsAnswered { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("protected_date")]
        public int ProtectedDate { get; set; }

        [JsonProperty("accepted_answer_id")]
        public int AcceptedAnswerId { get; set; }

        [JsonProperty("answer_count")]
        public int AnswerCount { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("last_activity_date")]
        public long LastActivityDate { get; set; }

        [JsonProperty("creation_date")]
        public long CreationDate { get; set; }

        [JsonProperty("last_edit_date")]
        public long LastEditDate { get; set; }

        [JsonProperty("question_id")]
        public int QuestionId { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    [JsonObject]
    public class StackExchangeResponseOwner
    {
        [JsonProperty("reputation")]
        public int Reputation { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        [JsonProperty("accept_rate")]
        public int AcceptRate { get; set; }

        [JsonProperty("profile_image")]
        public string ProfileImage { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
