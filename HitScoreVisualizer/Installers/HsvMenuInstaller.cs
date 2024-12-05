using HitScoreVisualizer.UI;
using Zenject;

namespace HitScoreVisualizer.Installers
{
	internal sealed class HsvMenuInstaller : Installer
	{
		public override void InstallBindings()
		{
			Container.Bind<ConfigSelectorViewController>().FromNewComponentAsViewController().AsSingle();
			Container.Bind<HitScoreFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
			Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
		}
	}
}