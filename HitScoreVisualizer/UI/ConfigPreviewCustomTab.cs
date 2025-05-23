using System;
using System.Linq;
using System.Text.RegularExpressions;
using BeatSaberMarkupLanguage.Attributes;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Services;
using TMPro;
using UnityEngine;
using Zenject;
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace HitScoreVisualizer.UI;

internal class ConfigPreviewCustomTab
{
	[Inject] private readonly PluginConfig pluginConfig = null!;
	[Inject] private readonly ConfigLoader configLoader = null!;

	private TextMeshProUGUI[] texts = null!; // set in initializer

	[UIAction("#post-parse")]
	public void PostParse()
	{
		texts = [judgmentsText, chainLinkText, badCutText, missText];
		foreach (var text in texts)
		{
			text.enableAutoSizing = true;
			text.fontSizeMax = 5;
		}
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
		if (config is not null)
		{
			if (config.BadCutDisplays is not null or [])
			{
				allBadCuts = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All));
				wrongDirections = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All or BadCutDisplayType.WrongDirection));
				wrongColors = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All or BadCutDisplayType.WrongColor));
				bombs = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All or BadCutDisplayType.Bomb));
			}

			if (config.MissDisplays is not null or [])
			{
				misses = new(config.MissDisplays);
			}
		}
		else
		{
			allBadCuts = new();
			wrongDirections = new();
			wrongColors = new();
			bombs = new();
			misses = new();
		}

		UpdateAllTexts();
	}

	private void UpdateAllTexts()
	{
		foreach (var text in texts)
		{
			text.fontStyle = pluginConfig.DisableItalics ? FontStyles.Normal : FontStyles.Italic;
		}
		UpdateJudgmentsText();
		UpdateChainLinkText();
		UpdateBadCutText();
		UpdateMissText();
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
			UpdateBadCutText();
		}
	}

	private ItemRevolver<BadCutDisplay> allBadCuts = new();
	private ItemRevolver<BadCutDisplay> wrongDirections = new();
	private ItemRevolver<BadCutDisplay> wrongColors = new();
	private ItemRevolver<BadCutDisplay> bombs = new();

	public void NextBadCut()
	{
		UpdateBadCutText(BadCutType switch
		{
			BadCutDisplayType.All => allBadCuts.AdvanceNext(),
			BadCutDisplayType.WrongDirection => wrongDirections.AdvanceNext(),
			BadCutDisplayType.WrongColor => wrongColors.AdvanceNext(),
			BadCutDisplayType.Bomb => bombs.AdvanceNext(),
			_ => throw new ArgumentOutOfRangeException()
		});
	}

	public void PreviousBadCut()
	{
		UpdateBadCutText(BadCutType switch
		{
			BadCutDisplayType.All => allBadCuts.AdvancePrevious(),
			BadCutDisplayType.WrongDirection => wrongDirections.AdvancePrevious(),
			BadCutDisplayType.WrongColor => wrongColors.AdvancePrevious(),
			BadCutDisplayType.Bomb => bombs.AdvancePrevious(),
			_ => throw new ArgumentOutOfRangeException()
		});
	}

	private void UpdateBadCutText(BadCutDisplay? display = null)
	{
		display ??= badCutType switch
		{
			BadCutDisplayType.All => allBadCuts.Current,
			BadCutDisplayType.WrongDirection => wrongDirections.Current,
			BadCutDisplayType.WrongColor => wrongColors.Current,
			BadCutDisplayType.Bomb => bombs.Current,
			_ => throw new ArgumentOutOfRangeException()
		};
		badCutText.text = display?.Text ?? "<i>No display.";
		badCutText.color = display?.Color ?? new Color32(0xFF, 0xFF, 0xFF, 0xAA);
	}
#endregion

#region MissTab
	[UIComponent("MissText")] private readonly TextMeshProUGUI missText = null!;

	private ItemRevolver<MissDisplay> misses = new();

	public void NextMiss()
	{
		UpdateMissText(misses.AdvanceNext());
	}

	public void PreviousMiss()
	{
		UpdateMissText(misses.AdvancePrevious());
	}

	private void UpdateMissText(MissDisplay? display = null)
	{
		display ??= misses.Current;
		missText.text = display?.Text ?? "<i>No display.";
		missText.color = display?.Color ?? new Color32(0xFF, 0xFF, 0xFF, 0xAA);
	}
#endregion

	private enum JudgmentType
	{
		Normal,
		ChainHead
	}
}