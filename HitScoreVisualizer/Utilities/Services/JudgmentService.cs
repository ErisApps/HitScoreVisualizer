using System.Text;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Services;

[UsedImplicitly]
internal class JudgmentService
{
	private readonly HsvConfigModel config;

	public JudgmentService(HsvConfigModel config)
	{
		this.config = config;
	}

	public (string hitScoreText, Color hitScoreColor) Judge(IReadonlyCutScoreBuffer cutScoreBuffer, bool assumeMaxPostSwing)
	{
		var beforeCutScore = cutScoreBuffer.beforeCutScore;
		var centerCutScore = cutScoreBuffer.centerDistanceCutScore;
		var afterCutScore = assumeMaxPostSwing ? cutScoreBuffer.noteScoreDefinition.maxAfterCutScore : cutScoreBuffer.afterCutScore;
		var totalCutScore = cutScoreBuffer.cutScore;
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

		for (var i = 0; i < config.Judgments.Count; i++)
		{
			if (config.Judgments[i].Threshold > totalCutScore)
			{
				continue;
			}

			judgment = config.Judgments[i];
			if (i > 0)
			{
				fadeJudgment = config.Judgments[i - 1];
			}
			break;
		}

		var color = judgment.Fade
			? Color.Lerp(
				judgment.Color,
				fadeJudgment.Color,
				Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, totalCutScore))
			: judgment.Color;

		var text = FormatJudgmentTextByMode(judgment.Text, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo);

		return (text, color);
	}

	private (string, Color) GetChainHeadDisplay(int totalCutScore, int beforeCutScore,
		int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
	{
		var judgment = ChainHeadJudgment.Default;
		var fadeJudgment = ChainHeadJudgment.Default;

		for (var i = 0; i < config.ChainHeadJudgments.Count; i++)
		{
			if (config.ChainHeadJudgments[i].Threshold > totalCutScore)
			{
				continue;
			}

			judgment = config.ChainHeadJudgments[i];
			if (i > 0)
			{
				fadeJudgment = config.ChainHeadJudgments[i - 1];
			}
			break;
		}

		var color = !judgment.Fade ? judgment.Color
			: Color.Lerp(judgment.Color, fadeJudgment.Color, Mathf.InverseLerp(judgment.Threshold, fadeJudgment.Threshold, totalCutScore));

		var text = FormatJudgmentTextByMode(judgment.Text, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo);

		return (text, color);
	}

	private (string, Color) GetChainSegmentDisplay(int totalCutScore, int beforeCutScore,
		int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
	{
		var chainLinkDisplay = config.ChainLinkDisplay ?? ChainLinkDisplay.Default;
		var text = FormatJudgmentTextByMode(chainLinkDisplay.Text, totalCutScore, beforeCutScore, centerCutScore, afterCutScore, maxPossibleScore, noteCutInfo);
		return (text, chainLinkDisplay.Color);
	}

	private string FormatJudgmentTextByMode(string unformattedText, int totalCutScore, int beforeCutScore,
		int centerCutScore, int afterCutScore, int maxPossibleScore, NoteCutInfo noteCutInfo)
	{
		return config.DisplayMode switch
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
				't' => (timeDependence * Mathf.Pow(10, config.TimeDependenceDecimalOffset)).ToString($"n{config.TimeDependenceDecimalPrecision}"),
				'B' => config.BeforeCutAngleJudgments.JudgeSegment(beforeCutScore),
				'C' => config.AccuracyJudgments.JudgeSegment(centerCutScore),
				'A' => config.AfterCutAngleJudgments.JudgeSegment(afterCutScore),
				'T' => config.TimeDependenceJudgments.JudgeTimeDependenceSegment(timeDependence, config.TimeDependenceDecimalOffset, config.TimeDependenceDecimalPrecision),
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