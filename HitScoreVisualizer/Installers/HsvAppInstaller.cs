using HitScoreVisualizer.HarmonyPatches;
using HitScoreVisualizer.Utilities.Services;
using JetBrains.Annotations;
using Zenject;

namespace HitScoreVisualizer.Installers;

[UsedImplicitly]
internal sealed class HsvAppInstaller : Installer
{
	private readonly PluginConfig pluginConfig;

	private HsvAppInstaller(PluginConfig pluginConfig)
	{
		this.pluginConfig = pluginConfig;
	}

	public override void InstallBindings()
	{
		Container.BindInstance(pluginConfig);
		Container.Bind<PluginDirectories>().AsSingle();

		Container.BindInterfacesAndSelfTo<ConfigLoader>().AsSingle();
		Container.Bind<ConfigMigrator>().AsSingle();

		Container.BindInterfacesAndSelfTo<BloomFontProvider>().AsSingle();

		Container.Bind<JudgmentService>().AsSingle();

		// Patches
		Container.BindInterfacesTo<HarmonyPatchManager>().AsSingle();
		Container.BindInterfacesTo<EffectPoolsManualInstallerPatch>().AsSingle();
		Container.BindInterfacesTo<FlyingScoreEffectPatch>().AsSingle();
	}
}