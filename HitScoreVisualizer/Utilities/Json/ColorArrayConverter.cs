using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Json;

public class ColorArrayConverter : JsonConverter
{
	// Converts a 4-decimal array such as [1.0, 1.0, 1.0, 1.0]

	private static Regex ColorRegex { get; } = new(
		@"\[\s*(?<r>\d+(\.?\d+)?)\s*,\s*(?<g>\d+(\.?\d+)?)\s*,\s*(?<b>\d+(\.?\d+)?)\s*,\s*(?<a>\d+(\.?\d+)?)\s*\]",
		RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is not Color c)
		{
			return;
		}

		writer.WriteStartArray();
		writer.WriteValue(c.r);
		writer.WriteValue(c.g);
		writer.WriteValue(c.b);
		writer.WriteValue(c.a);
		writer.WriteEndArray();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var str = serializer.Deserialize(reader)?.ToString();
		if (str is null)
		{
			return objectType == typeof(Color) ? default(Color) : null!;
		}

		var match = ColorRegex.Match(str);

		return match.Success
		       && float.TryParse(match.Groups["r"].Value, out var r)
		       && float.TryParse(match.Groups["g"].Value, out var g)
		       && float.TryParse(match.Groups["b"].Value, out var b)
		       && float.TryParse(match.Groups["a"].Value, out var a)
			? new Color(r, g, b, a)
			: objectType == typeof(Color) ? default(Color) : null!;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Color?) || objectType == typeof(Color);
	}
}