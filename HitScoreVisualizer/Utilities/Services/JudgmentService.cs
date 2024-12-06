using System.Text;
using HitScoreVisualizer.Settings;
using HitScoreVisualizer.Utilities.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Services
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

		public (string hitScoreText, Color hitScoreColor) Judge(IReadonlyCutScoreBuffer cutScoreBuffer, bool assumeMaxPostSwing)
		{
			var beforeCutScore = cutScoreBuffer.beforeCutScore;
			var centerCutScore = cutScoreBuffer.centerDistanceCutScore;
			var afterCutScore = assumeMaxPostSwing ? cutScoreBuffer.noteScoreDefinition.maxAfterCutScore : cutScoreBuffer.afterCutScore;
			var totalCutScore = beforeCutScore + centerCutScore + afterCutScore;
			var maxPossibleScore = cutScoreBuffer.noteScoreDefinition.maxCutScore;
			var noteCutInfo = cutScoreBuffer.noteCutInfo;

			return cutScoreBuffer.noteCutInfo.noteData.gameplayType switch
			{
				NoteData.GameplayType.Normal => GetNormalDisplay(totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo),
				NoteData.GameplayType.BurstSliderHead => GetChainHeadDisplay(totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo),
				NoteData.GameplayType.BurstSliderElement => GetChainSegmentDisplay(totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo),
				_ => (string.Empty, Color.white),
			};
		}

		private (string, Color) GetNormalDisplay(int totalCutScore, int beforeCutScore,
			int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
		{
			var judgment = NormalJudgment.Default;
			var fadeJudgment = NormalJudgment.Default;
			Config.NormalJudgments ??= [judgment];

			for (var i = 0; i < Config.NormalJudgments.Count; i++)
			{
				if (Config.NormalJudgments[i].Threshold > totalCutScore)
				{
					continue;
				}

				judgment = Config.NormalJudgments[i];
				if (i > 0)
				{
					fadeJudgment = Config.NormalJudgments[i - 1];
				}
				break;
			}

			var color = judgment.Fade
				? Color.Lerp(
					judgment.Color.ToColor(),
					fadeJudgment.Color.ToColor(),
					Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, totalCutScore))
				: judgment.Color.ToColor();

			var text = FormatJudgmentTextByMode(judgment.Text, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo);

			return (text, color);
		}

		private (string, Color) GetChainHeadDisplay(int totalCutScore, int beforeCutScore,
			int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
		{
			var judgment = ChainHeadJudgment.Default;
			var fadeJudgment = ChainHeadJudgment.Default;
			Config.ChainHeadJudgments ??= [judgment];

			for (var i = 0; i < Config.ChainHeadJudgments.Count; i++)
			{
				if (Config.ChainHeadJudgments[i].Threshold > totalCutScore)
				{
					continue;
				}

				judgment = Config.ChainHeadJudgments[i];
				if (i > 0)
				{
					fadeJudgment = Config.ChainHeadJudgments[i - 1];
				}
				break;
			}

			var color = !judgment.Fade ? judgment.Color.ToColor()
				: Color.Lerp(judgment.Color.ToColor(), fadeJudgment.Color.ToColor(),
					Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, totalCutScore));

			var text = FormatJudgmentTextByMode(judgment.Text, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo);

			return (text, color);
		}

		private (string, Color) GetChainSegmentDisplay(int totalCutScore, int beforeCutScore,
			int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
		{
			var chainLinkDisplay = Config.ChainLinkDisplay ?? ChainLinkDisplay.Default;
			var text = FormatJudgmentTextByMode(chainLinkDisplay.Text, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo);
			return (text, chainLinkDisplay.Color.ToColor());
		}

		private string FormatJudgmentTextByMode(string unformattedText, int totalCutScore, int beforeCutScore,
			int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
		{
			return Config.DisplayMode switch
			{
				"format" => FormatJudgmentText(unformattedText, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo),
				"textOnly" => unformattedText,
				"numeric" => totalCutScore.ToString(),
				"scoreOnTop" => $"{totalCutScore}\n{unformattedText}\n",
				"directions" => $"{unformattedText}\n{noteCutInfo.CalculateOffDirection().ToFormattedDirection()}\n",
				_ => $"{unformattedText}\n{totalCutScore}\n"
			};
		}

		private string FormatJudgmentText(string unformattedText, int totalCutScore, int beforeCutScore,
			int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
		{
			var formattedBuilder = new StringBuilder();
			var nextPercentIndex = unformattedText.IndexOf('%');

			var timeDependence = Mathf.Abs(noteCutInfo.cutNormal.z);

			while (nextPercentIndex != -1)
			{
				formattedBuilder.Append(unformattedText.Substring(0, nextPercentIndex));
				if (unformattedText.Length == nextPercentIndex + 1)
				{
					unformattedText += " ";
				}

				var specifier = unformattedText[nextPercentIndex + 1];

				formattedBuilder.Append(specifier switch
				{
					'b' => beforeCutScore,
					'c' => centerCutScore,
					'a' => afterCutScore,
					't' => (timeDependence * Mathf.Pow(10, Config.TimeDependenceDecimalOffset)).ToString($"n{Config.TimeDependenceDecimalPrecision}"),
					'B' => Config.BeforeCutAngleJudgments.JudgeSegment(beforeCutScore),
					'C' => Config.AccuracyJudgments.JudgeSegment(centerCutScore),
					'A' => Config.AfterCutAngleJudgments.JudgeSegment(afterCutScore),
					'T' => Config.TimeDependenceJudgments.JudgeTimeDependenceSegment(timeDependence, Config.TimeDependenceDecimalOffset, Config.TimeDependenceDecimalPrecision),
					'd' => noteCutInfo.CalculateOffDirection().ToFormattedDirection(),
					's' => totalCutScore,
					'p' => $"{(double) totalCutScore / maxPossibleScore * 100:0}",
					'%' => "%",
					'n' => "\n",
					_ => $"%{specifier}"
				});

				unformattedText = unformattedText.Remove(0, nextPercentIndex + 2);
				nextPercentIndex = unformattedText.IndexOf('%');
			}

			return formattedBuilder.Append(unformattedText).ToString();
		}
	}
}