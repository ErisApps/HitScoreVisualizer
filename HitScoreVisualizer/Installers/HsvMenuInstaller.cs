using HitScoreVisualizer.UI;
using Zenject;

namespace HitScoreVisualizer.Installers;

internal sealed class HsvMenuInstaller : Installer
{
	public override void InstallBindings()
	{
		Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
		Container.Bind<HsvMainFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
		Container.Bind<ConfigSelectorViewController>().FromNewComponentAsViewController().AsSingle();
		Container.Bind<PluginSettingsViewController>().FromNewComponentAsViewController().AsSingle();

		Container.Bind<ConfigPreviewViewController>().FromNewComponentAsViewController().AsSingle();
		Container.Bind<ConfigPreviewGridTab>().AsSingle();
		Container.Bind<ConfigPreviewAnimatedTab>().AsSingle();
		Container.Bind<ConfigPreviewCustomTab>().AsSingle();
	}
}