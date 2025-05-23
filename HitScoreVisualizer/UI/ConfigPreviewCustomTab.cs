using System;
using System.Text.RegularExpressions;
using HitScoreVisualizer.Models;

namespace HitScoreVisualizer.UI;

internal class ConfigPreviewCustomTab
{
	private CustomPreviewTab currentTab;

	public Array BadCutTypeOptions { get; set; } = Enum.GetValues(typeof(BadCutDisplayType));

	public object BadCutTypeFormatter(object v)
	{
		return Regex.Replace(v.ToString(), "(\\B[A-Z])", " $1");
	}

	public void CustomPreviewTabChanged(object segmentedControl, int index)
	{
		currentTab = (CustomPreviewTab)index;
	}

	private BadCutDisplayType badCutType = BadCutDisplayType.All;
	public BadCutDisplayType BadCutType
	{
		get => badCutType;
		set
		{
			badCutType = value;
		}
	}

	public void Disable()
	{
	}

	public void Enable()
	{
	}

	private enum CustomPreviewTab
	{
		Judgments = 0,
		ChainLink = 1,
		BadCut = 2,
		Miss = 3
	}
}