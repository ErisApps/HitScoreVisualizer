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
	[Inject] private readonly PluginConfig config = null!;

	public List<object> FontTypeChoices = Enum.GetNames(typeof(HsvFontType)).ToList<object>();

	public string FontType
	{
		get => config.FontType.ToString();
		set => config.FontType = Enum.TryParse(value, out HsvFontType t) ? t : HsvFontType.Default;
	}

	public bool DisableItalics
	{
		get => config.DisableItalics;
		set => config.DisableItalics = value;
	}

	public bool OverrideNoTextsAndHuds
	{
		get => config.OverrideNoTextsAndHuds;
		set => config.OverrideNoTextsAndHuds = value;
	}
}