using System;

namespace Modix.Services.CommandHelp
{
    /// <summary>
    /// Information to be displayed in help documentation related to the module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleHelpAttribute : Attribute
    {
        public ModuleHelpAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }

        public string Description { get; }
    }
}
