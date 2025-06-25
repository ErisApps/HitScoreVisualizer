using HitScoreVisualizer.Components;
using HitScoreVisualizer.Models;
using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches;

public class GameCoreInstallerHook : IAffinity
{
	[AffinityPrefix]
	[AffinityPriority(1000)]
	[AffinityPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
	private void InstallBindingsPostfix(GameplayCoreInstaller __instance)
	{
		var container = __instance.Container;
		var missSpriteSpawner = __instance._missedNoteEffectSpawnerPrefab._missedNoteFlyingSpriteSpawner;
		var flyingTextEffect = __instance._effectPoolsManualInstaller._flyingTextEffectPrefab;

		container.BindInstance(new HsvFlyingEffectSpawner.InitData(
			missSpriteSpawner._duration,
			missSpriteSpawner._xSpread,
			missSpriteSpawner._targetYPos,
			missSpriteSpawner._targetZPos,
			Color.white,
			4.5f)).AsSingle();

		container.BindInstance(new FlyingTextEffectAnimationData(
			flyingTextEffect._fadeAnimationCurve,
			flyingTextEffect._moveAnimationCurve)).AsSingle();
	}
}