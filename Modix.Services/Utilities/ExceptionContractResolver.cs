using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class ExceptionContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

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