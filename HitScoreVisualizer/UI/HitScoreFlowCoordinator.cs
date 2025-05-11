using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace HitScoreVisualizer.UI;

internal class HitScoreFlowCoordinator : FlowCoordinator
{
	[Inject] private readonly ConfigSelectorViewController configSelectorViewController = null!;
	[Inject] private readonly PluginSettingsViewController pluginSettingsViewController = null!;

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