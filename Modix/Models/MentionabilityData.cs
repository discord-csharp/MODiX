using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;

using Modix.Data.Models.Mentions;

namespace Modix.Models
{
    public class MentionabilityData
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public static Dictionary<MentionabilityType, MentionabilityData> GetMentionabilityTypes()
            => LazyInitializer.EnsureInitialized(ref _cachedMentionabilityTypeData, ()
                => typeof(MentionabilityType).GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary
                (
                    x => (MentionabilityType)x.GetValue(null),
                    x =>
                    {
                        var description = x.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();

                        return new MentionabilityData
                        {
                            Name = x.Name,
                            Description = description?.Description ?? x.Name,
                        };
                    }
                ));

        private static Dictionary<MentionabilityType, MentionabilityData> _cachedMentionabilityTypeData;
    }
}
