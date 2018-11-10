using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modix.Services.Utilities
{
    public class ExceptionContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            //Skip serializing complex properties
            if ((property.DeclaringType == typeof(MethodBase) && property.PropertyName == "TargetSite") 
                || property.PropertyName == "Context"
                || property.PropertyName == "Command")
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