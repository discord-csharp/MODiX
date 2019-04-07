using System;

namespace Modix.Services.CommandHelp
{
    /// <summary>
    /// Indicates tags to use during help searches to increase the hit rate of the module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HelpTagsAttribute : Attribute
    {
        public HelpTagsAttribute(params string[] tags)
        {
            Tags = tags;
        }

        public string[] Tags { get; }
    }
}
