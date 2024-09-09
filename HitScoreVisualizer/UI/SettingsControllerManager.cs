using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using IPA.Loader;
using SiraUtil.Zenject;
using Zenject;

namespace HitScoreVisualizer.UI
{
	internal class SettingsControllerManager(UBinder<Plugin, PluginMetadata> pluginMetadata, HitScoreFlowCoordinator hitScoreFlowCoordinator) : IInitializable, IDisposable
	{
		private readonly HitScoreFlowCoordinator hitScoreFlowCoordinator = hitScoreFlowCoordinator;
		private readonly PluginMetadata pluginMetadata = pluginMetadata.Value;

		private MenuButton hsvButton = null!;

		public void Initialize()
		{
			hsvButton = new MenuButton($"<size=89.5%>{pluginMetadata.Name}", "Select the config you want.", OnClick);
			MenuButtons.Instance.RegisterButton(hsvButton);
		}

		private void OnClick()
		{
			if (hitScoreFlowCoordinator == null)
			{
				return;
			}

			BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(hitScoreFlowCoordinator);
		}

		public void Dispose()
		{
			if (hsvButton == null)
			{
				return;
			}

			MenuButtons.Instance.UnregisterButton(hsvButton);

			hsvButton = null!;
		}
	}
}