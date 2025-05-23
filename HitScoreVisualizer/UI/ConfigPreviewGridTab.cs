using System.Collections;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using HitScoreVisualizer.Models;
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
	private readonly BasicScoreData[] gridScores =
	[
		new(70, 15, 30), new(70, 14, 30), new(70, 13, 30), new(70, 12, 30),
		new(70, 11, 30), new(70, 10, 30), new(70, 05, 30), new(70, 00, 30),
		new(61, 15, 26), new(52, 10, 22), new(43, 05, 18), new(34, 00, 14),
		new(26, 15, 10), new(17, 10, 07), new(09, 05, 04), new(00, 00, 00)
	];

	private readonly WaitForSeconds animationInterval = new(0.04f);
	private PreviewGridText[] gridTexts = null!; // assigned in initializer

	[UIAction("#post-parse")]
	public void PostParse()
	{
		scoreGrid.childAlignment = TextAnchor.MiddleCenter;
		scoreGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		scoreGrid.constraintCount = 4;
		gridTexts = gridScores.Select(score =>
		{
			var gameObject = Object.Instantiate(gridTextTemplate, scoreGrid.transform);
			gameObject.name = nameof(PreviewGridText);
			var textMesh = gameObject.GetComponentInChildren<TextMeshProUGUI>();
			textMesh.enableAutoSizing = true;
			textMesh.fontSizeMax = 5f;
			return new PreviewGridText(gameObject, textMesh, score);
		}).ToArray();
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

	private IEnumerator AnimateGridDisplay(HsvConfigModel config)
	{
		foreach (var text in gridTexts)
		{
			text.SetActive(false);
		}

		foreach (var text in gridTexts)
		{
			text.SetActive(true);
			text.FontStyle = pluginConfig.DisableItalics ? FontStyles.Normal : FontStyles.Italic;
			text.SetTextForConfig(config);

			yield return animationInterval;
		}
	}
}