using HitScoreVisualizer.Models;
using TMPro;
using UnityEngine;
using Zenject;

namespace HitScoreVisualizer.UI;

internal class PreviewTextEffect : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI textMesh = null!;

	[SerializeField]
	private AnimationCurve fadeAnimationCurve = null!;

	[SerializeField]
	private AnimationCurve moveAnimationCurve = null!;

	public static PreviewTextEffect Construct(GameObject go, TextMeshProUGUI textMesh, AnimationCurve fade, AnimationCurve move)
	{
		var previewTextEffect = go.AddComponent<PreviewTextEffect>();
		previewTextEffect.textMesh = textMesh;
		previewTextEffect.fadeAnimationCurve = fade;
		previewTextEffect.moveAnimationCurve = move;
		return previewTextEffect;
	}

	private readonly LazyCopyHashSet<IPreviewTextEffectDidFinishEvent> didFinishEvent = new();
	public ILazyCopyHashSet<IPreviewTextEffectDidFinishEvent> DidFinishEvent => didFinishEvent;

	private bool initialized;
	private float elapsedTime;
	private float duration;
	private Color color;
	private Vector3 startPos;
	private Vector3 endPos;

	public void InitAndPresent(float duration, Vector3 startPos, Vector3 endPos, Color color, string text, bool italics)
	{
		this.duration = duration;
		this.color = color;
		this.startPos = startPos;
		this.endPos = endPos;
		elapsedTime = 0f;
		textMesh.text = text;
		textMesh.fontStyle = italics ? FontStyles.Italic : FontStyles.Normal;
		ManualUpdate(0f);
		initialized = true;
		enabled = true;
	}

	private void Update()
	{
		if (!initialized)
		{
			enabled = false;
			return;
		}

		if (elapsedTime >= duration)
		{
			foreach (var e in didFinishEvent.items)
			{
				e.HandlePreviewTextEffectDidFinish(this);
			}
			return;
		}
		var progress = elapsedTime / duration;
		ManualUpdate(progress);
		elapsedTime += Time.deltaTime;
	}

	private void ManualUpdate(float progress)
	{
		textMesh.color = color with { a = fadeAnimationCurve.Evaluate(progress) };
		transform.localPosition = Vector3.Lerp(startPos, endPos, moveAnimationCurve.Evaluate(progress));
	}
}