using System.Collections.Generic;
using Newtonsoft.Json;

namespace HitScoreVisualizer.Models;

public class ChainLinkDisplay
{
	[JsonConstructor]
	public ChainLinkDisplay(string? text = null, List<float>? color = null)
	{
		Text = text ?? string.Empty;
		Color = color ?? [0, 0, 0, 0];
	}

	[JsonIgnore]
	internal static ChainLinkDisplay Default { get; } = new("<u>20", [1, 1, 1, 1]);

	// The text to display for a chain segment
	[JsonProperty("text")]
	public string Text { get; }

	// 4 floats, 0-1; red, green, blue, glow (not transparency!)
	// leaving this out should look obviously wrong
	[JsonProperty("color")]
	public List<float> Color { get; }
}