using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HitScoreVisualizer.Models;

[Serializable]
public class BadCutDisplay
{
	public required string Text { get; init; }

	public required List<float> Color { get; init; }

	public BadCutDisplayType? Type { get; init; } = BadCutDisplayType.All;
}