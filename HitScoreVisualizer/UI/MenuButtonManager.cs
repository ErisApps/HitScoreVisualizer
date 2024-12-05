using System;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace HitScoreVisualizer.UI
{
	internal class MenuButtonManager : IInitializable, IDisposable
	{
		private readonly HitScoreFlowCoordinator hitScoreFlowCoordinator;
		private readonly MainFlowCoordinator mainFlowCoordinator;

		private readonly MenuButton hsvButton;

		public MenuButtonManager(HitScoreFlowCoordinator hitScoreFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
		{
			this.hitScoreFlowCoordinator = hitScoreFlowCoordinator;
			this.mainFlowCoordinator = mainFlowCoordinator;

			hsvButton = new($"<size=89.5%>{Plugin.Metadata.Name}", "Select the config you want.", OnClick);
		}

		public void Initialize()
		{
			MenuButtons.Instance.RegisterButton(hsvButton);
		}

		public void Dispose()
		{
			MenuButtons.Instance.UnregisterButton(hsvButton);
		}

		private void OnClick()
		{
			mainFlowCoordinator.PresentFlowCoordinator(hitScoreFlowCoordinator);
		}
	}
}