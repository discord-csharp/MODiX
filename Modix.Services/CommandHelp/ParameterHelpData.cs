using Discord.Commands;

namespace Modix.Services.CommandHelp
{
    public class ParameterHelpData
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Type { get; set; }
        public bool IsOptional { get; set; }

        public string[] Options { get; set; }

        public static ParameterHelpData FromParameterInfo(ParameterInfo parameter)
        {
            var ret = new ParameterHelpData
            {
                Name = parameter.Name,
                Summary = parameter.Summary,
                Type = parameter.Type.Name,
                IsOptional = parameter.IsOptional,
                Options = parameter.Type.IsEnum ? parameter.Type.GetEnumNames() : new string[0]
            };

            return ret;
        }
    }
}
