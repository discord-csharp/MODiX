using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modix.Services.Utilities
{
    public class ExceptionContractResolver : DefaultContractResolver
    {
        private static readonly string[] _propertyFilter = new[]
        {
            "Message", "StackTraceString", "InnerException",
            "Source", "ClassName", "StackTrace"
        };

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            //Refer to the default if not an exception
            if (!typeof(Exception).IsAssignableFrom(member.ReflectedType))
            {
                return property;
            }

            //Only serialize some properties
            if (_propertyFilter.Contains(property.PropertyName))
            {
                property.ShouldSerialize = instance => true;
            }
            else
            {
                property.ShouldSerialize = instance => false;
            }
            
            return property;
        }
    }
}
