using Newtonsoft.Json;

namespace HitScoreVisualizer.Settings;

public class JudgmentSegment
{
	[JsonConstructor]
	public JudgmentSegment(int threshold = 0, string? text = null)
	{
		Threshold = threshold;
		Text = text ?? string.Empty;
	}

	[JsonIgnore]
	internal static JudgmentSegment Default { get; } = new(0, string.Empty);

	// This judgment will be applied only when the appropriate part of the swing contributes score >= this number.
	// If no judgment can be applied, the judgment for this segment will be "" (the empty string).
	[JsonProperty("threshold")]
	public int Threshold { get; }

	// The text to replace the appropriate judgment specifier with (%B, %C, %A) when this judgment applies.
	[JsonProperty("text")]
	public string? Text { get; }
}