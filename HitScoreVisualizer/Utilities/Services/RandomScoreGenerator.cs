using System;
using HitScoreVisualizer.Models;
using UnityEngine;
using Random = System.Random;

namespace HitScoreVisualizer.Utilities.Services;

internal class RandomScoreGenerator
{
	private readonly Random random;

	public RandomScoreGenerator(Random random)
	{
		this.random = random;
	}

	public JudgmentDetails GetRandomScore()
	{
		const int beforeMax = 70;
		const int centerMax = 15;
		const int afterMax = 30;
		const int max = beforeMax + centerMax + afterMax;
		var before = GetWeightedScoreExponential((float)random.NextDouble() * beforeMax, beforeMax);
		var center = GetWeightedScoreCircle((float)random.NextDouble() * centerMax, centerMax);
		var after = GetWeightedScoreExponential((float)random.NextDouble() * afterMax, afterMax);
		return new()
		{
			BeforeCutScore = before,
			CenterCutScore = center,
			AfterCutScore = after,
			MaxPossibleScore = max,
			TotalCutScore = before + center + after,
			CutInfo = DummyScores.Normal
		};
	}

	private static int GetWeightedScoreCircle(float x, int max)
	{
		return x >= max ? max
			: x <= 0 ? 0
			: Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(max, 2) - MathF.Pow(x - max, 2)));
	}

	private static int GetWeightedScoreExponential(float x, int max)
	{
		return x >= max ? max
			: x <= 0 ? 0
			: Mathf.Clamp(Mathf.RoundToInt(max / 2f * Mathf.Pow(-x - 1, -1) + max + 1), 0, max);
	}
}