using UnityEngine;
using Zenject;

namespace HitScoreVisualizer.Components;

internal class HsvFlyingEffectSpawner : MonoBehaviour, IFlyingObjectEffectDidFinishEvent
{
	public record InitData(float Duration, float XSpread, float TargetYPos, float TargetZPos, Color Color, float FontSize);

	private float duration;
	private float xSpread;
	private float targetYPos;
	private float targetZPos;
	private Color color;
	private float fontSize;
	private HsvFlyingEffect.Pool missTextEffectPool = null!;
	private PluginConfig pluginConfig = null!;

	[Inject]
	public void Init(InitData initData, HsvFlyingEffect.Pool effectPool, PluginConfig config)
	{
		duration = initData.Duration;
		xSpread = initData.XSpread;
		targetYPos = initData.TargetYPos;
		targetZPos = initData.TargetZPos;
		color = initData.Color;
		fontSize = initData.FontSize;
		missTextEffectPool = effectPool;
		pluginConfig = config;
	}

	public void SpawnText(Vector3 pos, Quaternion rotation, Quaternion inverseRotation, string text)
	{
		var missTextEffect = missTextEffectPool.Spawn();
		missTextEffect.didFinishEvent.Add(this);
		missTextEffect.transform.localPosition = pos;

		var targetPos = rotation * new Vector3(Mathf.Sign((inverseRotation * pos).x) * xSpread, targetYPos, targetZPos);

		missTextEffect.InitAndPresent(text, duration, targetPos, rotation, color, fontSize, false);
	}

	public void HandleFlyingObjectEffectDidFinish(FlyingObjectEffect flyingObjectEffect)
	{
		flyingObjectEffect.didFinishEvent.Remove(this);
		missTextEffectPool.Despawn((HsvFlyingEffect)flyingObjectEffect);
	}
}