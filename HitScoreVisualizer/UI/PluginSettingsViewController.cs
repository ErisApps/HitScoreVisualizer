using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Services;
using Zenject;

namespace HitScoreVisualizer.UI;

[HotReload(RelativePathToLayout = @"Views\PluginSettings.bsml")]
[ViewDefinition("HitScoreVisualizer.UI.Views.PluginSettings.bsml")]
internal class PluginSettingsViewController : BSMLAutomaticViewController
{
	[Inject] private readonly HSVConfig config = null!;

	[UIValue("font-type-choices")] private List<object> trailTypeChoices = Enum.GetNames(typeof(HsvFontType)).ToList<object>();
	[UIValue("font-type")]
	public string FontType
	{
		get => config.FontType.ToString();
		set => config.FontType = Enum.TryParse(value, out HsvFontType t) ? t : HsvFontType.Default;
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