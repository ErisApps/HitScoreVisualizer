using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using IPA.Loader;
using SiraUtil.Zenject;
using Zenject;

namespace HitScoreVisualizer.UI
{
	internal class SettingsControllerManager : IInitializable, IDisposable
	{
		private readonly HitScoreFlowCoordinator _hitScoreFlowCoordinator;

		private MenuButton? _hsvButton;

		public SettingsControllerManager(UBinder<Plugin, PluginMetadata> pluginMetadata, HitScoreFlowCoordinator hitScoreFlowCoordinator)
		{
			_hitScoreFlowCoordinator = hitScoreFlowCoordinator;

			_hsvButton = new MenuButton($"<size=89.5%>{pluginMetadata.Value.Name}", "Select the config you want.", OnClick);
		}

		public void Initialize()
		{
			MenuButtons.instance.RegisterButton(_hsvButton);
		}

		private void OnClick()
		{
			if (_hitScoreFlowCoordinator == null)
			{
				return;
			}

			BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_hitScoreFlowCoordinator);
		}

		public void Dispose()
		{
			if (_hsvButton == null)
			{
				return;
			}

			MenuButtons.instance.UnregisterButton(_hsvButton);

			_hsvButton = null!;
		}
	}
}