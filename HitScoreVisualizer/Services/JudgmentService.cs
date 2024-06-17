using System.Text;
using HitScoreVisualizer.Extensions;
using HitScoreVisualizer.Settings;
using SiraUtil.Logging;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	internal class JudgmentService(ConfigProvider configProvider, SiraLog log)
	{
		private readonly ConfigProvider configProvider = configProvider;
		private readonly SiraLog log = log;

		private Configuration Config => configProvider.CurrentConfig ?? Configuration.Default;

		public (string hitScoreText, Color hitScoreColor) Judge(IReadonlyCutScoreBuffer cutScoreBuffer)
		{
			return cutScoreBuffer.noteCutInfo.noteData.gameplayType switch
			{
				NoteData.GameplayType.Normal => JudgeNormal(cutScoreBuffer, Config),
				NoteData.GameplayType.BurstSliderHead => JudgeChainHead(cutScoreBuffer),
				NoteData.GameplayType.BurstSliderElement => ChainSegmentDisplay(Config),
				_ => (string.Empty, Color.white),
			};
		}

		private static (string, Color) JudgeNormal(IReadonlyCutScoreBuffer cutScoreBuffer, Configuration config)
		{
			var judgment = NormalJudgment.Default;
			var fadeJudgment = NormalJudgment.Default;
			config.NormalJudgments ??= [judgment];

			for (var i = 0; i < config.NormalJudgments.Count; i++)
			{
				if (config.NormalJudgments[i].Threshold <= cutScoreBuffer.cutScore)
				{
					judgment = config.NormalJudgments[i];
					if (i > 0)
					{
						fadeJudgment = config.NormalJudgments[i - 1];
					}
					break;
				}
			}

			var color = judgment.Fade
				? Color.Lerp(
					judgment.Color.ToColor(),
					fadeJudgment.Color.ToColor(),
					Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, cutScoreBuffer.cutScore))
				: judgment.Color.ToColor();

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

		private (string, Color) JudgeChainHead(IReadonlyCutScoreBuffer cutScoreBuffer)
		{
			var judgment = ChainHeadJudgment.Default;
			var fadeJudgment = ChainHeadJudgment.Default;
			Config.ChainHeadJudgments ??= [judgment];

			for (var i = 0; i < Config.ChainHeadJudgments.Count; i++)
			{
				if (Config.ChainHeadJudgments[i].Threshold <= cutScoreBuffer.cutScore)
				{
					judgment = Config.ChainHeadJudgments[i];
					if (i > 0)
					{
						fadeJudgment = Config.ChainHeadJudgments[i - 1];
					}
					break;
				}
			}

			var color = judgment.Fade
				? Color.Lerp(
					judgment.Color!.ToColor(),
					fadeJudgment.Color.ToColor(),
					Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, cutScoreBuffer.cutScore))
				: judgment.Color?.ToColor();

			var text = Config.DisplayMode switch
			{
				"format" => ChainHeadFormat(judgment.Text, cutScoreBuffer, Config),
				"textOnly" => judgment.Text,
				"numeric" => cutScoreBuffer.cutScore.ToString(),
				"scoreOnTop" => $"{cutScoreBuffer.cutScore}\n{judgment.Text}\n",
				_ => $"{judgment.Text}\n{cutScoreBuffer.cutScore}\n"
			};

			return (text, color ?? Color.white);
		}

		private (string, Color) ChainSegmentDisplay(Configuration config) =>
		(
			config.ChainLinkDisplay?.Text ?? ChainLinkDisplay.Default.Text,
			(config.ChainLinkDisplay?.Color ?? ChainLinkDisplay.Default.Color).ToColor()
		);

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

		private static string ConvertTimeDependencePrecision(float timeDependence, int decimalOffset, int decimalPrecision)
		{
			return (timeDependence * Mathf.Pow(10, decimalOffset)).ToString($"n{decimalPrecision}");
		}
	}
}