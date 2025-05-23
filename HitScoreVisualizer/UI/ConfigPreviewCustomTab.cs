using System;
using System.Collections.Generic;
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
				allBadCuts = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All).ToList();
				wrongDirections = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All or BadCutDisplayType.WrongDirection).ToList();
				wrongColors = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All or BadCutDisplayType.WrongColor).ToList();
				bombs = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.All or BadCutDisplayType.Bomb).ToList();
			}

			if (config.MissDisplays is not null or [])
			{
				misses = config.MissDisplays;
			}
		}
		else
		{
			allBadCuts = [];
			wrongDirections = [];
			wrongColors = [];
			bombs = [];
			misses = [];
		}
		allIndex = 0;
		wrongDirectionIndex = 0;
		wrongColorIndex = 0;
		bombIndex = 0;
		missIndex = 0;
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

	private List<BadCutDisplay> allBadCuts = [];
	private int allIndex;
	private List<BadCutDisplay> wrongDirections = [];
	private int wrongDirectionIndex;
	private List<BadCutDisplay> wrongColors = [];
	private int wrongColorIndex;
	private List<BadCutDisplay> bombs = [];
	private int bombIndex;

	public void NextBadCut()
	{
		switch (BadCutType)
		{
			case BadCutDisplayType.All:
				allIndex = allIndex < allBadCuts.Count - 1 ? allIndex + 1 : 0;
				break;
			case BadCutDisplayType.WrongDirection:
				wrongDirectionIndex = wrongDirectionIndex < wrongDirections.Count - 1 ? wrongDirectionIndex + 1 : 0;
				break;
			case BadCutDisplayType.WrongColor:
				wrongColorIndex = wrongColorIndex < wrongColors.Count - 1 ? wrongColorIndex + 1 : 0;
				break;
			case BadCutDisplayType.Bomb:
				bombIndex = bombIndex < bombs.Count - 1 ? bombIndex + 1 : 0;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		UpdateBadCutText();
	}

	public void PreviousBadCut()
	{
		switch (BadCutType)
		{
			case BadCutDisplayType.All:
				allIndex = allIndex > 0 ? allIndex - 1 : allBadCuts.Count - 1;
				break;
			case BadCutDisplayType.WrongDirection:
				wrongDirectionIndex = wrongDirectionIndex > 0 ? wrongDirectionIndex - 1 : wrongDirections.Count - 1;
				break;
			case BadCutDisplayType.WrongColor:
				wrongColorIndex = wrongColorIndex > 0 ? wrongColorIndex - 1 : wrongColors.Count - 1;
				break;
			case BadCutDisplayType.Bomb:
				bombIndex = bombIndex > 0 ? bombIndex - 1 : bombs.Count - 1;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		UpdateBadCutText();
	}

	private void UpdateBadCutText()
	{
		var display = badCutType switch
		{
			BadCutDisplayType.All => allBadCuts.ElementAtOrDefault(allIndex),
			BadCutDisplayType.WrongDirection => wrongDirections.ElementAtOrDefault(wrongDirectionIndex),
			BadCutDisplayType.WrongColor => wrongColors.ElementAtOrDefault(wrongColorIndex),
			BadCutDisplayType.Bomb => bombs.ElementAtOrDefault(bombIndex),
			_ => throw new ArgumentOutOfRangeException()
		};
		badCutText.text = display?.Text ?? "<i>No display.";
		badCutText.color = display?.Color ?? new Color32(0xFF, 0xFF, 0xFF, 0xAA);
	}
#endregion

#region MissTab
	[UIComponent("MissText")] private readonly TextMeshProUGUI missText = null!;

	private List<MissDisplay> misses = [];
	private int missIndex;

	public void NextMiss()
	{
		missIndex = missIndex < misses.Count - 1 ? missIndex + 1 : 0;
		UpdateMissText();
	}

	public void PreviousMiss()
	{
		missIndex = missIndex > 0 ? missIndex - 1 : misses.Count - 1;
		UpdateMissText();
	}

	private void UpdateMissText()
	{
		var display = misses.ElementAtOrDefault(missIndex);
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