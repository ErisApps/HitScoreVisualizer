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

		public (string hitScoreText, Color hitScoreColor) Judge(IReadonlyCutScoreBuffer cutScoreBuffer)
		{
			return cutScoreBuffer.noteCutInfo.noteData.gameplayType switch
			{
				NoteData.GameplayType.Normal => JudgeNormal(cutScoreBuffer, Config),
				NoteData.GameplayType.BurstSliderHead => JudgeChainHead(cutScoreBuffer, Config),
				NoteData.GameplayType.BurstSliderElement => ChainSegmentDisplay(Config),
				_ => (string.Empty, Color.white),
			};
		}

		private static (string, Color) JudgeNormal(IReadonlyCutScoreBuffer cutScoreBuffer, Configuration config)
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
				"format" => NormalNoteFormat(judgment.Text, cutScoreBuffer, config),
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
				"format" => ChainHeadFormat(judgment.Text, cutScoreBuffer, config),
				"textOnly" => judgment.Text,
				"numeric" => cutScoreBuffer.cutScore.ToString(),
				"scoreOnTop" => $"{cutScoreBuffer.cutScore}\n{judgment.Text}\n",
				_ => $"{judgment.Text}\n{cutScoreBuffer.cutScore}\n"
			};

			return (text, color);
		}

		private static string NormalNoteFormat(string unformattedText, IReadonlyCutScoreBuffer cutScoreBuffer, Configuration config)
		{
			var formattedBuilder = new StringBuilder();
			var nextPercentIndex = unformattedText.IndexOf('%');

			var timeDependence = Mathf.Abs(cutScoreBuffer.noteCutInfo.cutNormal.z);

			while (nextPercentIndex != -1)
			{
				formattedBuilder.Append(unformattedText.Substring(0, nextPercentIndex));
				if (unformattedText.Length == nextPercentIndex + 1)
				{
					unformattedText += " ";
				}

				var specifier = unformattedText[nextPercentIndex + 1];

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
						formattedBuilder.Append(ConvertTimeDependencePrecision(timeDependence, config.TimeDependenceDecimalOffset, config.TimeDependenceDecimalPrecision));
						break;
					case 'B':
						formattedBuilder.Append(config.BeforeCutAngleJudgments.JudgeSegment(cutScoreBuffer.beforeCutScore));
						break;
					case 'C':
						formattedBuilder.Append(config.AccuracyJudgments.JudgeSegment(cutScoreBuffer.centerDistanceCutScore));
						break;
					case 'A':
						formattedBuilder.Append(config.AfterCutAngleJudgments.JudgeSegment(cutScoreBuffer.afterCutScore));
						break;
					case 'T':
						formattedBuilder.Append(config.TimeDependenceJudgments.JudgeTimeDependenceSegment(timeDependence, config.TimeDependenceDecimalOffset, config.TimeDependenceDecimalPrecision));
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

				unformattedText = unformattedText.Remove(0, nextPercentIndex + 2);
				nextPercentIndex = unformattedText.IndexOf('%');
			}

			return formattedBuilder.Append(unformattedText).ToString();
		}

		private static string ChainHeadFormat(string unformattedText, IReadonlyCutScoreBuffer cutScoreBuffer, Configuration config)
		{
			var builder = new StringBuilder();
			var nextPercentIndex = unformattedText.IndexOf('%');

			var timeDependence = Mathf.Abs(cutScoreBuffer.noteCutInfo.cutNormal.z);

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
					case 'b':
						builder.Append(cutScoreBuffer.beforeCutScore);
						break;
					case 'c':
						builder.Append(cutScoreBuffer.centerDistanceCutScore);
						break;
					case 't':
						builder.Append(ConvertTimeDependencePrecision(timeDependence, config.TimeDependenceDecimalOffset, config.TimeDependenceDecimalPrecision));
						break;
					case 'B':
						builder.Append(config.BeforeCutAngleJudgments.JudgeSegment(cutScoreBuffer.beforeCutScore));
						break;
					case 'C':
						builder.Append(config.AccuracyJudgments.JudgeSegment(cutScoreBuffer.centerDistanceCutScore));
						break;
					case 'T':
						builder.Append(config.TimeDependenceJudgments.JudgeTimeDependenceSegment(timeDependence, config.TimeDependenceDecimalOffset, config.TimeDependenceDecimalPrecision));
						break;
					case 's':
						builder.Append(cutScoreBuffer.cutScore);
						break;
					case 'p':
						builder.Append($"{(double) cutScoreBuffer.cutScore / cutScoreBuffer.noteScoreDefinition.maxCutScore * 100:0}");
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

		private static (string, Color) ChainSegmentDisplay(Configuration config) =>
		(
			config.ChainLinkDisplay?.Text ?? string.Empty,
			(config.ChainLinkDisplay?.Color ?? ChainLinkDisplay.Default.Color).ToColor()
		);

		private static string ConvertTimeDependencePrecision(float timeDependence, int decimalOffset, int decimalPrecision)
		{
			return (timeDependence * Mathf.Pow(10, decimalOffset)).ToString($"n{decimalPrecision}");
		}
	}
}