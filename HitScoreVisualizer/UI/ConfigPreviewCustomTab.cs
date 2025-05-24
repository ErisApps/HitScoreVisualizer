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

namespace HitScoreVisualizer.UI;

internal class ConfigPreviewCustomTab
{
	[Inject] private readonly PluginConfig pluginConfig = null!;
	[Inject] private readonly ConfigLoader configLoader = null!;

	[UIComponent("CustomPreviewText")] private readonly TextMeshProUGUI previewText = null!;
	// private TextMeshProUGUI[] texts = null!; // set in initializer

	private CustomPreviewTab currentTab = CustomPreviewTab.Judgments;
	private JudgmentType currentJudgmentType = JudgmentType.Normal;
	private BadCutDisplayType currentBadCutType = BadCutDisplayType.All;

	private float timeDependence;
	private int before = 70;
	private int center = 15;
	private int after = 30;

	private ItemRevolver<BadCutDisplay> allBadCuts = new();
	private ItemRevolver<BadCutDisplay> wrongDirections = new();
	private ItemRevolver<BadCutDisplay> wrongColors = new();
	private ItemRevolver<BadCutDisplay> bombs = new();
	private ItemRevolver<MissDisplay> misses = new();

	[UIAction("#post-parse")]
	public void PostParse()
	{
		previewText.enableAutoSizing = true;
		previewText.fontSizeMax = 5;
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

	public object EnumFormatter(Enum v)
	{
		return Regex.Replace(v.ToString(), "(\\B[A-Z])", " $1");
	}

	public Array JudgmentOptions { get; } = Enum.GetValues(typeof(JudgmentType));
	public Array BadCutTypeOptions { get; } = Enum.GetValues(typeof(BadCutDisplayType));

	public int Before
	{
		get => before;
		set
		{
			before = value;
			UpdateText();
		}
	}

	public int Center
	{
		get => center;
		set
		{
			center = value;
			UpdateText();
		}
	}

	public int After
	{
		get => after;
		set
		{
			after = value;
			UpdateText();
		}
	}

	public float TimeDependence
	{
		get => timeDependence;
		set
		{
			timeDependence = value;
			UpdateText();
		}
	}

	public JudgmentType JudgmentType
	{
		get => currentJudgmentType;
		set
		{
			currentJudgmentType = value;
			UpdateText();
		}
	}

	public BadCutDisplayType BadCutType
	{
		get => currentBadCutType;
		set
		{
			currentBadCutType = value;
			UpdateText();
		}
	}

	public void TabChanged(object segmentedControl, int idx)
	{
		currentTab = (CustomPreviewTab)idx;
		UpdateText();
	}

	public void NextBadCut()
	{
		CurrentBadCuts.AdvanceNext();
		UpdateText();
	}

	public void PreviousBadCut()
	{
		CurrentBadCuts.AdvancePrevious();
		UpdateText();
	}

	public void NextMiss()
	{
		misses.AdvanceNext();
		UpdateText();
	}

	public void PreviousMiss()
	{
		misses.AdvancePrevious();
		UpdateText();
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

		UpdateText();
	}

	private void UpdateText()
	{
		previewText.fontStyle = pluginConfig.DisableItalics ? FontStyles.Normal : FontStyles.Italic;
		(previewText.text, previewText.color) = currentTab switch
		{
			CustomPreviewTab.Judgments => GetJudgmentsText(),
			CustomPreviewTab.ChainLink => GetChainLinkText(),
			CustomPreviewTab.BadCut => GetBadCutText(),
			CustomPreviewTab.Miss => GetMissText(),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private ItemRevolver<BadCutDisplay> CurrentBadCuts => currentBadCutType switch
	{
		BadCutDisplayType.All => allBadCuts,
		BadCutDisplayType.WrongDirection => wrongDirections,
		BadCutDisplayType.WrongColor => wrongColors,
		BadCutDisplayType.Bomb => bombs,
		_ => throw new ArgumentOutOfRangeException()
	};

	private (string, Color) GetJudgmentsText()
	{
		var (afterCut, max, cutInfo) = currentJudgmentType switch
		{
			JudgmentType.Normal => (after, 115, DummyScores.Normal),
			JudgmentType.ChainHead => (0, 85, DummyScores.ChainHead),
			_ => throw new ArgumentOutOfRangeException()
		};
		return (pluginConfig.SelectedConfig?.Config ?? HsvConfigModel.Vanilla).Judge(new()
		{
			BeforeCutScore = before,
			CenterCutScore = center,
			AfterCutScore = afterCut,
			MaxPossibleScore = max,
			TotalCutScore = before + center + afterCut,
			CutInfo = cutInfo
		});
	}

	private (string, Color) GetChainLinkText()
	{
		return (pluginConfig.SelectedConfig?.Config ?? HsvConfigModel.Vanilla).Judge(new()
		{
			BeforeCutScore = 0,
			CenterCutScore = 0,
			AfterCutScore = 0,
			MaxPossibleScore = 20,
			TotalCutScore = 20,
			CutInfo = DummyScores.ChainLink
		});
	}

	private (string, Color) GetBadCutText(BadCutDisplay? display = null)
	{
		display ??= currentBadCutType switch
		{
			BadCutDisplayType.All => allBadCuts.Current,
			BadCutDisplayType.WrongDirection => wrongDirections.Current,
			BadCutDisplayType.WrongColor => wrongColors.Current,
			BadCutDisplayType.Bomb => bombs.Current,
			_ => throw new ArgumentOutOfRangeException()
		};
		var text = display?.Text ?? "<i>No display.";
		var color = display?.Color ?? new Color32(0xFF, 0xFF, 0xFF, 0xAA);
		return (text, color);
	}

	private (string, Color) GetMissText(MissDisplay? display = null)
	{
		display ??= misses.Current;
		var text = display?.Text ?? "<i>No display.";
		var color = display?.Color ?? new Color32(0xFF, 0xFF, 0xFF, 0xAA);
		return (text, color);
	}
}

internal enum JudgmentType
{
	Normal,
	ChainHead
}

internal enum CustomPreviewTab
{
	Judgments = 0,
	ChainLink = 1,
	BadCut = 2,
	Miss = 3
}