using Newtonsoft.Json;

namespace HitScoreVisualizer.Models;

public class JudgmentSegment
{
	// This judgment will be applied only when the appropriate part of the swing contributes score >= this number.
	// If no judgment can be applied, the judgment for this segment will be "" (the empty string).
	[JsonProperty("threshold")]
	public required int Threshold { get; init; }

	// The text to replace the appropriate judgment specifier with (%B, %C, %A) when this judgment applies.
	[JsonProperty("text")]
	public required string? Text { get; init; }

	[JsonIgnore] internal static JudgmentSegment Default { get; } = new()
	{
		Threshold = 0,
		Text = "%s"
	};

	[JsonConstructor]
	public JudgmentSegment(int threshold = 0, string? text = null)
	{
		Threshold = threshold;
		Text = text ?? string.Empty;
	}
}