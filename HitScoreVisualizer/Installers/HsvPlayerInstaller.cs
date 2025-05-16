using HitScoreVisualizer.Components;
using HitScoreVisualizer.HarmonyPatches;
using HitScoreVisualizer.Utilities.Services;
using Zenject;

namespace HitScoreVisualizer.Installers;

internal class HsvPlayerInstaller : Installer
{
	private readonly PluginConfig pluginConfig;
	private readonly HsvFlyingEffect hsvFlyingEffectPrefab = HsvFlyingEffect.CreatePrefab();

	public HsvPlayerInstaller(PluginConfig pluginConfig)
	{
		this.pluginConfig = pluginConfig;
	}

	public override void InstallBindings()
	{
		var currentConfig = pluginConfig.SelectedConfig?.Config;
		if (currentConfig is null)
		{
			// No valid HSV config is selected, so HSV will not do anything during gameplay
			return;
		}

		Container.BindInstance(currentConfig).AsSingle();

		Container.Bind<JudgmentService>().AsSingle();

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