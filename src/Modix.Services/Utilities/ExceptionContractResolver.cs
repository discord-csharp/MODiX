using System;
using System.Linq;
using System.Text.Json.Serialization.Metadata;

namespace Modix.Services.Utilities
{


    public class ExampleClass
    {
        public string Name { get; set; } = "";
        public SecretHolder? Secret { get; set; }
    }

    public class SecretHolder
    {
        public string Value { get; set; } = "";
    }

    public class IgnorePropertiesWithType
    {
        private readonly Type[] _ignoredTypes;

        public IgnorePropertiesWithType(params Type[] ignoredTypes)
            => _ignoredTypes = ignoredTypes;

        public void ModifyTypeInfo(JsonTypeInfo ti)
        {
            if (ti.Kind != JsonTypeInfoKind.Object)
                return;

            ti.Properties.RemoveAll(prop => _ignoredTypes.Contains(prop.PropertyType));
        }
    }










    public static class ExceptionJsonTypeInfoResolver
    {
        private static readonly string[] _propertyFilter = new[]
        {
            "Message", "StackTraceString", "InnerException",
            "Source", "ClassName", "StackTrace"
        };

        public static void Modifier(JsonTypeInfo jsonTypeInfo)
        {
            foreach (var jsonPropertyInfo in jsonTypeInfo.Properties)
            {
                if (!typeof(Exception).IsAssignableFrom(jsonPropertyInfo.PropertyType))
                {
                    continue;
                }

                jsonPropertyInfo.ShouldSerialize = (instance, x) =>
                {
                    return false;
                };
            }
        }
    }

    //public class ExceptionContractResolver : DefaultContractResolver
    //{
    //    private static readonly string[] _propertyFilter = new[]
    //    {
    //        "Message", "StackTraceString", "InnerException",
    //        "Source", "ClassName", "StackTrace"
    //    };

    //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    //    {
    //        var property = base.CreateProperty(member, memberSerialization);

    //        //Refer to the default if not an exception
    //        if (!typeof(Exception).IsAssignableFrom(member.ReflectedType))
    //        {
    //            return property;
    //        }

    //        //Only serialize some properties
    //        if (_propertyFilter.Contains(property.PropertyName))
    //        {
    //            property.ShouldSerialize = instance => true;
    //        }
    //        else
    //        {
    //            property.ShouldSerialize = instance => false;
    //        }
            
    //        return property;
    //    }
    //}
}
