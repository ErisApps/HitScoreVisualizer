using System;
using Newtonsoft.Json;
using UnityEngine;

namespace HitScoreVisualizer.Models;

[Serializable]
public class ChainLinkDisplay
{
	// The text to display for a chain segment
	public required string Text { get; set; }

	// 4 floats, 0-1; red, green, blue, glow (not transparency!)
	// leaving this out should look obviously wrong
	public required Color Color { get; set; }

	[JsonIgnore]
	internal static ChainLinkDisplay Default { get; } = new()
	{
		Text = "<u>20</u>",
		Color = Color.white
	};
}