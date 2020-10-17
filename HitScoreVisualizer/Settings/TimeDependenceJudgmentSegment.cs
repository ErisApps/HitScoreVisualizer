using Newtonsoft.Json;

namespace HitScoreVisualizer.Settings
{
	internal class TimeDependenceJudgmentSegment
	{
		[JsonIgnore]
		internal static TimeDependenceJudgmentSegment Default { get; } = new TimeDependenceJudgmentSegment { Threshold = 0, Text = string.Empty };

		// This judgment will be applied only when the time dependence >= this number.
		// If no judgment can be applied, the judgment for this segment will be "" (the empty string).
		[JsonProperty("threshold")]
		public float Threshold { get; set; }

		// The text to replace the appropriate judgment specifier with (%T) when this judgment applies.
		[JsonProperty("text")]
		public string? Text { get; set; }
	}
}