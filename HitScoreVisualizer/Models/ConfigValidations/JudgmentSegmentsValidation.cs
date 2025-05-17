using System;
using System.Collections.Generic;
using System.Linq;

namespace HitScoreVisualizer.Models.ConfigValidations;

internal class JudgmentSegmentsValidation : IConfigValidation
{
	private readonly Func<HsvConfigModel, List<JudgmentSegment>?> propertyGetter;

	public JudgmentSegmentsValidation(Func<HsvConfigModel, List<JudgmentSegment>?> propertyGetter)
	{
		this.propertyGetter = propertyGetter;
	}

	public bool IsValid(HsvConfigModel config)
	{
		var segments = propertyGetter(config);
		if (segments is null || segments.Count <= 1)
		{
			return true;
		}

		var isOrdered = segments
			.Zip(segments.Skip(1), (a, b) => a.Threshold > b.Threshold)
			.All(x => x);

		if (!isOrdered)
		{
			Plugin.Log.Warn("Judgment segments are not correctly ordered; they should be ordered from highest to lowest threshold");
			return false;
		}

		var hasDuplicate = segments
			.Zip(segments.Skip(1), (a, b) => a.Threshold == b.Threshold)
			.Contains(true);

		if (hasDuplicate)
		{
			Plugin.Log.Warn("Judgment segments contain a duplicate threshold");
			return false;
		}

		return true;
	}
}