using System;
using UnityEngine;

namespace HitScoreVisualizer.Models;

[Serializable]
public class BadCutDisplay
{
	public required string Text { get; init; }

	public required Color Color { get; init; }

	public BadCutDisplayType? Type { get; init; } = BadCutDisplayType.All;
}