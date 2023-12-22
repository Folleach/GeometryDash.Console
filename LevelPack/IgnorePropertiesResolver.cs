using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LevelPack;

// taken from: https://stackoverflow.com/questions/10169648/how-to-exclude-property-from-json-serialization
public class IgnorePropertiesResolver<T> : DefaultContractResolver
{
    private readonly HashSet<MemberInfo> ignoreProperties;

    public IgnorePropertiesResolver(params Expression<Func<T, object>>[] ignores)
    {
        ignoreProperties = ignores
            .Select(x => x.Body)
            .Cast<MemberExpression>()
            .Select(x => x.Member)
            .ToHashSet();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (ignoreProperties.Contains(member))
            property.ShouldSerialize = _ => false;
        return property;
    }
}
