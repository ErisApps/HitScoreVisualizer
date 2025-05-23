using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Services;
using TMPro;
using UnityEngine;

namespace HitScoreVisualizer.UI;

internal class PreviewGridText
{
	private readonly GameObject root;
	private readonly TextMeshProUGUI textMesh;
	private readonly BasicScoreData score;

	public PreviewGridText(GameObject root, TextMeshProUGUI textMesh, BasicScoreData score)
	{
		this.root = root;
		this.textMesh = textMesh;
		this.score = score;
	}

	public FontStyles FontStyle
	{
		set => textMesh.fontStyle = value;
	}

	public void SetActive(bool active)
	{
		root.SetActive(active);
	}

	public void SetTextForConfig(HsvConfigModel config)
	{
		var judgmentDetails = new JudgmentDetails
		{
			BeforeCutScore = score.Before,
			CenterCutScore = score.Center,
			AfterCutScore = score.After,
			MaxPossibleScore = 115,
			TotalCutScore = score.Before + score.Center + score.After,
			CutInfo = RandomScoreGenerator.DummyNormalNote
		};
		(textMesh.text, textMesh.color) = config.Judge(in judgmentDetails);
	}
}