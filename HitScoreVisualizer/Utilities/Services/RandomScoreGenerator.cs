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

	private static SaberMovementData DummySaberMovementData { get; } = new();

	private static NoteCutInfo? dummyNormalNote;
	private static NoteData DummyNormalNoteData { get; } = NoteData.CreateBasicNoteData(0, 0, 0, 0, 0, 0, 0);
	public static NoteCutInfo DummyNormalNote => dummyNormalNote ??= new(
		DummyNormalNoteData,
		true, true, true, false, 0, Vector3.zero, SaberType.SaberA, 0, 0, Vector3.zero, Vector3.zero, 0, 0, Quaternion.identity, Quaternion.identity, Quaternion.identity, Vector3.zero,
		DummySaberMovementData);

	private static NoteCutInfo? dummyChainNote;
	private static NoteData DummyChainNoteData { get; } = new(0, 0, 0, 0, 0, 0, NoteData.GameplayType.BurstSliderHead, NoteData.ScoringType.ChainHead, 0, 0, 0, 0, 0, 0, 0, 0);
	public static NoteCutInfo DummyChainNote => dummyChainNote ??= new(
		DummyChainNoteData,
		true, true, true, false, 0, Vector3.zero, SaberType.SaberA, 0, 0, Vector3.zero, Vector3.zero, 0, 0, Quaternion.identity, Quaternion.identity, Quaternion.identity, Vector3.zero,
		DummySaberMovementData);

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
			CutInfo = DummyNormalNote
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