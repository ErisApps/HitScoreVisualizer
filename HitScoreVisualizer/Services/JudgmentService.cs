using System.Collections.Generic;
using System.Text;
using HitScoreVisualizer.Extensions;
using HitScoreVisualizer.Settings;
using TMPro;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	internal class JudgmentService
	{
		private readonly ConfigProvider _configProvider;

		public JudgmentService(ConfigProvider configProvider)
		{
			_configProvider = configProvider;
		}

		internal void Judge(IReadonlyCutScoreBuffer cutScoreBuffer, ref TextMeshPro text, ref Color color)
		{
			var config = _configProvider.GetCurrentConfig();
			if (config == null)
			{
				return;
			}

			var timeDependence = Mathf.Abs(cutScoreBuffer.noteCutInfo.cutNormal.z);

			// enable rich text
			text.richText = true;
			// disable word wrap, make sure full text displays
			text.enableWordWrapping = false;
			text.overflowMode = TextOverflowModes.Overflow;

			// save in case we need to fade
			var index = config.Judgments!.FindIndex(j => j.Threshold <= cutScoreBuffer.cutScore);
			var judgment = index >= 0 ? config.Judgments[index] : Judgment.Default;

			if (judgment.Fade)
			{
				var fadeJudgment = config.Judgments[index - 1];
				var baseColor = judgment.Color.ToColor();
				var fadeColor = fadeJudgment.Color.ToColor();
				var lerpDistance = Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, cutScoreBuffer.cutScore);
				color = Color.Lerp(baseColor, fadeColor, lerpDistance);
			}
			else
			{
				color = judgment.Color.ToColor();
			}

			text.text = config.DisplayMode switch
			{
				"format" => DisplayModeFormat(cutScoreBuffer, judgment, config),
				"textOnly" => judgment.Text,
				"numeric" => cutScoreBuffer.cutScore.ToString(),
				"scoreOnTop" => $"{cutScoreBuffer.cutScore}\n{judgment.Text}\n",
				_ => $"{judgment.Text}\n{cutScoreBuffer.cutScore}\n"
			};
		}

		// ReSharper disable once CognitiveComplexity
		private static string DisplayModeFormat(IReadonlyCutScoreBuffer cutScoreBuffer, Judgment judgment, Configuration instance)
		{
			return cutScoreBuffer.noteCutInfo.noteData.gameplayType switch
			{
				NoteData.GameplayType.Normal => NormalNoteJudgment(cutScoreBuffer, judgment, instance),
				NoteData.GameplayType.BurstSliderElement => string.Empty,
				_ => cutScoreBuffer.cutScore.ToString(),
			};
		}

		private static string NormalNoteJudgment(IReadonlyCutScoreBuffer cutScoreBuffer, Judgment judgment, Configuration instance)
		{
			var formattedBuilder = new StringBuilder();
			var formatString = judgment.Text;
			var nextPercentIndex = formatString.IndexOf('%');

			var timeDependence = Mathf.Abs(cutScoreBuffer.noteCutInfo.cutNormal.z);

			while (nextPercentIndex != -1)
			{
				formattedBuilder.Append(formatString.Substring(0, nextPercentIndex));
				if (formatString.Length == nextPercentIndex + 1)
				{
					formatString += " ";
				}

				var specifier = formatString[nextPercentIndex + 1];

				switch (specifier)
				{
					case 'b':
						formattedBuilder.Append(cutScoreBuffer.beforeCutScore);
						break;
					case 'c':
						formattedBuilder.Append(cutScoreBuffer.centerDistanceCutScore);
						break;
					case 'a':
						formattedBuilder.Append(cutScoreBuffer.afterCutScore);
						break;
					case 't':
						formattedBuilder.Append(ConvertTimeDependencePrecision(timeDependence, instance.TimeDependenceDecimalOffset, instance.TimeDependenceDecimalPrecision));
						break;
					case 'B':
						formattedBuilder.Append(JudgeSegment(cutScoreBuffer.beforeCutScore, instance.BeforeCutAngleJudgments));
						break;
					case 'C':
						formattedBuilder.Append(JudgeSegment(cutScoreBuffer.centerDistanceCutScore, instance.AccuracyJudgments));
						break;
					case 'A':
						formattedBuilder.Append(JudgeSegment(cutScoreBuffer.afterCutScore, instance.AfterCutAngleJudgments));
						break;
					case 'T':
						formattedBuilder.Append(JudgeTimeDependenceSegment(timeDependence, instance.TimeDependenceJudgments, instance));
						break;
					case 's':
						formattedBuilder.Append(cutScoreBuffer.cutScore);
						break;
					case 'p':
						formattedBuilder.Append($"{(double) cutScoreBuffer.cutScore / cutScoreBuffer.noteScoreDefinition.maxCutScore * 100:0}");
						break;
					case '%':
						formattedBuilder.Append("%");
						break;
					case 'n':
						formattedBuilder.Append("\n");
						break;
					default:
						formattedBuilder.Append("%" + specifier);
						break;
				}

				formatString = formatString.Remove(0, nextPercentIndex + 2);
				nextPercentIndex = formatString.IndexOf('%');
			}

			return formattedBuilder.Append(formatString).ToString();
		}

		private static string JudgeSegment(int scoreForSegment, IList<JudgmentSegment>? judgments)
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

		private static string JudgeTimeDependenceSegment(float scoreForSegment, IList<TimeDependenceJudgmentSegment>? judgments, Configuration instance)
		{
			if (judgments == null)
			{
				return string.Empty;
			}

			foreach (var j in judgments)
			{
				if (scoreForSegment >= j.Threshold)
				{
					return FormatTimeDependenceSegment(j, scoreForSegment, instance);
				}
			}

			return string.Empty;
		}

		private static string FormatTimeDependenceSegment(TimeDependenceJudgmentSegment? judgment, float timeDependence, Configuration instance)
		{
			if (judgment == null)
			{
				return string.Empty;
			}

			var formattedBuilder = new StringBuilder();
			var formatString = judgment.Text ?? string.Empty;
			var nextPercentIndex = formatString.IndexOf('%');
			while (nextPercentIndex != -1)
			{
				formattedBuilder.Append(formatString.Substring(0, nextPercentIndex));
				if (formatString.Length == nextPercentIndex + 1)
				{
					formatString += " ";
				}

				var specifier = formatString[nextPercentIndex + 1];

				switch (specifier)
				{
					case 't':
						formattedBuilder.Append(ConvertTimeDependencePrecision(timeDependence, instance.TimeDependenceDecimalOffset, instance.TimeDependenceDecimalPrecision));
						break;
					case '%':
						formattedBuilder.Append("%");
						break;
					case 'n':
						formattedBuilder.Append("\n");
						break;
					default:
						formattedBuilder.Append("%" + specifier);
						break;
				}

				formatString = formatString.Remove(0, nextPercentIndex + 2);
				nextPercentIndex = formatString.IndexOf('%');
			}

			return formattedBuilder.Append(formatString).ToString();
		}

		private static string ConvertTimeDependencePrecision(float timeDependence, int decimalOffset, int decimalPrecision)
		{
			var multiplier = Mathf.Pow(10, decimalOffset);
			return (timeDependence * multiplier).ToString($"n{decimalPrecision}");
		}
	}
}