using UnityEngine;

namespace HitScoreVisualizer.Models;

internal interface IJudgment
{
	int Threshold { get; init; }
	string Text { get; init; }
	Color Color { get; init; }
	bool Fade { get; init; }
}