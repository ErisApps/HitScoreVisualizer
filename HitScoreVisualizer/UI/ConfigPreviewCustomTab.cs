using System;
using System.Text.RegularExpressions;
using BeatSaberMarkupLanguage.Attributes;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Services;
using TMPro;
using Zenject;
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace HitScoreVisualizer.UI;

internal class ConfigPreviewCustomTab
{
	[Inject] private readonly PluginConfig pluginConfig = null!;
	[Inject] private readonly ConfigLoader configLoader = null!;

	[UIAction("#post-parse")]
	public void PostParse()
	{
		judgmentsText.enableAutoSizing = true;
		judgmentsText.fontSizeMax = 5;
		chainLinkText.enableAutoSizing = true;
		chainLinkText.fontSizeMax = 5;
		badCutText.enableAutoSizing = true;
		badCutText.fontSizeMax = 5;
		missText.enableAutoSizing = true;
		missText.fontSizeMax = 5;
	}

	public void Enable()
	{
		configLoader.ConfigChanged += ConfigChanged;
		ConfigChanged(pluginConfig.SelectedConfig?.Config);
	}

	public void Disable()
	{
		configLoader.ConfigChanged -= ConfigChanged;
	}

	private void ConfigChanged(HsvConfigModel? config)
	{
		UpdateJudgmentsText();
		UpdateChainLinkText();
	}

	public object EnumFormatter(Enum v)
	{
		return Regex.Replace(v.ToString(), "(\\B[A-Z])", " $1");
	}

#region JudgmentsTab
	[UIComponent("JudgmentsText")] private readonly TextMeshProUGUI judgmentsText = null!;

	private JudgmentType currentJudgmentType = JudgmentType.Normal;
	private JudgmentType CurrentJudgmentType
	{
		get => currentJudgmentType;
		set
		{
			currentJudgmentType = value;
			UpdateJudgmentsText();
		}
	}

	public Array JudgmentOptions { get; set; } = Enum.GetValues(typeof(JudgmentType));

	private int before = 70;
	public int Before
	{
		get => before;
		set
		{
			before = value;
			UpdateJudgmentsText();
		}
	}

	private int center = 15;
	public int Center
	{
		get => center;
		set
		{
			center = value;
			UpdateJudgmentsText();
		}
	}

	private int after = 30;
	public int After
	{
		get => after;
		set
		{
			after = value;
			UpdateJudgmentsText();
		}
	}

	private float timeDependence;
	public float TimeDependence
	{
		get => timeDependence;
		set
		{
			timeDependence = value;
			UpdateJudgmentsText();
		}
	}

	private void UpdateJudgmentsText()
	{
		var (afterCut, max, cutInfo) = currentJudgmentType switch
		{
			JudgmentType.Normal => (after, 115, DummyScores.Normal),
			JudgmentType.ChainHead => (0, 85, DummyScores.ChainHead),
			_ => throw new ArgumentOutOfRangeException()
		};
		(judgmentsText.text, judgmentsText.color) = (pluginConfig.SelectedConfig?.Config ?? HsvConfigModel.Vanilla).Judge(new()
		{
			BeforeCutScore = before,
			CenterCutScore = center,
			AfterCutScore = afterCut,
			MaxPossibleScore = max,
			TotalCutScore = before + center + afterCut,
			CutInfo = cutInfo
		});
	}
#endregion

#region ChainLinkTab
	[UIComponent("ChainLinkText")] private readonly TextMeshProUGUI chainLinkText = null!;

	private void UpdateChainLinkText()
	{
		(chainLinkText.text, chainLinkText.color) = (pluginConfig.SelectedConfig?.Config ?? HsvConfigModel.Vanilla).Judge(new()
		{
			BeforeCutScore = 0,
			CenterCutScore = 0,
			AfterCutScore = 0,
			MaxPossibleScore = 20,
			TotalCutScore = 20,
			CutInfo = DummyScores.ChainLink
		});
	}
#endregion

#region BadCutTab
	[UIComponent("BadCutText")] private readonly TextMeshProUGUI badCutText = null!;

	public Array BadCutTypeOptions { get; set; } = Enum.GetValues(typeof(BadCutDisplayType));

	private BadCutDisplayType badCutType = BadCutDisplayType.All;
	public BadCutDisplayType BadCutType
	{
		get => badCutType;
		set
		{
			badCutType = value;
		}
	}
#endregion

#region MissTab
	[UIComponent("MissText")] private readonly TextMeshProUGUI missText = null!;

#endregion

private enum JudgmentType
	{
		Normal,
		ChainHead
	}
}