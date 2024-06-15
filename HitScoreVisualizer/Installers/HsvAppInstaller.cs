using HitScoreVisualizer.HarmonyPatches;
using HitScoreVisualizer.Services;
using HitScoreVisualizer.Settings;
using Zenject;

namespace HitScoreVisualizer.Installers
{
	internal sealed class HsvAppInstaller(HSVConfig hsvConfig) : Installer
	{
		private readonly HSVConfig hsvConfig = hsvConfig;

		public override void InstallBindings()
		{
			Container.BindInstance(hsvConfig);
			Container.BindInterfacesAndSelfTo<ConfigProvider>().AsSingle();
			Container.BindInterfacesAndSelfTo<BloomFontProvider>().AsSingle();

			Container.Bind<JudgmentService>().AsSingle();
			Container.BindInterfacesTo<FlyingScoreEffectPatch>().AsSingle();
			Container.BindInterfacesTo<EffectPoolsManualInstallerPatch>().AsSingle();
		}
	}
}