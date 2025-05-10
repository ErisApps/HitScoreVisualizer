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

	public required bool IsDefaultConfig { get; set; }

	public required string DisplayMode { get; set; }

	[Obsolete("Use the FixedPosition property instead.")] public bool UseFixedPos { get; set; }
	[Obsolete("Use the FixedPosition property instead.")] public float FixedPosX { get; set; }
	[Obsolete("Use the FixedPosition property instead.")] public float FixedPosY { get; set; }
	[Obsolete("Use the FixedPosition property instead.")] public float FixedPosZ { get; set; }

	public Vector3? FixedPosition { get; set; }
	public Vector3? TargetPositionOffset { get; set; }

	public required bool DoIntermediateUpdates { get; set; } = true;

	public required bool AssumeMaxPostSwing { get; set; }

	public int TimeDependenceDecimalPrecision { get; set; } = 1;
	public int TimeDependenceDecimalOffset { get; set; } = 2;

	public required List<NormalJudgment> Judgments { get; set; }
	public required List<ChainHeadJudgment> ChainHeadJudgments { get; set; }

	public ChainLinkDisplay? ChainLinkDisplay { get; set; }

	public List<JudgmentSegment>? BeforeCutAngleJudgments { get; set; }
	public List<JudgmentSegment>? AccuracyJudgments { get; set; }
	public List<JudgmentSegment>? AfterCutAngleJudgments { get; set; }

	public List<TimeDependenceJudgmentSegment>? TimeDependenceJudgments { get; set; }

	[JsonIgnore]
	internal static HsvConfigModel Default { get; } = new()
	{
		MajorVersion = Plugin.Metadata.HVersion.Major,
		MinorVersion = Plugin.Metadata.HVersion.Minor,
		PatchVersion = Plugin.Metadata.HVersion.Patch,
		IsDefaultConfig = true,
		DisplayMode = "format",
		DoIntermediateUpdates = true,
		AssumeMaxPostSwing = false,
		TimeDependenceDecimalPrecision = 1,
		TimeDependenceDecimalOffset = 2,
		Judgments =
		[
			new()
			{
				Threshold = 115,
				Text = "%BFantastic%A%n%s",
				Color = [1f, 1f, 1f, 1f]
			},
			new()
			{
				Threshold = 101,
				Text = "<size=80%>%BExcellent%A</size>%n%s",
				Color = [0.0f, 1.0f, 0.0f, 1.0f]
			},
			new()
			{
				Threshold = 90,
				Text = "<size=80%>%BGreat%A</size>%n%s",
				Color = [1.0f, 0.98f, 0.0f, 1.0f]
			},
			new()
			{
				Threshold = 80,
				Text = "<size=80%>%BGood%A</size>%n%s",
				Color = [1.0f, 0.6f, 0.0f, 1.0f],
				Fade = true
			},
			new()
			{
				Threshold = 60,
				Text = "<size=80%>%BDecent%A</size>%n%s",
				Color = [1.0f, 0.0f, 0.0f, 1.0f],
				Fade = true
			},
			new()
			{
				Threshold = 0,
				Text = "<size=80%>%BWay Off%A</size>%n%s",
				Color = [0.5f, 0.0f, 0.0f, 1.0f],
				Fade = true
			}
		],
		ChainHeadJudgments =
		[
			new()
			{
				Threshold = 85,
				Text = "%BFantastic%A%n%s",
				Color = [1f, 1f, 1f, 1f]
			},
			new()
			{
				Threshold = 71,
				Text = "<size=80%>%BExcellent%A</size>%n%s",
				Color = [0.0f, 1.0f, 0.0f, 1.0f]
			},
			new()
			{
				Threshold = 60,
				Text = "<size=80%>%BGreat%A</size>%n%s",
				Color = [1.0f, 0.98f, 0.0f, 1.0f]
			},
			new()
			{
				Threshold = 50,
				Text = "<size=80%>%BGood%A</size>%n%s",
				Color = [1.0f, 0.6f, 0.0f, 1.0f],
				Fade = true
			},
			new()
			{
				Threshold = 30,
				Text = "<size=80%>%BDecent%A</size>%n%s",
				Color = [1.0f, 0.0f, 0.0f, 1.0f],
				Fade = true
			},
			new()
			{
				Threshold = 0,
				Text = "<size=80%>%BWay Off%A</size>%n%s",
				Color = [0.5f, 0.0f, 0.0f, 1.0f],
				Fade = true
			}
		],
		BeforeCutAngleJudgments =
		[
			new()
			{
				Threshold = 70,
				Text = " + "
			},
			new()
			{
				Threshold = 0,
				Text = " "
			}
		],
		AccuracyJudgments =
		[
			new()
			{
				Threshold = 15,
				Text = " + "
			},
			new()
			{
				Threshold = 0,
				Text = " "
			}
		],
		AfterCutAngleJudgments =
		[
			new()
			{
				Threshold = 30,
				Text = " + "
			},
			new()
			{
				Threshold = 0,
				Text = " "
			}
		]
	};
}