using System;
using Newtonsoft.Json;

namespace Modix.Services.CodePaste
{
    public class UserCodePaste
    {
        public int Id { get; set; }

        [JsonIgnore] public uint CreatorId { get; set; }

        public string CreatorUsername { get; set; }

        [JsonIgnore] public uint ChannelId { get; set; }

        public string ChannelName { get; set; }

        [JsonIgnore] public uint? MessageId { get; set; }

        public DateTime Created { get; set; }
        public string Content { get; set; }
    }
}