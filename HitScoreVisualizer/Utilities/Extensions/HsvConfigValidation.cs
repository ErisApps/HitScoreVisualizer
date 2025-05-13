using System;
using System.Collections.Generic;
using System.Linq;
using HitScoreVisualizer.Models;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class HsvConfigValidation
{
	public static bool Validate(this HsvConfigModel configuration)
	{
		if (!configuration.Judgments.Any() || !ValidateJudgments(configuration))
		{
			return false;
		}

		// 99 is the max for NumberFormatInfo.NumberDecimalDigits
		if (configuration.TimeDependenceDecimalPrecision is < 0 or > 99)
		{
			Plugin.Log.Warn($"timeDependencyDecimalPrecision value {configuration.TimeDependenceDecimalPrecision} is outside the range of acceptable values [0, 99]");
			return false;
		}

		if (configuration.TimeDependenceDecimalOffset < 0 || configuration.TimeDependenceDecimalOffset > Math.Log10(float.MaxValue))
		{
			Plugin.Log.Warn($"timeDependencyDecimalOffset value {configuration.TimeDependenceDecimalOffset} is outside the range of acceptable values [0, {(int) Math.Log10(float.MaxValue)}]");
			return false;
		}

		if (configuration.BeforeCutAngleJudgments != null)
		{
			configuration.BeforeCutAngleJudgments = configuration.BeforeCutAngleJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.BeforeCutAngleJudgments))
			{
				return false;
			}
		}

		if (configuration.AccuracyJudgments != null)
		{
			configuration.AccuracyJudgments = configuration.AccuracyJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.AccuracyJudgments))
			{
				return false;
			}
		}

		if (configuration.AfterCutAngleJudgments != null)
		{
			configuration.AfterCutAngleJudgments = configuration.AfterCutAngleJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.AfterCutAngleJudgments))
			{
				return false;
			}
		}

		if (configuration.TimeDependenceJudgments != null)
		{
			configuration.TimeDependenceJudgments = configuration.TimeDependenceJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateTimeDependenceJudgmentSegment(configuration.TimeDependenceJudgments))
			{
				return false;
			}
		}

		return true;
	}

	private static bool ValidateJudgments(HsvConfigModel configuration)
	{
		configuration.Judgments = configuration.Judgments.OrderByDescending(x => x.Threshold).ToList();
		var prevJudgment = configuration.Judgments[0];
		if (prevJudgment.Fade)
		{
			prevJudgment = new()
			{
				Color = prevJudgment.Color,
				Fade = false,
				Text = prevJudgment.Text,
				Threshold = prevJudgment.Threshold,
			};
		}

		if (!ValidateJudgmentColor(prevJudgment))
		{
			Plugin.Log.Warn($"Judgment entry for threshold {prevJudgment.Threshold} has invalid color");
			return false;
		}

		if (configuration.Judgments.Count > 1)
		{
			for (var i = 1; i < configuration.Judgments.Count; i++)
			{
				var currentJudgment = configuration.Judgments[i];
				if (prevJudgment.Threshold != currentJudgment.Threshold)
				{
					if (!ValidateJudgmentColor(currentJudgment))
					{
						Plugin.Log.Warn($"Judgment entry for threshold {currentJudgment.Threshold} has invalid color");
						return false;
					}

					prevJudgment = currentJudgment;
					continue;
				}

				Plugin.Log.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold}");
				return false;
			}
		}

		return true;
	}

	private static bool ValidateJudgmentColor(NormalJudgment judgment)
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

	private static bool ValidateJudgmentSegment(List<JudgmentSegment> segments)
	{
		if (segments.Count <= 1)
		{
			return true;
		}

		var prevJudgmentSegment = segments.First();
		for (var i = 1; i < segments.Count; i++)
		{
			var currentJudgment = segments[i];
			if (prevJudgmentSegment.Threshold != currentJudgment.Threshold)
			{
				prevJudgmentSegment = currentJudgment;
				continue;
			}

			Plugin.Log.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold}");
			return false;
		}

		return true;
	}

	private static bool ValidateTimeDependenceJudgmentSegment(List<TimeDependenceJudgmentSegment> segments)
	{
		if (segments.Count <= 1)
		{
			return true;
		}

		var prevJudgmentSegment = segments.First();
		for (var i = 1; i < segments.Count; i++)
		{
			var currentJudgment = segments[i];
			if (prevJudgmentSegment.Threshold - currentJudgment.Threshold > double.Epsilon)
			{
				prevJudgmentSegment = currentJudgment;
				continue;
			}

			Plugin.Log.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold}");
			return false;
		}

		return true;
	}
}