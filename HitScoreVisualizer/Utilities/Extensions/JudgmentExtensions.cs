using System.Collections.Generic;
using System.Text;
using HitScoreVisualizer.Settings;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class JudgmentExtensions
{
	public static string JudgeSegment(this IList<JudgmentSegment>? judgments, int scoreForSegment)
	{
		if (judgments == null)
		{
			return string.Empty;
		}

		foreach (var j in judgments)
		{
			if (scoreForSegment >= j.Threshold)
			{
				return j.Text ?? string.Empty;
			}
		}

		return string.Empty;
	}

	public static string JudgeTimeDependenceSegment(this IList<TimeDependenceJudgmentSegment>? judgments, float scoreForSegment, int tdDecimalOffset, int tdDecimalPrecision)
	{
		if (judgments == null)
		{
			return string.Empty;
		}

		foreach (var j in judgments)
		{
			if (scoreForSegment >= j.Threshold)
			{
				return j.Text != null
					? FormatTimeDependenceSegment(j.Text, scoreForSegment, tdDecimalOffset, tdDecimalPrecision)
					: string.Empty;
			}
		}

		return string.Empty;
	}

	private static string FormatTimeDependenceSegment(string unformattedText, float timeDependence, int tdDecimalOffset, int tdDecimalPrecision)
	{
		var builder = new StringBuilder();
		var nextPercentIndex = unformattedText.IndexOf('%');
		while (nextPercentIndex != -1)
		{
			builder.Append(unformattedText.Substring(0, nextPercentIndex));
			if (unformattedText.Length == nextPercentIndex + 1)
			{
				unformattedText += " ";
			}

			var specifier = unformattedText[nextPercentIndex + 1];

			switch (specifier)
			{
				case 't':
					builder.Append(ConvertTimeDependencePrecision(timeDependence, tdDecimalOffset, tdDecimalPrecision));
					break;
				case '%':
					builder.Append("%");
					break;
				case 'n':
					builder.Append("\n");
					break;
				default:
					builder.Append("%" + specifier);
					break;
			}

			unformattedText = unformattedText.Remove(0, nextPercentIndex + 2);
			nextPercentIndex = unformattedText.IndexOf('%');
		}

		return builder.Append(unformattedText).ToString();
	}

	private static string ConvertTimeDependencePrecision(float timeDependence, int decimalOffset, int decimalPrecision)
	{
		return (timeDependence * Mathf.Pow(10, decimalOffset)).ToString($"n{decimalPrecision}");
	}
}