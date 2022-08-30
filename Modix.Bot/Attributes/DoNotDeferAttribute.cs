using System;

namespace Modix.Bot.Attributes
{
    /// <summary>
    /// Indicates that the command should not be deferred,
    /// e.g. if we need to respond with a modal instead of with a message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DoNotDeferAttribute : Attribute
    {
    }
}
