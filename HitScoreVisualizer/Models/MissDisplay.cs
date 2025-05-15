using System.Collections.Generic;

namespace HitScoreVisualizer.Models;

public class MissDisplay
{
	public required string Text { get; init; }

	public required List<float> Color { get; init; }
}