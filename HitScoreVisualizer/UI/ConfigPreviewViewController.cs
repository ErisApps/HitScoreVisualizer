using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;

namespace HitScoreVisualizer.UI;

[HotReload(RelativePathToLayout = @"\Views\ConfigPreview.bsml")]
[ViewDefinition("HitScoreVisualizer.UI.Views.ConfigPreview.bsml")]
internal class ConfigPreviewViewController : BSMLAutomaticViewController
{
	[UIAction("#post-parse")]
	public void PostParse()
	{
	}

	[Inject] public ConfigPreviewGridTab GridTab { get; set; } = null!;
	[Inject] public ConfigPreviewAnimatedTab AnimatedTab { get; set; } = null!;

	private PreviewTab currentTab;

	public void PreviewTabChanged(object segmentedControl, int index)
	{
		NotifyCurrentTabDisabled();
		currentTab = (PreviewTab)index;
		NotifyCurrentTabEnabled();
	}

	protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
	{
		base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
		NotifyCurrentTabEnabled();
	}

	protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
	{
		base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
		NotifyCurrentTabDisabled();
	}

	private void NotifyCurrentTabEnabled()
	{
		switch (currentTab)
		{
			case PreviewTab.Grid:
				GridTab.Enable();
				break;
			case PreviewTab.Animated:
				AnimatedTab.Enable();
				break;
			case PreviewTab.Custom:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void NotifyCurrentTabDisabled()
	{
		switch (currentTab)
		{
			case PreviewTab.Grid:
				GridTab.Disable();
				break;
			case PreviewTab.Animated:
				AnimatedTab.Disable();
				break;
			case PreviewTab.Custom:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private enum PreviewTab
	{
		Grid = 0,
		Animated = 1,
		Custom = 2
	}
}