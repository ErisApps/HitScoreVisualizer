using HitScoreVisualizer.HarmonyPatches;
using HitScoreVisualizer.Settings;
using HitScoreVisualizer.Utilities.Services;
using JetBrains.Annotations;
using Zenject;

namespace HitScoreVisualizer.Installers
{
	[UsedImplicitly]
	internal sealed class HsvAppInstaller : Installer
	{
		private readonly HSVConfig hsvConfig;

		private HsvAppInstaller(HSVConfig hsvConfig)
		{
			this.hsvConfig = hsvConfig;
		}

		public override void InstallBindings()
		{
			Container.BindInstance(hsvConfig);
			Container.BindInterfacesAndSelfTo<ConfigProvider>().AsSingle();
			Container.BindInterfacesAndSelfTo<BloomFontProvider>().AsSingle();

			Container.Bind<JudgmentService>().AsSingle();

			// Patches
			Container.BindInterfacesTo<HarmonyPatchManager>().AsSingle();
			Container.BindInterfacesTo<EffectPoolsManualInstallerPatch>().AsSingle();
			Container.BindInterfacesTo<FlyingScoreEffectPatch>().AsSingle();
		}
	}
}