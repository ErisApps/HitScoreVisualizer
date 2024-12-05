using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace HitScoreVisualizer.UI
{
	internal class HitScoreFlowCoordinator : FlowCoordinator
	{
		private ConfigSelectorViewController configSelectorViewController = null!;
		private PluginSettingsViewController pluginSettingsViewController = null!;

		[Inject]
		internal void Construct(ConfigSelectorViewController configSelectorViewController, PluginSettingsViewController pluginSettingsViewController)
		{
			this.configSelectorViewController = configSelectorViewController;
			this.pluginSettingsViewController = pluginSettingsViewController;
		}

		protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
		{
			if (firstActivation)
			{
				SetTitle(Plugin.Metadata.Name);
				showBackButton = true;

				ProvideInitialViewControllers(configSelectorViewController, pluginSettingsViewController);
			}
		}

		protected override void BackButtonWasPressed(ViewController _)
		{
			// Dismiss ourselves
			BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
		}
	}
}