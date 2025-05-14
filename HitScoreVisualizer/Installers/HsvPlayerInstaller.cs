using HitScoreVisualizer.Components;
using HitScoreVisualizer.HarmonyPatches;
using TMPro;
using UnityEngine;
using Zenject;

namespace HitScoreVisualizer.Installers;

internal class HsvPlayerInstaller : Installer
{
	private readonly HsvFlyingEffect hsvFlyingEffectPrefab = HsvFlyingEffect.CreatePrefab();

	public override void InstallBindings()
	{
		Container.Bind<HsvFlyingEffectSpawner>().FromNewComponentOnNewGameObject().AsSingle();

		Container.BindMemoryPool<HsvFlyingEffect, HsvFlyingEffect.Pool>()
			.WithInitialSize(20)
			.FromComponentInNewPrefab(hsvFlyingEffectPrefab);

		// Patches
		Container.BindInterfacesTo<MissedNoteEffectSpawnerPatch>().AsSingle();
		Container.BindInterfacesTo<BadNoteCutEffectSpawnerPatch>().AsSingle();
		Container.BindInterfacesTo<FlyingScoreEffectPatch>().AsSingle();
	}
}