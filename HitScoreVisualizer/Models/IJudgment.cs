using System.Collections.Generic;

namespace HitScoreVisualizer.Models;

internal interface IJudgment
{
	int Threshold { get; init; }
	string Text { get; init; }
	List<float> Color { get; init; }
	bool Fade { get; init; }
}