using System.Collections.Generic;
using Newtonsoft.Json;

namespace HitScoreVisualizer.Settings
{
	[method: JsonConstructor]
	public class ChainLinkDisplay(string? text = null, List<float>? color = null)
	{
		[JsonIgnore]
		internal static ChainLinkDisplay Default { get; } = new();

		// The text to display for a chain segment
		[JsonProperty("text")]
		public string? Text = text ?? string.Empty;

		// 4 floats, 0-1; red, green, blue, glow (not transparency!)
		// leaving this out should look obviously wrong
		[JsonProperty("color")]
		public List<float> Color = color ?? [];
	}
}