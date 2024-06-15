using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Loader;
using SiraUtil.Zenject;
using Zenject;

namespace HitScoreVisualizer.UI
{
	internal class HitScoreFlowCoordinator : FlowCoordinator
	{
		private string pluginName = null!;
		private ConfigSelectorViewController configSelectorViewController = null!;

		[Inject]
		internal void Construct(UBinder<Plugin, PluginMetadata> pluginMetadata, ConfigSelectorViewController configSelectorViewController)
		{
			pluginName = pluginMetadata.Value.Name;
			this.configSelectorViewController = configSelectorViewController;
		}

		protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
		{
			if (firstActivation)
			{
				SetTitle(pluginName);
				showBackButton = true;

				ProvideInitialViewControllers(configSelectorViewController);
			}
		}

		protected override void BackButtonWasPressed(ViewController _)
		{
			// Dismiss ourselves
			BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
		}
	}
}