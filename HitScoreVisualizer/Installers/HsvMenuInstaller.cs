using HitScoreVisualizer.UI;
using Zenject;

namespace HitScoreVisualizer.Installers;

internal sealed class HsvMenuInstaller : Installer
{
	public override void InstallBindings()
	{
		Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
		Container.Bind<HitScoreFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
		Container.Bind<ConfigSelectorViewController>().FromNewComponentAsViewController().AsSingle();
		Container.Bind<PluginSettingsViewController>().FromNewComponentAsViewController().AsSingle();
	}
}