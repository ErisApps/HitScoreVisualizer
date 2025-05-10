using System.Collections.Generic;
using Newtonsoft.Json;

namespace HitScoreVisualizer.Models;

public class ChainHeadJudgment
{
	// This judgment will be applied only to chain note heads hit with score >= this number.
	// Note that if no judgment can be applied to a note, the text will appear as in the unmodded
	// game.
	public required int Threshold { get; init; }

	// The text to display (if judgment text is enabled).
	public required string Text { get; init; }

	// 4 floats, 0-1; red, green, blue, glow (not transparency!)
	// leaving this out should look obviously wrong
	public required List<float> Color { get; init; }

	// If true, the text color will be interpolated between this judgment's color and the previous
	// based on how close to the next threshold it is.
	// Specifying fade : true for the first judgment in the array is an error, and will crash the
	// plugin.
	public bool Fade { get; init; }

	[JsonIgnore]
	internal static ChainHeadJudgment Default { get; } = new()
	{
		Threshold = 0,
		Text = "%s",
		Color = [1f, 1f, 1f, 1f]
	};

	[JsonConstructor]
	public ChainHeadJudgment(int threshold = 0, string? text = null, List<float>? color = null, bool fade = false)
	{
		Threshold = threshold;
		Text = text ?? string.Empty;
		Color = color ?? [0, 0, 0, 0];
		Fade = fade;
	}
}