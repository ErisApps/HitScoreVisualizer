using System.Collections.Generic;
using System.Text;
using HitScoreVisualizer.Extensions;
using HitScoreVisualizer.Settings;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	internal class JudgmentService(ConfigProvider configProvider)
	{
		private readonly ConfigProvider configProvider = configProvider;


		private Configuration Config => configProvider.CurrentConfig ?? Configuration.Default;

		internal (string hitScoreText, Color hitScoreColor) Judge(IReadonlyCutScoreBuffer cutScoreBuffer)
		{
			return cutScoreBuffer.noteCutInfo.noteData.gameplayType switch
			{
				NoteData.GameplayType.Normal => JudgeNormal(cutScoreBuffer, Config),
				NoteData.GameplayType.BurstSliderHead => JudgeChainHead(cutScoreBuffer, Config),
				NoteData.GameplayType.BurstSliderElement => ChainSegmentDisplay(Config),
				_ => (string.Empty, Color.white),
			};
		}

		private static (string, Color) ChainSegmentDisplay(Configuration config)
		{
			var color = config.ChainLinkDisplay?.Color.ToColor() ?? ChainLinkDisplay.Default.Color.ToColor();
			var text = config.ChainLinkDisplay?.Text ?? string.Empty;
			return (text, color);
		}

		private (string, Color) JudgeNormal(IReadonlyCutScoreBuffer cutScoreBuffer, Configuration config)
		{
			config.NormalJudgments ??= [];
			NormalJudgment? judgment = null;
			NormalJudgment? fadeJudgment = null;

			for (var i = 0; i < config.NormalJudgments.Count; i++)
			{
				if (config.NormalJudgments[i].Threshold <= cutScoreBuffer.cutScore)
				{
					judgment = config.NormalJudgments[i];
					fadeJudgment = i > 0
						? config.NormalJudgments[i - 1]
						: NormalJudgment.Default;
					break;
				}
			}

			judgment ??= NormalJudgment.Default;

			Color color;
			if (judgment.Fade)
			{
				fadeJudgment ??= NormalJudgment.Default;
				var lerpDistance = Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, cutScoreBuffer.cutScore);
				color = Color.Lerp(judgment.Color.ToColor(), fadeJudgment.Color.ToColor(), lerpDistance);
			}
			else
			{
				color = judgment.Color.ToColor();
			}

			var text = config.DisplayMode switch
			{
				"format" => NormalNoteFormat(cutScoreBuffer, judgment, config),
				"textOnly" => judgment.Text,
				"numeric" => cutScoreBuffer.cutScore.ToString(),
				"scoreOnTop" => $"{cutScoreBuffer.cutScore}\n{judgment.Text}\n",
				_ => $"{judgment.Text}\n{cutScoreBuffer.cutScore}\n"
			};

			return (text, color);
		}

		private static (string, Color) JudgeChainHead(IReadonlyCutScoreBuffer cutScoreBuffer, Configuration config)
		{
			config.ChainHeadJudgments ??= [];
			ChainHeadJudgment? judgment = null;
			ChainHeadJudgment? fadeJudgment = null;

			for (var i = 0; i < config.ChainHeadJudgments.Count; i++)
			{
				if (config.ChainHeadJudgments[i].Threshold <= cutScoreBuffer.cutScore)
				{
					judgment = config.ChainHeadJudgments[i];
					fadeJudgment = i > 0
						? config.ChainHeadJudgments[i - 1]
						: ChainHeadJudgment.Default;
					break;
				}
			}

			judgment ??= ChainHeadJudgment.Default;

			Color color;
			if (judgment.Fade)
			{
				fadeJudgment ??= ChainHeadJudgment.Default;
				var lerpDistance = Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, cutScoreBuffer.cutScore);
				color = Color.Lerp(judgment.Color.ToColor(), fadeJudgment.Color.ToColor(), lerpDistance);
			}
			else
			{
				color = judgment.Color.ToColor();
			}

			var text = config.DisplayMode switch
			{
				"format" => ChainHeadFormat(cutScoreBuffer, judgment, config),
				"textOnly" => judgment.Text,
				"numeric" => cutScoreBuffer.cutScore.ToString(),
				"scoreOnTop" => $"{cutScoreBuffer.cutScore}\n{judgment.Text}\n",
				_ => $"{judgment.Text}\n{cutScoreBuffer.cutScore}\n"
			};

			return (text, color);
		}

		private static string NormalNoteFormat(IReadonlyCutScoreBuffer cutScoreBuffer, NormalJudgment judgment, Configuration instance)
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

		private static string ChainHeadFormat(IReadonlyCutScoreBuffer cutScoreBuffer, ChainHeadJudgment judgment, Configuration instance)
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
					case 't':
						formattedBuilder.Append(ConvertTimeDependencePrecision(timeDependence, instance.TimeDependenceDecimalOffset, instance.TimeDependenceDecimalPrecision));
						break;
					case 'B':
						formattedBuilder.Append(JudgeSegment(cutScoreBuffer.beforeCutScore, instance.BeforeCutAngleJudgments));
						break;
					case 'C':
						formattedBuilder.Append(JudgeSegment(cutScoreBuffer.centerDistanceCutScore, instance.AccuracyJudgments));
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