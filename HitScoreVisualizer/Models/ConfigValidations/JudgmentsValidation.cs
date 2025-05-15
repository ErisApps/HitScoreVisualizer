using System;
using System.Collections.Generic;
using System.Linq;

namespace HitScoreVisualizer.Models.ConfigValidations;

internal class JudgmentsValidation : IConfigValidation
{
	private readonly Func<HsvConfigModel, IEnumerable<IJudgment>> propertyGetter;

	public JudgmentsValidation(Func<HsvConfigModel, IEnumerable<IJudgment>> propertyGetter)
	{
		this.propertyGetter = propertyGetter;
	}

	public bool IsValid(HsvConfigModel config)
	{
		var judgments = propertyGetter(config).ToList();
		if (judgments is [])
		{
			Plugin.Log.Warn("Config contains no Judgments when it should specify at least one Judgment");
			return false;
		}

		var isOrdered = judgments
			.Zip(judgments.Skip(1), (a, b) => a.Threshold > b.Threshold)
			.All(x => x);

		if (!isOrdered)
		{
			Plugin.Log.Warn("Judgments are not correctly ordered; they should be ordered from highest to lowest threshold");
			return false;
		}

		var isHighestJudgmentValid = !judgments.First().Fade;

		var hasDuplicates = judgments.Count > 1 && judgments
			.OrderBy(x => x.Threshold)
			.Zip(judgments.Skip(1), (a, b) => a.Threshold != b.Threshold)
			.All(x => x);

		var areColorsValid = judgments.All(IsJudgmentColorValid);

		if (!isHighestJudgmentValid)
		{
			Plugin.Log.Warn("The first judgment cannot have fade set to true");
		}

		if (hasDuplicates)
		{
			Plugin.Log.Warn("Judgments contain a duplicate threshold");
		}

		if (!areColorsValid)
		{
			Plugin.Log.Warn("One or more Judgments contain an invalid color");
		}

		return isHighestJudgmentValid && !hasDuplicates && areColorsValid;
	}

	private static bool IsJudgmentColorValid(IJudgment judgment)
	{
		if (judgment.Color.Count != 4)
		{
			Plugin.Log.Warn($"Judgment for threshold {judgment.Threshold} has invalid color. Make sure to include exactly 4 numbers for each judgment's color!");
			return false;
		}

		if (judgment.Color.All(x => x >= 0f))
		{
			return true;
		}

		Plugin.Log.Warn($"Judgment for threshold {judgment.Threshold} has invalid color. Make sure to include exactly 4 numbers that are greater or equal than 0 (and preferably smaller or equal than 1) for each judgment's color!");
		return false;
	}
}