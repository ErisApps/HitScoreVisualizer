using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HitScoreVisualizer.Helpers.Json
{
	internal class ShouldNotSerializeContractResolver : DefaultContractResolver
	{
		internal static readonly ShouldNotSerializeContractResolver Instance = new ShouldNotSerializeContractResolver();

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (property.AttributeProvider?.GetAttributes(true).OfType<ShouldNotSerializeAttribute>().Any() ?? false)
			{
				property.ShouldSerialize = _ => false;
			}

			return property;
		}
	}
}