using Modix.Data.Models.Core;

namespace Modix.Services.MessageContentPatterns
{
    public class MessageContentPatternDto
    {
        public MessageContentPatternDto(string pattern, MessageContentPatternType patternType)
        {
            Pattern = pattern;
            Type = patternType;
        }

        public string Pattern { get; }
        public MessageContentPatternType Type { get; }
    }
}
