using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace HitScoreVisualizer.Models;

[Serializable]
public class HsvConfigModel
{
	// Properties that are optional are marked as nullable. They can be ignored during serialization.
	// When a config object is made, all required fields will have to be provided with a value.

	public required ulong MajorVersion { get; set; } = Plugin.Metadata.HVersion.Major;
	public required ulong MinorVersion { get; set; } = Plugin.Metadata.HVersion.Minor;
	public required ulong PatchVersion { get; set; } = Plugin.Metadata.HVersion.Patch;

	public required string DisplayMode { get; set; }

	[Obsolete("Use the FixedPosition property instead.")] public bool UseFixedPos { get; set; }
	[Obsolete("Use the FixedPosition property instead.")] public float FixedPosX { get; set; }
	[Obsolete("Use the FixedPosition property instead.")] public float FixedPosY { get; set; }
	[Obsolete("Use the FixedPosition property instead.")] public float FixedPosZ { get; set; }

	public Vector3? FixedPosition { get; set; }
	public Vector3? TargetPositionOffset { get; set; }

	public required bool DoIntermediateUpdates { get; set; } = true;

	public required bool AssumeMaxPostSwing { get; set; }

	public required List<NormalJudgment> Judgments { get; set; }

	public required List<ChainHeadJudgment> ChainHeadJudgments { get; set; }
	public ChainLinkDisplay? ChainLinkDisplay { get; set; }

	public List<JudgmentSegment>? BeforeCutAngleJudgments { get; set; }
	public List<JudgmentSegment>? AccuracyJudgments { get; set; }
	public List<JudgmentSegment>? AfterCutAngleJudgments { get; set; }

	public int TimeDependenceDecimalPrecision { get; set; } = 1;
	public int TimeDependenceDecimalOffset { get; set; } = 2;
	public List<TimeDependenceJudgmentSegment>? TimeDependenceJudgments { get; set; }

	public bool RandomizeBadCutDisplays { get; set; } = true;
	public List<BadCutDisplay>? BadCutDisplays { get; set; }

	public bool RandomizeMissDisplays { get; set; } = true;
	public List<MissDisplay>? MissDisplays { get; set; }

	[JsonIgnore]
	internal static HsvConfigModel Default { get; } = new()
	{
		MajorVersion = Plugin.Metadata.HVersion.Major,
		MinorVersion = Plugin.Metadata.HVersion.Minor,
		PatchVersion = Plugin.Metadata.HVersion.Patch,
		DisplayMode = "format",
		DoIntermediateUpdates = true,
		AssumeMaxPostSwing = false,
		Judgments = [
			new()
			{
				Threshold = 115,
				Text = "<size=150%><u>%s</u></size>",
				Color = new(1f, 1f, 1f)
			},
			new()
			{
				Threshold = 110,
				Text = "%B<size=120%>%C%s</u></size>%A",
				Color = new(0f, 0.5f, 1.0f)
			},
			new()
			{
				Threshold = 105,
				Text = "%B%C%s</u>%A",
				Color = new(0f, 1f, 0f)
			},
			new()
			{
				Threshold = 100,
				Text = "%B%C%s</u>%A",
				Color = new(1f, 1f, 0f)
			},
			new()
			{
				Threshold = 50,
				Text = "%B<size=80%>%s</size>%A",
				Color = new(1f, 0f, 0f),
				Fade = true
			},
			new()
			{
				Threshold = 0,
				Text = "%B<size=80%>%s</size>%A",
				Color = new(1f, 0f, 0f)
			}
		],
		ChainHeadJudgments = [
			new()
			{
				Threshold = 85,
				Text = "<size=150%><u>%s</u></size>",
				Color = new(1f, 1f, 1f)
			},
			new()
			{
				Threshold = 80,
				Text = "%B<size=120%>%C%s</u></size>",
				Color = new(0f, 0.5f, 1.0f)
			},
			new()
			{
				Threshold = 75,
				Text = "%B%C%s</u>",
				Color = new(0f, 1f, 0f)
			},
			new()
			{
				Threshold = 70,
				Text = "%B%C%s</u>",
				Color = new(1f, 1f, 0f)
			},
			new()
			{
				Threshold = 35,
				Text = "%B<size=80%>%s</size>",
				Color = new(1f, 0f, 0f),
				Fade = true
			},
			new()
			{
				Threshold = 0,
				Text = "%B<size=80%>%s</size>",
				Color = new(1f, 0f, 0f)
			}
		],
		ChainLinkDisplay = new()
		{
			Text = "<alpha=#80><size=80%>%s",
			Color = new(1f, 1f, 1f)
		},
		BeforeCutAngleJudgments = [
			new()
			{
				Threshold = 70,
				Text = " + "
			},
			new()
			{
				Threshold = 0,
				Text = "<color=#ff4f4f> - </color>"
			}
		],
		AccuracyJudgments = [
			new()
			{
				Threshold = 15,
				Text = "<u>"
			},
			new()
			{
				Threshold = 0,
				Text = ""
			}
		],
		AfterCutAngleJudgments = [
			new()
			{
				Threshold = 30,
				Text = " + "
			},
			new()
			{
				Threshold = 0,
				Text = "<color=#ff4f4f> - </color>"
			}
		],
		TimeDependenceDecimalPrecision = 1,
		TimeDependenceDecimalOffset = 2,
		TimeDependenceJudgments = []
	};

	[JsonIgnore]
	internal static HsvConfigModel Vanilla { get; } = new()
	{
		MajorVersion = Plugin.Metadata.HVersion.Major,
		MinorVersion = Plugin.Metadata.HVersion.Minor,
		PatchVersion = Plugin.Metadata.HVersion.Patch,
		DisplayMode = "format",
		DoIntermediateUpdates = true,
		AssumeMaxPostSwing = false,
		Judgments =
		[
			new() { Threshold = 104, Text = "%C%s", Color = Color.white },
			new() { Threshold = 0, Text = "<alpha=#4C>%C%s", Color = Color.white }
		],
		ChainHeadJudgments =
		[
			new() { Threshold = 77, Text = "%C%s", Color = Color.white },
			new() { Threshold = 0, Text = "<alpha=#4C>%C%s", Color = Color.white }
		],
		ChainLinkDisplay = new() { Text = "<u>%s", Color = Color.white },
		AccuracyJudgments =
		[
			new() { Threshold = 15, Text = "<u>" }
		]
	};
}