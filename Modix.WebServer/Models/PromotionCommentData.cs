using System;
using System.Collections.Generic;
using System.Text;
using Modix.Services.Promotions;

namespace Modix.WebServer.Models
{
    public class PromotionCommentData
    {
        public string Body { get; set; }
        public PromotionSentiment Sentiment { get; set; }
    }
}
