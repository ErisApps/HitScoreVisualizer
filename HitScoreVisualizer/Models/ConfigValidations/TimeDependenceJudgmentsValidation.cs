using System.Linq;
using UnityEngine;

namespace HitScoreVisualizer.Models.ConfigValidations;

internal class TimeDependenceJudgmentsValidation : IConfigValidation
{
	public bool IsValid(HsvConfigModel config)
	{
		var judgments = config.TimeDependenceJudgments;
		if (judgments is null || judgments.Count <= 1)
		{
			return true;
		}

		var isOrdered = judgments
			.Zip(judgments.Skip(1), (a, b) => a.Threshold > b.Threshold)
			.All(x => x);

		var hasDuplicate = judgments
			.OrderBy(j => j.Threshold)
			.Zip(judgments.Skip(1), (a, b) => !Mathf.Approximately(a.Threshold, b.Threshold))
			.All(x => x);

		if (!isOrdered)
		{
			Plugin.Log.Warn("Time dependence judgments are not correctly ordered; they should be ordered from highest to lowest threshold");
		}

		if (hasDuplicate)
		{
			Plugin.Log.Warn("Time dependence judgments contain a duplicate threshold");
		}

		return isOrdered && !hasDuplicate;
	}
}