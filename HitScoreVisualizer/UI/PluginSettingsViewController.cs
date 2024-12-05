using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HitScoreVisualizer.Settings;
using Zenject;

namespace HitScoreVisualizer.UI;

[HotReload(RelativePathToLayout = @"Views\PluginSettings.bsml")]
[ViewDefinition("HitScoreVisualizer.UI.Views.PluginSettings.bsml")]
internal class PluginSettingsViewController : BSMLAutomaticViewController
{
	private HSVConfig config = null!;

	[Inject]
	public void Construct(HSVConfig config)
	{
		this.config = config;
	}

	[UIValue("hit-score-bloom")]
	public bool HitScoreBloom
	{
		get => config.HitScoreBloom;
		set => config.HitScoreBloom = value;
	}

	[UIValue("disable-italics")]
	public bool DisableItalics
	{
		get => config.DisableItalics;
		set => config.DisableItalics = value;
	}

	[UIValue("override-no-texts-and-huds")]
	public bool OverrideNoTextsAndHuds
	{
		get => config.OverrideNoTextsAndHuds;
		set => config.OverrideNoTextsAndHuds = value;
	}
}