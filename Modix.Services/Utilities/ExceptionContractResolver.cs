using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modix.Services.Utilities
{
    public class ExceptionContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(MethodBase) && property.PropertyName == "TargetSite")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        return false;
                    };
            }
            return property;
        }
    }
}