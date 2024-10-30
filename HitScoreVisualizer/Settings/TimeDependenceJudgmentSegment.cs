using Newtonsoft.Json;

namespace HitScoreVisualizer.Settings
{
	public class TimeDependenceJudgmentSegment
	{
		[JsonConstructor]
		public TimeDependenceJudgmentSegment(float threshold = 0f, string? text = null)
		{
			Threshold = threshold;
			Text = text ?? string.Empty;
		}

		[JsonIgnore]
		internal static TimeDependenceJudgmentSegment Default { get; } = new(0, string.Empty);

		// This judgment will be applied only when the time dependence >= this number.
		// If no judgment can be applied, the judgment for this segment will be "" (the empty string).
		[JsonProperty("threshold")]
		public float Threshold { get; }

		// The text to replace the appropriate judgment specifier with (%T) when this judgment applies.
		[JsonProperty("text")]
		public string? Text { get; }
	}
}