using System;

namespace HitScoreVisualizer.Models;

[Serializable]
public class TimeDependenceJudgmentSegment
{
	// This judgment will be applied only when the time dependence >= this number.
	// If no judgment can be applied, the judgment for this segment will be "" (the empty string).
	public required float Threshold { get; init; }

	// The text to replace the appropriate judgment specifier with (%T) when this judgment applies.
	public required string Text { get; init; }
}