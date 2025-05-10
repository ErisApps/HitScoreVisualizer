using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HitScoreVisualizer.Utilities.Json;

/// <summary>
/// Contract resolver used when serializing/deserializing HSV config files.
/// Ensures that properties are serialized in camelCase.
/// Ensures that [Obsolete] properties are not serialized.
/// </summary>
internal class HsvConfigContractResolver : CamelCasePropertyNamesContractResolver
{
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		var property = base.CreateProperty(member, memberSerialization);

		if (property.AttributeProvider is null)
		{
			return property;
		}

		if (property.AttributeProvider.GetAttributes(true).OfType<ObsoleteAttribute>().Any())
		{
			property.ShouldSerialize = _ => false;
		}

		return property;
	}
}