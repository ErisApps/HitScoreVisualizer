using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace HitScoreVisualizer.UI;

internal class ConfigPreviewGridTab
{
	[Inject] private readonly ConfigLoader configLoader = null!;
	[Inject] private readonly PluginConfig pluginConfig = null!;
	[Inject] private readonly ICoroutineStarter coroutineStarter = null!;

	[UIComponent("ScoreGrid")] private readonly GridLayoutGroup scoreGrid = null!;
	[UIObject("GridTextTemplate")] private readonly GameObject gridTextTemplate = null!;

	[UIAction("#post-parse")]
	public void PostParse()
	{
		scoreGrid.childAlignment = TextAnchor.MiddleCenter;
		scoreGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		scoreGrid.constraintCount = 4;
	}

	public void Enable()
	{
		configLoader.ConfigChanged += RestartAnimation;
		RestartAnimation(pluginConfig.SelectedConfig?.Config);
	}

	public void Disable()
	{
		configLoader.ConfigChanged -= RestartAnimation;
		StopAnimation();
	}

	private Coroutine? currentAnimation;

	private void RestartAnimation(HsvConfigModel? config)
	{
		StopAnimation();
		currentAnimation = coroutineStarter.StartCoroutine(AnimateGridDisplay(config ?? HsvConfigModel.Vanilla));
	}

	private void StopAnimation()
	{
		if (currentAnimation != null)
		{
			coroutineStarter.StopCoroutine(currentAnimation);
		}
	}

	private readonly WaitForSeconds waitForSeconds = new(0.033f);
	private readonly List<TextMeshProUGUI> gridTexts = [];

	private record GridScore(int Before, int Center, int After);
	private readonly GridScore[] gridScores =
	[
		new(70, 15, 30), new(70, 14, 30), new(70, 13, 30), new(70, 12, 30),
		new(70, 11, 30), new(70, 10, 30), new(70, 05, 30), new(70, 00, 30),
		new(61, 15, 26), new(52, 10, 22), new(43, 05, 18), new(34, 00, 14),
		new(26, 15, 10), new(17, 10, 07), new(09, 05, 04), new(00, 00, 00)
	];

	private IEnumerator AnimateGridDisplay(HsvConfigModel config)
	{
		foreach (var text in gridTexts)
		{
			if (text != null)
			{
				text.gameObject.SetActive(false);
			}
		}

		for (var i = 0; i < gridScores.Length; i++)
		{
			var (before, center, after) = gridScores[i];
			var textMesh = gridTexts.ElementAtOrDefault(i);
			if (textMesh == null)
			{
				var gameObject = Object.Instantiate(gridTextTemplate, scoreGrid.transform);
				gameObject.name = $"PreviewGridScore {i}";
				gameObject.SetActive(true);
				textMesh = gameObject.GetComponentInChildren<TextMeshProUGUI>();
				gridTexts.Add(textMesh);
			}
			textMesh.gameObject.SetActive(true);
			textMesh.enableAutoSizing = true;
			textMesh.fontSizeMax = 5;
			textMesh.fontStyle = pluginConfig.DisableItalics ? FontStyles.Normal : FontStyles.Italic;
			var totalCutScore = before + center + after;
			var judgmentDetails = new JudgmentDetails
			{
				BeforeCutScore = before,
				CenterCutScore = center,
				AfterCutScore = after,
				MaxPossibleScore = 115,
				TotalCutScore = totalCutScore,
				CutInfo = new(
					NoteData.CreateBasicNoteData(0, 0, 0, 0, 0, 0, 0),
					true, true, true, false, 0, Vector3.zero, SaberType.SaberA, 0, 0, Vector3.zero, Vector3.zero, 0, 0, Quaternion.identity, Quaternion.identity, Quaternion.identity, Vector3.zero,
					new SaberMovementData())
			};
			(textMesh.text, textMesh.color) = config.Judge(in judgmentDetails);

			yield return waitForSeconds;
		}
	}
}