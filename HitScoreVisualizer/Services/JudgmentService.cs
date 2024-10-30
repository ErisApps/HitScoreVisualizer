using System.Text;
using HitScoreVisualizer.Extensions;
using HitScoreVisualizer.Settings;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	[UsedImplicitly]
	internal class JudgmentService
	{
		private readonly ConfigProvider configProvider;

		private JudgmentService(ConfigProvider configProvider)
		{
			this.configProvider = configProvider;
		}

		private Configuration Config => configProvider.CurrentConfig ?? Configuration.Default;

		public (string hitScoreText, Color hitScoreColor) Judge(IReadonlyCutScoreBuffer cutScoreBuffer, int? afterCutOverride = null)
		{
			afterCutOverride ??= cutScoreBuffer.afterCutScore;
			return cutScoreBuffer.noteCutInfo.noteData.gameplayType switch
			{
				NoteData.GameplayType.Normal => GetNormalDispaly(cutScoreBuffer, afterCutOverride.Value),
				NoteData.GameplayType.BurstSliderHead => GetChainHeadDisplay(cutScoreBuffer, afterCutOverride.Value),
				NoteData.GameplayType.BurstSliderElement => GetChainSegmentDisplay(cutScoreBuffer, afterCutOverride.Value),
				_ => (string.Empty, Color.white),
			};
		}

		private (string, Color) GetNormalDispaly(IReadonlyCutScoreBuffer cutScoreBuffer, int afterCutScore)
		{
			var judgment = NormalJudgment.Default;
			var fadeJudgment = NormalJudgment.Default;
			Config.NormalJudgments ??= [judgment];

			for (var i = 0; i < Config.NormalJudgments.Count; i++)
			{
				if (Config.NormalJudgments[i].Threshold <= cutScoreBuffer.cutScore)
				{
					judgment = Config.NormalJudgments[i];
					if (i > 0)
					{
						fadeJudgment = Config.NormalJudgments[i - 1];
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

			var text = FormatJudgmentTextByMode(judgment.Text, cutScoreBuffer, afterCutScore);

			return (text, color);
		}

		private (string, Color) GetChainHeadDisplay(IReadonlyCutScoreBuffer cutScoreBuffer, int afterCutScore)
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

			var text = FormatJudgmentTextByMode(judgment.Text, cutScoreBuffer, afterCutScore);

			return (text, color ?? Color.white);
		}

		private (string, Color) GetChainSegmentDisplay(IReadonlyCutScoreBuffer cutScoreBuffer, int afterCutScore)
		{
			var text = Config.ChainLinkDisplay == null ? ChainLinkDisplay.Default.Text
				: FormatJudgmentTextByMode(Config.ChainLinkDisplay.Text, cutScoreBuffer, afterCutScore);
			var color = (Config.ChainLinkDisplay?.Color ?? ChainLinkDisplay.Default.Color).ToColor();
			return (text, color);
		}

		private string FormatJudgmentTextByMode(string unformattedText, IReadonlyCutScoreBuffer cutScoreBuffer, int afterCutScore)
		{
			return Config.DisplayMode switch
			{
				"format" => FormatJudgmentText(unformattedText, cutScoreBuffer, afterCutScore),
				"textOnly" => unformattedText,
				"numeric" => cutScoreBuffer.cutScore.ToString(),
				"scoreOnTop" => $"{cutScoreBuffer.cutScore}\n{unformattedText}\n",
				_ => $"{unformattedText}\n{cutScoreBuffer.cutScore}\n"
			};
		}

		private string FormatJudgmentText(string unformattedText, IReadonlyCutScoreBuffer cutScoreBuffer, int afterCutScore)
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
						formattedBuilder.Append(afterCutScore);
						break;
					case 't':
						formattedBuilder.Append((timeDependence * Mathf.Pow(10, Config.TimeDependenceDecimalOffset)).ToString($"n{Config.TimeDependenceDecimalPrecision}"));
						break;
					case 'B':
						formattedBuilder.Append(Config.BeforeCutAngleJudgments.JudgeSegment(cutScoreBuffer.beforeCutScore));
						break;
					case 'C':
						formattedBuilder.Append(Config.AccuracyJudgments.JudgeSegment(cutScoreBuffer.centerDistanceCutScore));
						break;
					case 'A':
						formattedBuilder.Append(Config.AfterCutAngleJudgments.JudgeSegment(afterCutScore));
						break;
					case 'T':
						formattedBuilder.Append(Config.TimeDependenceJudgments.JudgeTimeDependenceSegment(timeDependence, Config.TimeDependenceDecimalOffset, Config.TimeDependenceDecimalPrecision));
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
	}
}