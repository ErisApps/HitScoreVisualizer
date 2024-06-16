using System;
using System.Collections.Generic;
using System.ComponentModel;
using HitScoreVisualizer.Helpers.Json;
using Newtonsoft.Json;
using UnityEngine;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Settings
{
	public class Configuration
	{
		[JsonIgnore]
		internal static Configuration Default { get; } = new Configuration
		{
			Version = Plugin.Version,
			IsDefaultConfig = true,
			DisplayMode = "format",
			DoIntermediateUpdates = true,
			TimeDependenceDecimalPrecision = 1,
			TimeDependenceDecimalOffset = 2,
			NormalJudgments =
			[
				new(115, "%BFantastic%A%n%s", [1.0f, 1.0f, 1.0f, 1.0f]),
				new(101, "<size=80%>%BExcellent%A</size>%n%s", [0.0f, 1.0f, 0.0f, 1.0f]),
				new(90, "<size=80%>%BGreat%A</size>%n%s", [1.0f, 0.980392158f, 0.0f, 1.0f]),
				new(80, "<size=80%>%BGood%A</size>%n%s", [1.0f, 0.6f, 0.0f, 1.0f], true),
				new(60, "<size=80%>%BDecent%A</size>%n%s", [1.0f, 0.0f, 0.0f, 1.0f], true),
				new(0, "<size=80%>%BWay Off%A</size>%n%s", [0.5f, 0.0f, 0.0f, 1.0f], true)
			],
			ChainHeadJudgments =
			[
				new(115, "%BFantastic%n%s", [1.0f, 1.0f, 1.0f, 1.0f]),
				new(101, "<size=80%>%BExcellent</size>%n%s", [0.0f, 1.0f, 0.0f, 1.0f]),
				new(90, "<size=80%>%BGreat</size>%n%s", [1.0f, 0.980392158f, 0.0f, 1.0f]),
				new(80, "<size=80%>%BGood</size>%n%s", [1.0f, 0.6f, 0.0f, 1.0f], true),
				new(60, "<size=80%>%BDecent</size>%n%s", [1.0f, 0.0f, 0.0f, 1.0f], true),
				new(0, "<size=80%>%BWay Off</size>%n%s", [0.5f, 0.0f, 0.0f, 1.0f], true)
			],
			BeforeCutAngleJudgments =
			[
				new() { Threshold = 70, Text = "+" },
				new() { Text = " " }
			],
			AccuracyJudgments =
			[
				new() { Threshold = 15, Text = " + " },
				new() { Text = " " }
			],
			AfterCutAngleJudgments =
			[
				new() { Threshold = 30, Text = " + " },
				new() { Text = " " }
			]
		};

		// If the version number (excluding patch version) of the config is higher than that of the plugin,
		// the config will not be loaded. If the version number of the config is lower than that of the
		// plugin, the file will be automatically converted. Conversion is not guaranteed to occur, or be
		// accurate, across major versions.
		[JsonProperty("majorVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public ulong MajorVersion { get; private set; } = Plugin.Version.Major;

		[JsonProperty("minorVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public ulong MinorVersion { get; private set; } = Plugin.Version.Minor;

		[JsonProperty("patchVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public ulong PatchVersion { get; private set; } = Plugin.Version.Patch;

		[JsonIgnore]
		internal Version Version
		{
			get => new(MajorVersion, MinorVersion, PatchVersion);
			set
			{
				MajorVersion = value.Major;
				MinorVersion = value.Minor;
				PatchVersion = value.Patch;
			}
		}

		// If this is true, the config will be overwritten with the plugin' default settings after an
		// update rather than being converted.
		[JsonProperty("isDefaultConfig")]
		public bool IsDefaultConfig { get; internal set; }

		// If set to "format", displays the judgment text, with the following format specifiers allowed:
		// - %b: The score contributed by the part of the swing before cutting the block.
		// - %c: The score contributed by the accuracy of the cut.
		// - %a: The score contributed by the part of the swing after cutting the block.
		// - %t: The time dependence of the swing
		// - %B, %C, %A, %T: As above, except using the appropriate judgment from that part of the swing (as configured for "beforeCutAngleJudgments", "accuracyJudgments", "afterCutAngleJudgments", or "timeDependencyJudgments").
		// - %s: The total score for the cut.
		// - %p: The percent out of 115 you achieved with your swing's score
		// - %%: A literal percent symbol.
		// - %n: A newline.
		//
		// If set to "numeric", displays only the note score.
		// If set to "textOnly", displays only the judgment text.
		// If set to "scoreOnTop", displays both (numeric score above judgment text).
		// Otherwise, displays both (judgment text above numeric score).
		[JsonProperty("displayMode")]
		[DefaultValue("")]
		public string DisplayMode { get; internal set; } = string.Empty;

		[JsonProperty("useFixedPos")]
		[DefaultValue(false)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Either use the FixedPosition property or leave as null instead.")]
		public bool UseFixedPos { get; internal set; }

		[JsonProperty("fixedPosX")]
		[DefaultValue(0f)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Use the FixedPosition property instead.")]
		public float FixedPosX { get; internal set; }

		[JsonProperty("fixedPosY")]
		[DefaultValue(0f)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Use the FixedPosition property instead.")]
		public float FixedPosY { get; internal set; }

		[JsonProperty("fixedPosZ")]
		[DefaultValue(0f)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Use the FixedPosition property instead.")]
		public float FixedPosZ { get; internal set; }

		// If not null, judgments will appear and stay at rather than moving as normal, this will take priority over TargetPositionOffset.
		// Additionally, the previous judgment will disappear when a new one is created (so there won't be overlap).
		// Format changed to nullable Vector3 since version 3.2.0
		[JsonProperty("fixedPosition", NullValueHandling = NullValueHandling.Ignore)]
		public Vector3? FixedPosition { get; set; }

		// Will offset the target position of the hitscore fade animation.
		// If a fixed position is defined in the config, that one will take priority over this one and this will be fully ignored.
		[JsonProperty("targetPositionOffset", NullValueHandling = NullValueHandling.Ignore)]
		public Vector3? TargetPositionOffset { get; set; }

		// If enabled, judgments will be updated more frequently. This will make score popups more accurate during a brief period before the note's score is finalized, at some cost of performance.
		[JsonProperty("doIntermediateUpdates")]
		public bool DoIntermediateUpdates { get; internal set; }

		// Number of decimal places to show time dependence to
		[JsonProperty("timeDependencyDecimalPrecision")]
		[DefaultValue(1)]
		public int TimeDependenceDecimalPrecision { get; internal set; }

		// Which power of 10 to multiply the time dependence by
		[JsonProperty("timeDependencyDecimalOffset")]
		[DefaultValue(2)]
		public int TimeDependenceDecimalOffset { get; internal set; }

		// Order from highest threshold to lowest; the first matching judgment will be applied
		[JsonProperty("judgments")]
		public List<NormalJudgment>? NormalJudgments { get; internal set; }

		// Same as normal judgments but for burst sliders aka. chain notes
		[JsonProperty("chainHeadJudgments")]
		public List<ChainHeadJudgment>? ChainHeadJudgments { get; internal set; }

		// Text displayed for burst slider segments
		[JsonProperty("chainLinkDisplay")]
		public ChainLinkDisplay? ChainLinkDisplay { get; internal set; }

		// Judgments for the part of the swing before cutting the block (score is from 0-70).
		// Format specifier: %B
		[JsonProperty("beforeCutAngleJudgments")]
		public List<JudgmentSegment>? BeforeCutAngleJudgments { get; internal set; }


		// Judgments for the accuracy of the cut (how close to the center of the block the cut was, score is from 0-15).
		// Format specifier: %C
		[JsonProperty("accuracyJudgments")]
		public List<JudgmentSegment>? AccuracyJudgments { get; internal set; }

		// Judgments for the part of the swing after cutting the block (score is from 0-30).
		// Format specifier: %A
		[JsonProperty("afterCutAngleJudgments")]
		public List<JudgmentSegment>? AfterCutAngleJudgments { get; internal set; }

		// Judgments for time dependence (score is from 0-1).
		// Format specifier: %T
		[JsonProperty("timeDependencyJudgments")]
		public List<TimeDependenceJudgmentSegment>? TimeDependenceJudgments { get; internal set; }
	}
}