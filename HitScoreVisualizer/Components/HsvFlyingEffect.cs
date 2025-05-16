using HitScoreVisualizer.Models;
using TMPro;
using UnityEngine;
using Zenject;

namespace HitScoreVisualizer.Components;

internal class HsvFlyingEffect : FlyingObjectEffect
{
	public class Pool : MonoMemoryPool<HsvFlyingEffect>;

	private AnimationCurve fadeAnimationCurve = null!;

	[Inject]
	public void Init(FlyingTextEffectAnimationData animationData)
	{
		fadeAnimationCurve = animationData.Fade;
		_moveAnimationCurve = animationData.Move;
	}

	[SerializeField] public TextMeshPro? textMesh;

	private Color color;

	public void InitAndPresent(string text, float duration, Vector3 targetPos, Quaternion rotation, Color color, float fontSize, bool shake)
	{
		if (textMesh == null)
		{
			return;
		}

		this.color = color;
		textMesh.text = text;
		textMesh.fontSize = fontSize;
		InitAndPresent(duration, targetPos, rotation, shake);
	}

	public override void ManualUpdate(float t)
	{
		if (textMesh != null)
		{
			textMesh.color = color with { a = fadeAnimationCurve.Evaluate(t) };
		}
	}

	public static HsvFlyingEffect CreatePrefab()
	{
		var effect = new GameObject(nameof(HsvFlyingEffect)).AddComponent<HsvFlyingEffect>();
		var textObject = new GameObject("Text") { layer = LayerMask.NameToLayer("UI") };
		effect.textMesh = textObject.AddComponent<TextMeshPro>();
		effect.textMesh.alignment = TextAlignmentOptions.Capline;
		effect.textMesh.fontStyle = FontStyles.Bold | FontStyles.Italic;
		textObject.transform.SetParent(effect.gameObject.transform, false);
		return effect;
	}
}