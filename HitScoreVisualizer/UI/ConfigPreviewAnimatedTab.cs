using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BGLib.UnityExtension;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace HitScoreVisualizer.UI;

internal class ConfigPreviewAnimatedTab : IPreviewTextEffectDidFinishEvent
{
	[Inject] private readonly ICoroutineStarter coroutineStarter = null!;
	[Inject] private readonly Random random = null!;
	[Inject] private readonly PluginConfig pluginConfig = null!;
	[Inject] private readonly ConfigLoader configLoader = null!;
	[Inject] private readonly RandomScoreGenerator randomScoreGenerator = null!;

	[UIComponent("PreviewTextTemplate")] private readonly TextMeshProUGUI previewTextTemplate = null!;
	[UIComponent("TextContainer")] private readonly RectTransform textContainer = null!;

	private PreviewTextEffect[] textEffects = null!; // assigned in initializer
	private const int NumberOfEffects = 14;
	private const float AnimationDuration = 0.7f; // based on FlyingScoreSpawner.SpawnFlyingScore

	[UIAction("#post-parse")]
	public void PostParse()
	{
		var prefab = AddressablesExtensions.LoadContent<GameObject>("Assets/Prefabs/Effects/FlyingTextEffect.prefab").FirstOrDefault();
		if (prefab == null)
		{
			return;
		}
		previewTextTemplate.gameObject.SetActive(false);
		previewTextTemplate.gameObject.name = nameof(PreviewTextEffect);
		previewTextTemplate.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
		previewTextTemplate.enableAutoSizing = true;
		previewTextTemplate.fontSizeMax = 5f;
		var flyingTextEffect = prefab.GetComponent<FlyingTextEffect>();
		var textEffectPrefab = PreviewTextEffect.Construct(previewTextTemplate.gameObject, previewTextTemplate, flyingTextEffect._fadeAnimationCurve, flyingTextEffect._moveAnimationCurve);
		textEffects = Enumerable.Range(1, NumberOfEffects).Select(_ => Object.Instantiate(textEffectPrefab, textContainer)).ToArray();
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
		currentAnimation = coroutineStarter.StartCoroutine(AnimateTextEffects(config ?? HsvConfigModel.Vanilla));
		animating = true;
	}

	private void StopAnimation()
	{
		if (currentAnimation != null)
		{
			coroutineStarter.StopCoroutine(currentAnimation);
			animating = false;
		}
	}

	private bool animating;
	private WaitForSeconds? animationInterval;
	private readonly WaitForSeconds initialDelay = new(0.25f);

	private IEnumerator AnimateTextEffects(HsvConfigModel config)
	{
		yield return initialDelay;
		animationInterval = new(AnimationDuration / textEffects.Length);
		while (animating)
		{
			foreach (var effect in textEffects)
			{
				SpawnTextEffect(effect, config);
				yield return animationInterval;
			}
		}
	}

	private void SpawnTextEffect(PreviewTextEffect effect, HsvConfigModel config)
	{
		var (text, color) = config.Judge(randomScoreGenerator.GetRandomScore());
		var endPos = effect.transform.localPosition with
		{
			x = random.Next((int) textContainer.rect.xMin, (int) textContainer.rect.xMax + 1),
			y = random.Next((int) (textContainer.rect.yMin + textContainer.rect.yMax / 2f), (int) textContainer.rect.yMax + 1)
		};
		var startPos = endPos with { y = textContainer.rect.yMin };
		effect.InitAndPresent(AnimationDuration, startPos, endPos, color, text, !pluginConfig.DisableItalics);
		effect.DidFinishEvent.Add(this);
		effect.gameObject.SetActive(true);
	}

	public void HandlePreviewTextEffectDidFinish(PreviewTextEffect effect)
	{
		effect.DidFinishEvent.Remove(this);
		effect.gameObject.SetActive(false);
	}
}